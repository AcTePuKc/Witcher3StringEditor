using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Xaml.Behaviors;
using Syncfusion.UI.Xaml.Grid;
using Witcher3StringEditor.Dialogs.Recipients;
using Log = Serilog.Log;

namespace Witcher3StringEditor.Dialogs.Behaviors;

internal class RecentItemDeletingBehavior : Behavior<SfDataGrid>
{
    protected override void OnAttached()
        => AssociatedObject.RecordDeleting += AssociatedObject_RecordDeleting;

    protected override void OnDetaching()
        => AssociatedObject.RecordDeleting -= AssociatedObject_RecordDeleting;

    private static async void AssociatedObject_RecordDeleting(object? sender, RecordDeletingEventArgs e)
    {
        try
        {
            if (await WeakReferenceMessenger.Default.Send(new RecentItemDeletingMessage()) == false) e.Cancel = true;
        }
        catch (Exception ex)
        {
            Log.Error(ex.Message);
        }
    }
}