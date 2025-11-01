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
                SetupHandlingOnVisibleRowsChanged(dg: dg, 
                                onVisibleRowsChanged: onVisibleRowsChanged);
            }
            
            // special case for the columns in our special dictionary
            if ( autoGenerateColumns == true && getModelValue(itemsModelFieldName)?.Value is IEnumerable<nac.utilities.BindableDynamicDictionary> dictList)
            {
                dg.AutoGenerateColumns = false; // we are going to generate our own columns
                var newColumns = new List<model.Column>();
                newColumns.AddRange(
                    generateColumnsForBindableDynamicDictionary(dictList)
                    );
                if (columns != null)
                {
                    newColumns.AddRange(columns);
                }
                
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

    private void SetupHandlingOnVisibleRowsChanged(DataGrid dg, Action<List<DataGridRow>> onVisibleRowsChanged)
    {
        // have to wait for DataGrid to be attached to visual tree
        dg.AttachedToVisualTree += (_s, _args) =>
        {
            // Need this UIThread.Post because sometimes the scrollviewer still isn't available after AttachedToVisualTree
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                SetupHandlingOnVisibleRowsChanged_VisualTreeAttachReady(dg: dg, onVisibleRowsChanged: onVisibleRowsChanged);
            }, Avalonia.Threading.DispatcherPriority.Loaded);
            
        };
        
    }

    private void SetupHandlingOnVisibleRowsChanged_VisualTreeAttachReady(DataGrid dg, Action<List<DataGridRow>> onVisibleRowsChanged)
    {
        var scrollViewer = dg.FindDescendantOfType<Avalonia.Controls.ScrollViewer>();
        
        // track scrolling and viewport changes
        
        // scrolling changes
        scrollViewer.GetObservable(ScrollViewer.OffsetProperty)
            .Subscribe(new AnonymousObserver<Vector>(offset =>
            {
                SetupHandlingOnVisibleRowsChanged_FireOnVisibleRowsChanged(dg: dg, 
                    onVisibleRowsChanged: onVisibleRowsChanged,
                    scrollViewer: scrollViewer);
            }) );
        
        // viewport changes
        scrollViewer.GetObservable(ScrollViewer.ViewportProperty)
            .Subscribe(new AnonymousObserver<Size>(viewport =>
            {
                SetupHandlingOnVisibleRowsChanged_FireOnVisibleRowsChanged(dg: dg,
                    onVisibleRowsChanged: onVisibleRowsChanged,
                    scrollViewer: scrollViewer);
            }));
        
    }

    private void SetupHandlingOnVisibleRowsChanged_FireOnVisibleRowsChanged(DataGrid dg, 
                                                            Action<List<DataGridRow>> onVisibleRowsChanged,
                                                            Avalonia.Controls.ScrollViewer scrollViewer)
    {
        var offset = scrollViewer.Offset;
        var viewport = scrollViewer.Viewport;
        log.Info($"Scroll offset: {offset.Y}, Viewport height: {viewport.Height}");
            
        var rowPresenter = dg.GetVisualDescendants()
            .OfType<Avalonia.Controls.Primitives.DataGridRowsPresenter>()
            .FirstOrDefault();

        var visibleRows = rowPresenter?.Children
            .OfType<DataGridRow>()
            .Where(row => row.IsVisible)
            .ToList();
            
        onVisibleRowsChanged.Invoke(visibleRows);
    }
    


    private IEnumerable<model.Column> generateColumnsForBindableDynamicDictionary(IEnumerable<nac.utilities.BindableDynamicDictionary> dictList)
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

        private void addDataGridStyleToApp(Application app)
        {
            // there is a bug in avalonia.  see: https://github.com/AvaloniaUI/Avalonia/issues/3788
            var datagridStyleUri = new Uri("avares://Avalonia.Controls.DataGrid/Themes/Simple.xaml");
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