using Microsoft.Xaml.Behaviors;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Helpers;

namespace Witcher3StringEditor.Behaviors;

internal class SfDataGridSizeChangedBehavior : Behavior<SfDataGrid>
{
    protected override void OnAttached()
    {
        AssociatedObject.SizeChanged += AssociatedObject_SizeChanged;
    }

    protected override void OnDetaching()
    {
        AssociatedObject.SizeChanged -= AssociatedObject_SizeChanged;
    }

    private void AssociatedObject_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
    {
        AssociatedObject.GetVisualContainer().RowHeightManager.Reset();
        AssociatedObject.GetVisualContainer().InvalidateMeasureInfo();
    }
}