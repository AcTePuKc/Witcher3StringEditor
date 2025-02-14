using Microsoft.Xaml.Behaviors;
using Syncfusion.UI.Xaml.Grid;

namespace Witcher3StringEditor.Dialogs.Behaviors;

public class SfDataGridQueryRowHeightBehavior : Behavior<SfDataGrid>
{
    private readonly GridRowSizingOptions gridRowResizingOptions = new();

    protected override void OnAttached()
        => AssociatedObject.QueryRowHeight += AssociatedObject_QueryRowHeight;

    protected override void OnDetaching()
        => AssociatedObject.QueryRowHeight -= AssociatedObject_QueryRowHeight;

    private void AssociatedObject_QueryRowHeight(object? sender, QueryRowHeightEventArgs e)
    {
        if (!AssociatedObject.GridColumnSizer.GetAutoRowHeight(e.RowIndex, gridRowResizingOptions, out var autoHeight)) return;
        if (autoHeight <= 30) return;
        e.Height = autoHeight;
        e.Handled = true;
    }
}