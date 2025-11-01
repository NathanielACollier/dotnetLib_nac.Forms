using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Reactive;
using Avalonia.VisualTree;

namespace nac.Forms.repos;

public class DataGridVisibleRowsObserverRepo
{
    private static nac.Logging.Logger log = new();
    
    public DataGrid DataGrid { get; set; }
    public Action<List<DataGridRow>> OnVisibleRowsChanged { get; set; }
    public ScrollViewer ScrollViewer { get; set; }
    public AnonymousObserver<Vector> ScrollOffsetPropertyObserver { get; set; }
    public AnonymousObserver<Size> ScrollViewPortChangedObserver { get; set; }
    
    
    public DataGridVisibleRowsObserverRepo(Avalonia.Controls.DataGrid dataGrid,
        Action<List<Avalonia.Controls.DataGridRow>> onVisibleRowsChanged)
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
                this.ScrollViewer = this.DataGrid.FindDescendantOfType<Avalonia.Controls.ScrollViewer>();
                SetupAfterScrollViewerAvailable();
            }, Avalonia.Threading.DispatcherPriority.Loaded);
            
        };
        
    }

    

    private void SetupAfterScrollViewerAvailable()
    {
        // track scrolling and viewport changes
        // scrolling changes
        this.ScrollOffsetPropertyObserver = new AnonymousObserver<Vector>(offset =>
        {
            SetupHandlingOnVisibleRowsChanged_FireOnVisibleRowsChanged();
        });
        
        
        this.ScrollViewer.GetObservable(ScrollViewer.OffsetProperty)
            .Subscribe(this.ScrollOffsetPropertyObserver);
        
        // viewport changes
        this.ScrollViewPortChangedObserver = new AnonymousObserver<Size>(viewPort =>
        {
            SetupHandlingOnVisibleRowsChanged_FireOnVisibleRowsChanged();
        });
        
        this.ScrollViewer.GetObservable(ScrollViewer.ViewportProperty)
            .Subscribe(this.ScrollViewPortChangedObserver);
    }

    private void SetupHandlingOnVisibleRowsChanged_FireOnVisibleRowsChanged()
    {
        var offset = this.ScrollViewer.Offset;
        var viewport = this.ScrollViewer.Viewport;
        log.Info($"Scroll offset: {offset.Y}, Viewport height: {viewport.Height}");
            
        var rowPresenter = this.DataGrid.GetVisualDescendants()
            .OfType<Avalonia.Controls.Primitives.DataGridRowsPresenter>()
            .FirstOrDefault();

        var visibleRows = rowPresenter?.Children
            .OfType<DataGridRow>()
            .Where(row => row.IsVisible)
            .ToList();
            
        this.OnVisibleRowsChanged.Invoke(visibleRows);
    }
    
    
    
}