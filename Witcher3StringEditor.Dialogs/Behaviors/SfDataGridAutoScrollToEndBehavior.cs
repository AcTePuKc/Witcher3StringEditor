using Microsoft.Xaml.Behaviors;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.ScrollAxis;
using System.Windows;

namespace Witcher3StringEditor.Dialogs.Behaviors;

internal class SfDataGridAutoScrollToEndBehavior : Behavior<SfDataGrid>
{
    protected override void OnAttached()
    {
        AssociatedObject.Loaded += AssociatedObject_Loaded;
    }

    protected override void OnDetaching()
    {
        AssociatedObject.Loaded -= AssociatedObject_Loaded;
    }

    private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
    {
        var lastItem = AssociatedObject.View.Records.LastOrDefault();
        if (lastItem == null) return;
        var rowIndex = AssociatedObject.ResolveToRowIndex(lastItem);
        AssociatedObject.ScrollInView(new RowColumnIndex(rowIndex, 0));
    }
}