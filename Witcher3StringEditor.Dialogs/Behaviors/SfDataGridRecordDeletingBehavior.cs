using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Microsoft.Xaml.Behaviors;
using Serilog;
using Syncfusion.UI.Xaml.Grid;

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

    private static async void AssociatedObject_RecordDeleting(object? sender, RecordDeletingEventArgs e)
    {
        try
        {
            var recordItemType = e.Items[0].GetType().Name;
            if (!await WeakReferenceMessenger.Default.Send(new AsyncRequestMessage<bool>(), recordItemType))
                e.Cancel = true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while deleting a record.");
        }
    }
}