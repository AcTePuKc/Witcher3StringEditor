using System.Windows;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using iNKORE.UI.WPF.Modern.Controls;
using Serilog;
using Witcher3StringEditor.Locales;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

namespace Witcher3StringEditor.Dialogs.Views;

/// <summary>
///     HistoryDialog.xaml 的交互逻辑
/// </summary>
public partial class BackupDialog : IRecipient<AsyncRequestMessage<bool>>
{
    public BackupDialog()
    {
        InitializeComponent();
        SetupSearchHelper();
        RegisterMessageHandlers();
    }

    public void Receive(AsyncRequestMessage<bool> message)
    {
    }

    private void SetupSearchHelper()
    {
        SfDataGrid.SearchHelper.AllowFiltering = true;
        SfDataGrid.SearchHelper.AllowCaseSensitiveSearch = false;
    }

    private void RegisterMessageHandlers()
    {
        var messageHandlers =
            new (string, Func<string>, Func<string>, MessageBoxButton, MessageBoxImage, MessageBoxResult)[]
            {
                ("BackupRestore", () => Strings.BackupRestoreMessage, () => Strings.BackupRestoreCaption,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question, MessageBoxResult.Yes),
                ("BackupDelete", () => Strings.BackupDeleteMessage, () => Strings.BackupDeleteCaption,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question, MessageBoxResult.Yes),
                ("BackupFileNoFound", () => Strings.BackupFileNoFoundMessage, () => Strings.BackupFileNoFoundCaption,
                    MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes),
                ("OperationFailed", () => Strings.OperationFailureMessage, () => Strings.OperationResultCaption,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning, MessageBoxResult.OK)
            };

        foreach (var (token, message, caption, button, icon, expected) in messageHandlers)
            WeakReferenceMessenger.Default.Register<BackupDialog, AsyncRequestMessage<bool>, string>(
                this,
                token,
                (_, m) => { m.Reply(MessageBox.Show(message(), caption(), button, icon) == expected); });
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        WeakReferenceMessenger.Default.UnregisterAll(this);
    }

    private void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        SfDataGrid.SearchHelper.Search(args.QueryText);
        Log.Information("Search query submitted: {QueryText}", args.QueryText);
    }

    private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (string.IsNullOrEmpty(sender.Text))
            SfDataGrid.SearchHelper.ClearSearch();
    }
}