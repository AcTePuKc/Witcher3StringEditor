using CommunityToolkit.Mvvm.Messaging;
using iNKORE.UI.WPF.Modern.Controls;
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
    private readonly BackupRecipient backupRecipient = new();

    public BackupDialog()
    {
        InitializeComponent();
        SfDataGrid.SearchHelper.AllowFiltering = true;
        SfDataGrid.SearchHelper.AllowCaseSensitiveSearch = false;
        WeakReferenceMessenger.Default.Register<BackupRecipient, BackupMessage, string>(backupRecipient, "BackupRestore", static (r, m) =>
        {
            r.Receive(m);
            m.Reply(MessageBox.Show(Strings.BackupRestoreMessage,
                                    Strings.BackupRestoreCaption,
                                    MessageBoxButton.YesNo,
                                    MessageBoxImage.Question) ==
                                    MessageBoxResult.Yes);
        });
        WeakReferenceMessenger.Default.Register<BackupRecipient, BackupMessage, string>(backupRecipient, "BackupDelete", static (r, m) =>
        {
            r.Receive(m);
            m.Reply(MessageBox.Show(Strings.BackupDeleteMessage,
                                    Strings.BackupDeleteCaption,
                                    MessageBoxButton.YesNo,
                                    MessageBoxImage.Question) ==
                                    MessageBoxResult.Yes);
        });
        WeakReferenceMessenger.Default.Register<BackupRecipient, BackupMessage, string>(backupRecipient, "BackupFileNoFound", static (r, m) =>
        {
            r.Receive(m);
            m.Reply(MessageBox.Show(Strings.BackupFileNoFoundMessage,
                                    Strings.BackupFileNoFoundCaption,
                                    MessageBoxButton.YesNo,
                                    MessageBoxImage.Question) ==
                                    MessageBoxResult.Yes);
        });
    }

    private void Window_Closed(object sender, EventArgs e)
        => WeakReferenceMessenger.Default.UnregisterAll(backupRecipient);

    private void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        => SfDataGrid.SearchHelper.Search(args.QueryText);

    private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (string.IsNullOrEmpty(sender.Text))
            SfDataGrid.SearchHelper.ClearSearch();
    }
}