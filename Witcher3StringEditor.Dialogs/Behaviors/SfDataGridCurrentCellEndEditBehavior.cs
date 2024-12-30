using Microsoft.Xaml.Behaviors;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Helpers;

namespace Witcher3StringEditor.Dialogs.Behaviors;

public class SfDataGridCurrentCellEndEditBehavior : Behavior<SfDataGrid>
{
    protected override void OnAttached() 
        => AssociatedObject.CurrentCellEndEdit += AssociatedObject_CurrentCellEndEdit;

    protected override void OnDetaching() 
        => AssociatedObject.CurrentCellEndEdit -= AssociatedObject_CurrentCellEndEdit;

    private void AssociatedObject_CurrentCellEndEdit(object? sender, CurrentCellEndEditEventArgs e)
    {
        AssociatedObject.InvalidateRowHeight(e.RowColumnIndex.RowIndex);
        AssociatedObject.GetVisualContainer().InvalidateMeasureInfo();
    }
}