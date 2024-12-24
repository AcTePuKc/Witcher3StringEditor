using CommunityToolkit.Mvvm.Messaging;
using Witcher3StringEditor.Dialogs.Recipients;

namespace Witcher3StringEditor.Dialogs.Views;

/// <summary>
/// RecentDialog.xaml 的交互逻辑
/// </summary>
public partial class RecentDialog
{
    private readonly ReturnNothingBooleanRecipient recipient = new();

    public RecentDialog()
    {
        InitializeComponent();
        WeakReferenceMessenger.Default.Register<ReturnNothingBooleanRecipient, ReturnNothingBooleanMessage>(recipient, (r, m) =>
        {
            r.Receive(m);
            SfDataGrid.View.Refresh();
        });
    }

    private void Window_Closed(object sender, EventArgs e)
        => WeakReferenceMessenger.Default.UnregisterAll(recipient);
}