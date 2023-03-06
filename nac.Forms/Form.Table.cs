using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Markup.Xaml.Styling;
using nac.Forms.lib;

namespace nac.Forms;

public partial class Form
{
    
    public Form Table<T>(string itemsModelFieldName,
                                IEnumerable<model.Column> columns = null,
                                bool autoGenerateColumns = true)
        {
            if (!isDataGridStyleInApp(app))
            {
                addDataGridStyleToApp(app);
            }
            
            var dg = new Avalonia.Controls.DataGrid();
            dg.AutoGenerateColumns = autoGenerateColumns;
            
            // special case for the columns in our special dictionary
            if ( autoGenerateColumns == true && getModelValue(itemsModelFieldName) is IEnumerable<nac.Forms.lib.BindableDynamicDictionary> dictList)
            {
                dg.AutoGenerateColumns = false; // we are going to generate our own columns
                var newColumns = new List<model.Column>();
                newColumns.AddRange(
                    generateColumnsForBindableDynamicDictionary(dictList)
                    );
                newColumns.AddRange(columns);
                columns = newColumns;
            }

            if (columns != null)
            {
                foreach (var c in columns)
                {
                    if (c.template == null)
                    {
                        var dgCol = new Avalonia.Controls.DataGridTextColumn();
                        dgCol.Header = c.Header;
                        dgCol.Binding = new Binding
                        {
                            Path = c.modelBindingPropertyName
                        };
                        dg.Columns.Add(dgCol);
                    }
                    else
                    {
                        var col = new Avalonia.Controls.DataGridTemplateColumn();
                        col.Header = c.Header;
                        col.CellTemplate = new FuncDataTemplate<object>((itemModel, nameScope) =>
                        {
                            var rowForm = new Form(__app: this.app, _model: new lib.BindableDynamicDictionary());
                            
                            // this has to have a unique model
                            rowForm.DataContext = itemModel;
                            c.template(rowForm);

                            rowForm.Host.DataContext = itemModel;

                            return rowForm.Host;
                        });
                        dg.Columns.Add(col);
                    }
                }
            }

            if (!(getModelValue(itemsModelFieldName) is IEnumerable<T>))
            {
                throw new Exception(
                    $"Model Items source property specified by name [{itemsModelFieldName}] must be IEnumerable<T>");
            }
            
            /*
             NOTE: two way data binding for ItemsSource should allways be false
                - If it's set to true then it requires a setter for the property and can crash.  Often for an ItemsSource on the model it will just have a getter and use the auto creation functionality of ViewModelBase
             */
            AddBinding<IEnumerable>(itemsModelFieldName, dg, Avalonia.Controls.DataGrid.ItemsProperty, 
                isTwoWayDataBinding: false);
            AddRowToHost(dg, rowAutoHeight: false);

            return this;
        }

        private IEnumerable<model.Column> generateColumnsForBindableDynamicDictionary(IEnumerable<BindableDynamicDictionary> dictList)
        {
            var dictColumns = new List<model.Column>();

            var firstDict = dictList.FirstOrDefault();

            if (firstDict == null)
            {
                throw new Exception(
                    "If using BindableDynamicDictionary for the type of the list items, you start with 1 item in the list, because there is no other way to figure out the type");
            }

            foreach (var key in firstDict.GetDynamicMemberNames())
            {
                dictColumns.Add(new model.Column
                {
                    Header = key,
                    modelBindingPropertyName = key
                });
            }

            return dictColumns;
        }

        private void addDataGridStyleToApp(Application app)
        {
            // there is a bug in avalonia.  see: https://github.com/AvaloniaUI/Avalonia/issues/3788
            var datagridStyleUri = new Uri("avares://Avalonia.Controls.DataGrid/Themes/Default.xaml");
            var _style = new StyleInclude(datagridStyleUri) {
                Source = datagridStyleUri
            };
            app.Styles.Add(_style);
        }

        private bool isDataGridStyleInApp(Application app)
        {
            var datagridStyleQuery = app.Styles
                .OfType<StyleInclude>()
                .Where(s => (s?.Source?.ToString() ?? "")
                            .IndexOf("/Avalonia.Controls.DataGrid/", StringComparison.OrdinalIgnoreCase) >=
                            0);

            return datagridStyleQuery.Any();
        }
}