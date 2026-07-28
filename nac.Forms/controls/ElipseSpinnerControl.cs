using System;
using Avalonia;
using Avalonia.Media;
using Avalonia.Threading;

namespace nac.Forms.controls;

public class ElipseSpinnerControl : Avalonia.Controls.Border
{
    
    private DispatcherTimer? _timer;
    private RotateTransform? _rotate;
    private double _angle;

    private int squareSize;

    public ElipseSpinnerControl(int squareSize = 20)
    {
        this.Width = squareSize;
        this.Height = squareSize;
        Child = CreatePath();
        RenderTransformOrigin = RelativePoint.Center;
    }


    private Avalonia.Controls.Control CreatePath()
    {
        var spinner = new Avalonia.Controls.Shapes.Ellipse
        {
            Width = this.Width,
            Height = this.Height,
            Stroke = Brushes.DodgerBlue,
            StrokeThickness = 6,
            StrokeDashArray = new Avalonia.Collections.AvaloniaList<double>
            {
                4,2
            }
        };

        var spinnerParent = new Avalonia.Controls.Viewbox
        {
            Child = spinner
        };

        return spinnerParent;
    }
    
    protected override void OnAttachedToVisualTree(
        VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        StartAnimation();
    }

    protected override void OnDetachedFromVisualTree(
        VisualTreeAttachmentEventArgs e)
    {
        StopAnimation();

        base.OnDetachedFromVisualTree(e);
    }

    private void StartAnimation()
    {
        if (_timer != null)
            return;

        _rotate = new RotateTransform();
        RenderTransform = _rotate;
        RenderTransformOrigin = RelativePoint.Center;

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16)
        };

        _timer.Tick += (_, _) =>
        {
            _angle += 5;

            if (_angle >= 360)
                _angle -= 360;

            _rotate.Angle = _angle;
        };

        _timer.Start();
    }

    private void StopAnimation()
    {
        _timer?.Stop();
        _timer = null;
    }
}