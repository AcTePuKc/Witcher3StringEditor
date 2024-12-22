using Microsoft.Xaml.Behaviors;
using Syncfusion.UI.Xaml.Grid;
using System.Windows;
using Witcher3StringEditor.Dialogs.Locales;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;

namespace Witcher3StringEditor.Dialogs.Behaviors;

internal class SfDataGridRecordDeletingBehavior : Behavior<SfDataGrid>
{
    protected override void OnAttached()
    {
        AssociatedObject.RecordDeleting += AssociatedObject_RecordDeleting;
    }

    protected override void OnDetaching()
    {
        AssociatedObject.RecordDeleting -= AssociatedObject_RecordDeleting;
    }

    private static void AssociatedObject_RecordDeleting(object? sender, RecordDeletingEventArgs e)
    {
        if (MessageBox.Show(Strings.RecentItemDeletingMessgae,
                Strings.Warning,
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning) == MessageBoxResult.No)
        {
            e.Cancel = true;
        }
    }
}