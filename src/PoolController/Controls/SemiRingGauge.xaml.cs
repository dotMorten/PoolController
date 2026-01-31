using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace PoolController.Controls;

public sealed partial class SemiRingGauge : UserControl
{
    public SemiRingGauge()
    {
        this.InitializeComponent();
        TrackBrush = new SolidColorBrush(Microsoft.UI.Colors.Gray);
        ValueBrush = new SolidColorBrush(Microsoft.UI.Colors.DeepSkyBlue);
        SizeChanged += (_, __) => UpdateGeometries();
    }

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(double), typeof(SemiRingGauge),
            new PropertyMetadata(0d, (d, _) => ((SemiRingGauge)d).UpdateGeometries()));

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public static readonly DependencyProperty StrokeThicknessProperty =
        DependencyProperty.Register(nameof(StrokeThickness), typeof(double), typeof(SemiRingGauge),
            new PropertyMetadata(10d, (d, _) => ((SemiRingGauge)d).UpdateGeometries()));

    public double StrokeThickness
    {
        get => (double)GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }

    public static readonly DependencyProperty TrackBrushProperty =
        DependencyProperty.Register(nameof(TrackBrush), typeof(Brush), typeof(SemiRingGauge),
            new PropertyMetadata(null));

    public Brush TrackBrush
    {
        get => (Brush)GetValue(TrackBrushProperty);
        set => SetValue(TrackBrushProperty, value);
    }

    public static readonly DependencyProperty ValueBrushProperty =
        DependencyProperty.Register(nameof(ValueBrush), typeof(Brush), typeof(SemiRingGauge),
            new PropertyMetadata(null));

    public Brush ValueBrush
    {
        get => (Brush)GetValue(ValueBrushProperty);
        set => SetValue(ValueBrushProperty, value);
    }

    // Bindable geometries
  //  public Geometry TrackGeometry { get; private set; } = new PathGeometry();
  //  public Geometry ValueGeometry { get; private set; } = new PathGeometry();

    private void UpdateGeometries()
    {
        var w = ActualWidth;
        var h = ActualHeight;
        if (w <= 0 || h <= 0) return;

        // Semi-ring spans 180 degrees (left to right along the top)
        // We’ll draw an arc centered horizontally, near the bottom of this control
        var thickness = StrokeThickness;
        var radius = Math.Max(0, (w - thickness) / 2.0);
        var center = new Point(w / 2.0, h); // bottom center
        var start = PointOnCircle(center, radius, 180); // left
        var endFull = PointOnCircle(center, radius, 0); // right

        TrackPath.Data = ArcGeometry(start, endFull, radius);

        var v = Math.Clamp(Value, 0, 3500) / 3500d;
        var endValue = PointOnCircle(center, radius, 180 * (1 - v)); // 180->0 as v 0->1
        ValuePath.Data = ArcGeometry(start, endValue, radius);
    }

    private static Geometry ArcGeometry(Point start, Point end, double radius)
    {
        var fig = new PathFigure { StartPoint = start, IsClosed = false };
        fig.Segments.Add(new ArcSegment
        {
            Point = end,
            Size = new Size(radius, radius),
            RotationAngle = 180,
            SweepDirection = SweepDirection.Clockwise,
            IsLargeArc = false
        });

        var geo = new PathGeometry();
        geo.Figures.Add(fig);
        return geo;
    }

    private static Point PointOnCircle(Point center, double radius, double degrees)
    {
        var rad = degrees * Math.PI / 180.0;
        return new Point(center.X + radius * Math.Cos(rad),
                         center.Y - radius * Math.Sin(rad));
    }
}
