namespace v2rayN.Desktop.Controls;

/// <summary>
/// Wraps children into as many columns as fit between <see cref="MinItemWidth"/> and
/// <see cref="MaxItemWidth"/>, then stretches each column so the row fills the width.
/// </summary>
public class AdaptiveWrapPanel : Panel
{
    public static readonly StyledProperty<double> MinItemWidthProperty =
        AvaloniaProperty.Register<AdaptiveWrapPanel, double>(nameof(MinItemWidth), 168);

    public static readonly StyledProperty<double> MaxItemWidthProperty =
        AvaloniaProperty.Register<AdaptiveWrapPanel, double>(nameof(MaxItemWidth), 280);

    public static readonly StyledProperty<double> HorizontalSpacingProperty =
        AvaloniaProperty.Register<AdaptiveWrapPanel, double>(nameof(HorizontalSpacing), 8);

    public static readonly StyledProperty<double> VerticalSpacingProperty =
        AvaloniaProperty.Register<AdaptiveWrapPanel, double>(nameof(VerticalSpacing), 8);

    static AdaptiveWrapPanel()
    {
        AffectsMeasure<AdaptiveWrapPanel>(
            MinItemWidthProperty,
            MaxItemWidthProperty,
            HorizontalSpacingProperty,
            VerticalSpacingProperty);
        AffectsArrange<AdaptiveWrapPanel>(
            MinItemWidthProperty,
            MaxItemWidthProperty,
            HorizontalSpacingProperty,
            VerticalSpacingProperty);
    }

    public double MinItemWidth
    {
        get => GetValue(MinItemWidthProperty);
        set => SetValue(MinItemWidthProperty, value);
    }

    public double MaxItemWidth
    {
        get => GetValue(MaxItemWidthProperty);
        set => SetValue(MaxItemWidthProperty, value);
    }

    public double HorizontalSpacing
    {
        get => GetValue(HorizontalSpacingProperty);
        set => SetValue(HorizontalSpacingProperty, value);
    }

    public double VerticalSpacing
    {
        get => GetValue(VerticalSpacingProperty);
        set => SetValue(VerticalSpacingProperty, value);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var (columns, itemWidth) = Compute(availableSize.Width, Children.Count);
        var gapY = Math.Max(0, VerticalSpacing);
        double height = 0;
        double rowHeight = 0;
        var col = 0;
        var index = 0;

        foreach (var child in Children)
        {
            child.Measure(new Size(itemWidth, double.PositiveInfinity));
            rowHeight = Math.Max(rowHeight, child.DesiredSize.Height);
            col++;
            index++;
            if (col == columns || index == Children.Count)
            {
                if (height > 0)
                {
                    height += gapY;
                }

                height += rowHeight;
                rowHeight = 0;
                col = 0;
            }
        }

        var width = double.IsInfinity(availableSize.Width)
            ? columns * itemWidth + Math.Max(0, columns - 1) * Math.Max(0, HorizontalSpacing)
            : availableSize.Width;
        return new Size(Math.Max(0, width), height);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var (columns, itemWidth) = Compute(finalSize.Width, Children.Count);
        var gapX = Math.Max(0, HorizontalSpacing);
        var gapY = Math.Max(0, VerticalSpacing);
        var row = new List<Control>(columns);
        double y = 0;

        void Flush()
        {
            if (row.Count == 0)
            {
                return;
            }

            var rowHeight = 0d;
            foreach (var child in row)
            {
                rowHeight = Math.Max(rowHeight, child.DesiredSize.Height);
            }

            double x = 0;
            foreach (var child in row)
            {
                child.Arrange(new Rect(x, y, itemWidth, rowHeight));
                x += itemWidth + gapX;
            }

            y += rowHeight + gapY;
            row.Clear();
        }

        foreach (var child in Children)
        {
            child.Measure(new Size(itemWidth, double.PositiveInfinity));
            row.Add(child);
            if (row.Count == columns)
            {
                Flush();
            }
        }

        Flush();
        return finalSize;
    }

    private (int columns, double itemWidth) Compute(double availableWidth, int childCount)
    {
        var min = Math.Max(1, MinItemWidth);
        var max = Math.Max(min, MaxItemWidth);
        var gap = Math.Max(0, HorizontalSpacing);

        if (childCount <= 0)
        {
            return (1, min);
        }

        if (double.IsInfinity(availableWidth) || availableWidth <= 0)
        {
            return (1, max);
        }

        var columns = Math.Max(1, (int)Math.Floor((availableWidth + gap) / (min + gap)));
        columns = Math.Min(columns, childCount);
        var itemWidth = (availableWidth - gap * (columns - 1)) / columns;

        while (itemWidth > max && columns < childCount)
        {
            var next = columns + 1;
            var nextWidth = (availableWidth - gap * (next - 1)) / next;
            if (nextWidth + 0.5 < min)
            {
                break;
            }

            columns = next;
            itemWidth = nextWidth;
        }

        if (itemWidth < 1)
        {
            itemWidth = availableWidth;
            columns = 1;
        }

        return (columns, itemWidth);
    }
}
