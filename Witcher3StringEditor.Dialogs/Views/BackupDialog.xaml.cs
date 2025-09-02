using System.Windows;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using iNKORE.UI.WPF.Modern.Controls;
using Microsoft.Extensions.Logging;
using Witcher3StringEditor.Locales;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

namespace Witcher3StringEditor.Dialogs.Views;

/// <summary>
///     HistoryDialog.xaml 的交互逻辑
/// </summary>
public partial class BackupDialog : IRecipient<AsyncRequestMessage<bool>>
{
    private readonly ILogger<BackupDialog> logger;

    public BackupDialog()
    {
        InitializeComponent();
        logger = Ioc.Default.GetRequiredService<ILogger<BackupDialog>>();
        SfDataGrid.SearchHelper.AllowFiltering = true;
        SfDataGrid.SearchHelper.AllowCaseSensitiveSearch = false;

        var messageHandlers = new[]
        {
            ("BackupRestore", LangKeys.BackupRestoreMessage, LangKeys.BackupRestoreCaption, MessageBoxButton.YesNo,
                MessageBoxImage.Question, MessageBoxResult.Yes),
            ("BackupDelete", LangKeys.BackupDeleteMessage, LangKeys.BackupDeleteCaption, MessageBoxButton.YesNo,
                MessageBoxImage.Question, MessageBoxResult.Yes),
            ("BackupFileNoFound", LangKeys.BackupFileNoFoundMessage, LangKeys.BackupFileNoFoundCaption,
                MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes),
            ("OperationFailed", LangKeys.OperationFailureMessage, LangKeys.OperationResultCaption, MessageBoxButton.OK,
                MessageBoxImage.Warning, MessageBoxResult.OK)
        };

        foreach (var (token, message, caption, button, icon, expected) in messageHandlers)
            WeakReferenceMessenger.Default.Register<BackupDialog, AsyncRequestMessage<bool>, string>(
                this,
                token,
                (_, m) => { m.Reply(MessageBox.Show(message, caption, button, icon) == expected); });
    }

    public void Receive(AsyncRequestMessage<bool> message)
    {
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        WeakReferenceMessenger.Default.UnregisterAll(this);
    }

    private void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        SfDataGrid.SearchHelper.Search(args.QueryText);
        logger.LogInformation("Search query submitted: {QueryText}", args.QueryText);
    }

    private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (string.IsNullOrEmpty(sender.Text))
            SfDataGrid.SearchHelper.ClearSearch();
    }
}