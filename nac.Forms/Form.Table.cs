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
using nac.Forms.model;

namespace nac.Forms;

public partial class Form
{
    public Form Table<T>(string itemsModelFieldName,
                                IEnumerable<model.Column> columns = null,
                                bool autoGenerateColumns = true,
                                Action<List<Avalonia.Controls.TableViewRow>> onVisibleRowsChanged = null)
        {
            var dg = new Avalonia.Controls.TableView();

            if (autoGenerateColumns == true)
            {
                if (getModelValue(itemsModelFieldName)?.Value is IEnumerable<nac.utilities.BindableDynamicDictionary>
                    dictList)
                {
                    // special case for the columns in our special dictionary
                    columns = generateColumnsForBindableDynamicDictionary(columns, dictList);
                }
                else
                {
                    // generate columns for all the T
                    // The way the DataGrid worked it just tacked them onto the end
                    columns = populateColumnsListWithPropertyColumns<T>(columns);
                }

            }

            if (onVisibleRowsChanged != null)
            {
                var observer = new repos.DataGridVisibleRowsObserverRepo(dataGrid: dg, onVisibleRowsChanged: onVisibleRowsChanged);
                observer.Setup();
            }
            
            

            if (columns != null)
            {
                foreach (var c in columns)
                {
                    if (c.template == null)
                    {
                        var dgCol = new Avalonia.Controls.TableViewColumn();
                        dgCol.Header = c.Header;
                        dgCol.Binding = new Binding
                        {
                            Path = c.modelBindingPropertyName
                        };
                        dg.Columns.Add(dgCol);
                    }
                    else
                    {
                        var col = new Avalonia.Controls.TableViewColumn();
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
            AddBinding<IEnumerable>(itemsModelFieldName, dg, Avalonia.Controls.TableView.ItemsSourceProperty, 
                isTwoWayDataBinding: false);
            AddRowToHost(dg, rowAutoHeight: false);

            return this;
        }

    private List<Column> populateColumnsListWithPropertyColumns<T>(IEnumerable<Column> columns)
    {
        var resultList = new List<Column>();
        if (columns != null)
        {
            resultList.AddRange(columns);
        }
        
        var targetProperties = typeof(T).GetProperties()
            .Where(prop => prop.CanRead &&
                           prop.GetIndexParameters().Length == 0
            )
            .ToList();

        foreach (var prop in targetProperties)
        {
            resultList.Add(new Column
            {
                Header = prop.Name,
                modelBindingPropertyName = prop.Name
            });
        }

        return resultList;
    }


    private IEnumerable<model.Column> generateColumnsForBindableDynamicDictionary(IEnumerable<Column> columns, IEnumerable<nac.utilities.BindableDynamicDictionary> dictList)
        {
            var dictColumns = new List<model.Column>();

            if (columns != null)
            {
                dictColumns.AddRange(columns);
            }

            var firstDict = dictList.FirstOrDefault();

            if (firstDict == null)
            {
                throw new Exception(
                    "If using BindableDynamicDictionary for the type of the list items, you start with 1 item in the list, because there is no other way to figure out the type");
            }

            foreach (var key in firstDict.GetDynamicMemberNames())
            {
                /*
                 4/30/2024 - Changed this to be a Template column because the DataGridTextColumn code creates a TextBlock but doesn't seem to bind to it in a way that this would work
                           + This makes things alot simpler by just controlling the binding
                           + See the DataGridTextColumn code here: https://github.com/AvaloniaUI/Avalonia/blob/334a8f7d0c947eb535f2ad7accc914e36727f334/src/Avalonia.Controls.DataGrid/DataGridTextColumn.cs#L187
                           + 
                 */
                dictColumns.Add(new model.Column
                {
                    Header = key,
                    template = f=> f.TextFor(modelFieldName: key)
                });
            }

            return dictColumns;
        }
    
}