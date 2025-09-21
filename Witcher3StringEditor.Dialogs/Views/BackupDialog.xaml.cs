using System.Windows;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using iNKORE.UI.WPF.Modern.Controls;
using Serilog;
using Witcher3StringEditor.Common.Constants;
using Witcher3StringEditor.Locales;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

namespace Witcher3StringEditor.Dialogs.Views;

/// <summary>
///     HistoryDialog.xaml 的交互逻辑
/// </summary>
public partial class BackupDialog
{
    public BackupDialog()
    {
        InitializeComponent();
        SetupSearchHelper();
        RegisterMessageHandlers();
    }

    private void SetupSearchHelper()
    {
        SfDataGrid.SearchHelper.AllowFiltering = true;
        SfDataGrid.SearchHelper.AllowCaseSensitiveSearch = false;
    }

    private void RegisterMessageHandlers()
    {
        var messageHandlers = CreateMessageHandlers();
        foreach (var (token, message, caption, button, icon, expected) in messageHandlers)
            RegisterMessageHandler(token, message, caption, button, icon, expected);
    }

    private void RegisterMessageHandler(string token, Func<string> message, Func<string> caption,
        MessageBoxButton button, MessageBoxImage icon,
        MessageBoxResult expected)
    {
        WeakReferenceMessenger.Default.Register<BackupDialog, AsyncRequestMessage<bool>, string>(
            this,
            token,
            (_, m) => { m.Reply(MessageBox.Show(message(), caption(), button, icon) == expected); });
    }

    private static (string, Func<string>, Func<string>, MessageBoxButton, MessageBoxImage, MessageBoxResult)[]
        CreateMessageHandlers()
    {
        return
        [
            (MessageTokens.BackupRestore, () => Strings.BackupRestoreMessage, () => Strings.BackupRestoreCaption,
                MessageBoxButton.YesNo,
                MessageBoxImage.Question, MessageBoxResult.Yes),
            (MessageTokens.BackupDelete, () => Strings.BackupDeleteMessage, () => Strings.BackupDeleteCaption,
                MessageBoxButton.YesNo,
                MessageBoxImage.Question, MessageBoxResult.Yes),
            (MessageTokens.BackupFileNoFound, () => Strings.BackupFileNoFoundMessage,
                () => Strings.BackupFileNoFoundCaption,
                MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes),
            (MessageTokens.OperationFailed, () => Strings.OperationFailureMessage, () => Strings.OperationResultCaption,
                MessageBoxButton.OK,
                MessageBoxImage.Warning, MessageBoxResult.OK)
        ];
    }

    private void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        if (string.IsNullOrWhiteSpace(args.QueryText)) return;
        SfDataGrid.SearchHelper.Search(args.QueryText);
        Log.Information("Search query submitted: {QueryText}", args.QueryText);
    }

    private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (string.IsNullOrEmpty(sender.Text))
            SfDataGrid.SearchHelper.ClearSearch();
    }

    private void BackupDialog_OnClosed(object? sender, EventArgs e)
    {
        WeakReferenceMessenger.Default.UnregisterAll(this);
        SfDataGrid.SearchHelper.Dispose();
        SfDataGrid.Dispose();
        SfDataPager.Dispose();
    }
}