using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Reactive;
using Avalonia.VisualTree;
using nac.Forms.lib;
using nac.Forms.model;

namespace nac.Forms;

public partial class Form
{
    public Form TableView<T>(string itemsModelFieldName,
                                IEnumerable<model.Column> columns = null,
                                bool autoGenerateColumns = true,
                                Action<List<Avalonia.Controls.TableViewRow>> onVisibleRowsChanged = null,
                                Style style=null)
        {
            var tv = new Avalonia.Controls.TableView();

            if (autoGenerateColumns == true)
            {
                if (getModelValue(itemsModelFieldName)?.Value is IEnumerable<nac.utilities.BindableDynamicDictionary>
                    dictList)
                {
                    // special case for the columns in our special dictionary
                    columns = repos.TableColumnModelWorkRepo.generateColumnsForBindableDynamicDictionary(columns, dictList);
                }
                else
                {
                    // generate columns for all the T
                    // The way the DataGrid worked it just tacked them onto the end
                    columns = repos.TableColumnModelWorkRepo.populateColumnsListWithPropertyColumns<T>(columns);
                }

            }

            if (onVisibleRowsChanged != null)
            {
                var observer = new repos.TableViewVisibleRowsObserverRepo(dataGrid: tv, onVisibleRowsChanged: onVisibleRowsChanged);
                observer.Setup();
            }
            
            

            if (columns != null)
            {
                foreach (var c in columns)
                {
                    if (c.template == null)
                    {
                        var dgCol = TableView_CreateMinimalColumn(c, style: style, columnCount: columns.Count());
                        dgCol.Binding = new Binding
                        {
                            Path = c.modelBindingPropertyName
                        };
                        tv.Columns.Add(dgCol);
                    }
                    else
                    {
                        var col = TableView_CreateMinimalColumn(c, style: style, columnCount: columns.Count());
                        col.CellTemplate = new FuncDataTemplate<object>((itemModel, nameScope) =>
                        {
                            var rowForm = new Form(__app: this.app, _model: new nac.utilities.BindableDynamicDictionary());
                            
                            // this has to have a unique model
                            rowForm.DataContext = itemModel;
                            c.template(rowForm);

                            rowForm.Host.DataContext = itemModel;

                            return rowForm.Host;
                        });
                        tv.Columns.Add(col);
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
            AddBinding<IEnumerable>(itemsModelFieldName, tv, Avalonia.Controls.TableView.ItemsSourceProperty, 
                isTwoWayDataBinding: false);
            AddRowToHost(tv, rowAutoHeight: false);

            return this;
        }

    private static TableViewColumn TableView_CreateMinimalColumn(Column colModel, Style style, int columnCount)
    {
        var dgCol = new Avalonia.Controls.TableViewColumn();
        dgCol.Header = colModel.Header;

        if (style.width.IsSet == true)
        {
            double colWidth = (double)style.width.Value / (double)columnCount;
            dgCol.Width = new GridLength( colWidth, GridUnitType.Pixel);
        }
        
        return dgCol;
    }
    

    
    
}