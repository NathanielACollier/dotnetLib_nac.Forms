using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Reactive;
using Avalonia.VisualTree;

namespace nac.Forms.repos;

public class DataGridVisibleRowsObserverRepo
{
    private static nac.Logging.Logger log = new();
    
    public DataGrid DataGrid { get; set; }
    public Action<List<DataGridRow>> OnVisibleRowsChanged { get; set; }
    public Avalonia.Controls.Presenters.ScrollContentPresenter ScrollViewerPresenter { get; set; }
    
    
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
                var scrollViewer = this.DataGrid.FindDescendantOfType<Avalonia.Controls.ScrollViewer>();
                this.ScrollViewerPresenter = scrollViewer.FindDescendantOfType<Avalonia.Controls.Presenters.ScrollContentPresenter>();
                SetupAfterScrollViewerAvailable();
            }, Avalonia.Threading.DispatcherPriority.Loaded);
            
        };
        
    }

    

    private void SetupAfterScrollViewerAvailable()
    {
        // track scrolling and viewport changes
        // scrolling changes
        this.ScrollViewerPresenter.GetObservable(Avalonia.Controls.Presenters.ScrollContentPresenter.OffsetProperty)
            .Subscribe(new AnonymousObserver<Vector>(offset =>
            {
                SetupHandlingOnVisibleRowsChanged_FireOnVisibleRowsChanged();
            }));
        
        // viewport changes
        this.ScrollViewerPresenter.GetObservable(Avalonia.Controls.Presenters.ScrollContentPresenter.ViewportProperty)
            .Subscribe(new AnonymousObserver<Size>(viewPort =>
            {
                SetupHandlingOnVisibleRowsChanged_FireOnVisibleRowsChanged();
            }));
    }

    private void SetupHandlingOnVisibleRowsChanged_FireOnVisibleRowsChanged()
    {
        var offset = this.ScrollViewerPresenter.Offset;
        var viewport = this.ScrollViewerPresenter.Viewport;
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