using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Reactive;
using Avalonia.VisualTree;

namespace nac.Forms.repos;

public class TableViewVisibleRowsObserverRepo
{
    private static nac.Logging.Logger log = new();
    
    public TableView DataGrid { get; set; }
    public Action<List<TableViewRow>> OnVisibleRowsChanged { get; set; }
    
    public TableViewVisibleRowsObserverRepo(Avalonia.Controls.TableView dataGrid,
        Action<List<Avalonia.Controls.TableViewRow>> onVisibleRowsChanged)
    {
        this.DataGrid = dataGrid;
        this.OnVisibleRowsChanged = onVisibleRowsChanged;
    }


    public void Setup()
    {
        this.SetupHandlingOnVisibleRowsChanged();
    }
    
    private void SetupHandlingOnVisibleRowsChanged()
    {
        // have to wait for DataGrid to be attached to visual tree
        this.DataGrid.AttachedToVisualTree += (_s, _args) =>
        {
            // Need this UIThread.Post because sometimes the scrollviewer still isn't available after AttachedToVisualTree
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {

                SetupAfterScrollViewerAvailable();
            }, Avalonia.Threading.DispatcherPriority.Loaded);
            
        };
        
    }

    

    private void SetupAfterScrollViewerAvailable()
    {
        // track scrolling and viewport changes

        this.DataGrid.LayoutUpdated += (_s, _args) =>
        {
            SetupHandlingOnVisibleRowsChanged_FireOnVisibleRowsChanged();
        };
    }

    private void SetupHandlingOnVisibleRowsChanged_FireOnVisibleRowsChanged()
    {
        var visibleRows = this.DataGrid.GetVisualDescendants()
            .OfType<TableViewRow>()
            .Where(row => row.IsVisible)
            .ToList();

        if (visibleRows == null)
        {
            return; // skip if rowpresenter was null and resulted in no visible children
        }
            
        this.OnVisibleRowsChanged.Invoke(visibleRows);
    }
    
    
    
}