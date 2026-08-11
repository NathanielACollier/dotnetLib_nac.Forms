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
            var dg = new Avalonia.Controls.TableView();
            lib.styleUtil.style(this, dg, style);
            
            MonitorForUIReady_TableView_ThenMakeAlterationsBasedOnStyle(style, dg);

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
                var observer = new repos.TableViewVisibleRowsObserverRepo(dataGrid: dg, onVisibleRowsChanged: onVisibleRowsChanged);
                observer.Setup();
            }
            
            

            if (columns != null)
            {
                foreach (var c in columns)
                {
                    if (c.template == null)
                    {
                        var dgCol = CreateMinimumTableViewColumn(c, style: style);
                        dgCol.Binding = new Binding
                        {
                            Path = c.modelBindingPropertyName
                        };
                        dg.Columns.Add(dgCol);
                    }
                    else
                    {
                        var col = CreateMinimumTableViewColumn(c, style: style);
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

    private static TableViewColumn CreateMinimumTableViewColumn(Column colModel, Style style)
    {
        var dgCol = new Avalonia.Controls.TableViewColumn();
        dgCol.Header = colModel.Header;
        //dgCol.Width = new GridLength(1.0, GridUnitType.Pixel);
        return dgCol;
    }

    private static void MonitorForUIReady_TableView_ThenMakeAlterationsBasedOnStyle(Style style, TableView dg)
    {
        // The old DataGrid showed a horizontal scrollbar, and had all the columns just be the size they needed
        //   If the person chooses to set a width in style we are going to turn on horizontal scroll
        dg.AttachedToVisualTree += (_, _) =>
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                var sv = dg.FindDescendantOfType<ScrollViewer>();
                sv.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;

                string debugText = $@"
                    sv.Extent.Width = {sv.Extent.Width}
                    sv.Viewport.Width = {sv.Viewport.Width}
                    dg.Bounds.Width = {dg.Bounds.Width}
                    dg.Columns.Sum(x => x.ActualWidth) = {dg.Columns.Sum(x => x.ActualWidth)}
                ";

                if (style.width.IsSet)
                {
                    sv.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
                }
            }, Avalonia.Threading.DispatcherPriority.Render);

        };
    }

    
    
}