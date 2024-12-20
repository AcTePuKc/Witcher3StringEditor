using CommunityToolkit.Mvvm.Messaging;
using System.Diagnostics;
using Witcher3StringEditor.Dialogs.Recipients;

namespace Witcher3StringEditor.Dialogs.Views;

/// <summary>
///     HistoryDialog.xaml 的交互逻辑
/// </summary>
public partial class BackupDialog
{
    private readonly BackupActionRecipient recipient;

    public BackupDialog()
    {
        InitializeComponent();
        recipient = new BackupActionRecipient();
        WeakReferenceMessenger.Default.Register<BackupActionRecipient,BackupActionMessage>(recipient: recipient,(r,m)=>
        {
            r.Receive(m);
            m.Reply(r.Response);
        });
    }
}