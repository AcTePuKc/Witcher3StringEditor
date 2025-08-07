using System.Windows;
using Microsoft.Xaml.Behaviors;
using Syncfusion.UI.Xaml.Grid;

namespace Witcher3StringEditor.Dialogs.Behaviors;

public class SfDataGridQueryRowHeightBehavior : Behavior<SfDataGrid>
{
    public static readonly DependencyProperty MinHeightProperty
        = DependencyProperty.Register(nameof(MinHeight), typeof(double), typeof(SfDataGridQueryRowHeightBehavior),
            new PropertyMetadata(25.0));

    private readonly GridRowSizingOptions gridRowResizingOptions = new();

    public double MinHeight
    {
        get => (double)GetValue(MinHeightProperty);
        set => SetValue(MinHeightProperty, value);
    }

    protected override void OnAttached()
    {
        AssociatedObject.QueryRowHeight += AssociatedObject_QueryRowHeight;
    }

    protected override void OnDetaching()
    {
        AssociatedObject.QueryRowHeight -= AssociatedObject_QueryRowHeight;
    }

    private void AssociatedObject_QueryRowHeight(object? sender, QueryRowHeightEventArgs e)
    {
        if (!AssociatedObject.GridColumnSizer.GetAutoRowHeight(e.RowIndex, gridRowResizingOptions, out var autoHeight) || autoHeight <= MinHeight) return;
        e.Height = autoHeight;
        e.Handled = true;
    }
}