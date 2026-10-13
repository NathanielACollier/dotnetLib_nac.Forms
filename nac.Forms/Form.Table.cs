using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Reactive;
using Avalonia.VisualTree;
using nac.Forms.lib;

namespace nac.Forms;

public partial class Form
{
    public Form Table<T>(string itemsModelFieldName,
        IEnumerable<model.Column> columns = null,
        bool autoGenerateColumns = true,
        Action<List<Avalonia.Controls.DataGridRow>> onVisibleRowsChanged = null)
    {
        if (!isDataGridStyleInApp(app))
        {
            addDataGridStyleToApp(app);
        }

        var dg = new Avalonia.Controls.DataGrid();
        dg.AutoGenerateColumns = autoGenerateColumns;

        if (onVisibleRowsChanged != null)
        {
            var observer =
                new repos.DataGridVisibleRowsObserverRepo(dataGrid: dg, onVisibleRowsChanged: onVisibleRowsChanged);
            observer.Setup();
        }

        // special case for the columns in our special dictionary
        if (autoGenerateColumns == true &&
            getModelValue(itemsModelFieldName)?.Value is IEnumerable<nac.utilities.BindableDynamicDictionary> dictList)
        {
            dg.AutoGenerateColumns = false; // we are going to generate our own columns
            columns = repos.TableColumnModelWorkRepo.generateColumnsForBindableDynamicDictionary(columns: [], dictList: dictList);
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
                        var rowForm = new Form(__app: this.app, _model: new nac.utilities.BindableDynamicDictionary());

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

        if (!(getModelValue(itemsModelFieldName)?.Value is IEnumerable<T>))
        {
            throw new Exception(
                $"Model Items source property specified by name [{itemsModelFieldName}] must be IEnumerable<T>");
        }

        /*
         NOTE: two way data binding for ItemsSource should allways be false
            - If it's set to true then it requires a setter for the property and can crash.  Often for an ItemsSource on the model it will just have a getter and use the auto creation functionality of ViewModelBase
         */
        AddBinding<IEnumerable>(itemsModelFieldName, dg, Avalonia.Controls.DataGrid.ItemsSourceProperty,
            isTwoWayDataBinding: false);
        AddRowToHost(dg, rowAutoHeight: false);

        return this;
    }
    

    private void addDataGridStyleToApp(Application app)
    {
        // there is a bug in avalonia.  see: https://github.com/AvaloniaUI/Avalonia/issues/3788
        var datagridStyleUri = new Uri("avares://Avalonia.Controls.DataGrid/Themes/Simple.xaml");
        var _style = new StyleInclude(datagridStyleUri)
        {
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