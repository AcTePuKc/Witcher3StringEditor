using CommunityToolkit.Mvvm.Messaging;
using System.Windows;
using Witcher3StringEditor.Dialogs.Locales;
using Witcher3StringEditor.Dialogs.Recipients;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;

namespace Witcher3StringEditor.Dialogs.Views;

/// <summary>
///     HistoryDialog.xaml 的交互逻辑
/// </summary>
public partial class BackupDialog
{
    private readonly BackupActionRecipient backupActionRecipient;

    public BackupDialog()
    {
        InitializeComponent();
        backupActionRecipient = new BackupActionRecipient();
        WeakReferenceMessenger.Default.Register<BackupActionRecipient, BackupActionMessage>(recipient: backupActionRecipient, (r, m) =>
        {
            r.Receive(m);
            m.Reply(MessageBox.Show(m.BackupAction == BackupActionType.restore
                                    ? Strings.BackupRestoreMessage
                                    : Strings.BackupDeleteMessage,
                                    Strings.Warning,
                                    MessageBoxButton.YesNo,
                                    MessageBoxImage.Warning) ==
                                    MessageBoxResult.Yes);
        });
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        WeakReferenceMessenger.Default.UnregisterAll(backupActionRecipient);
    }
}