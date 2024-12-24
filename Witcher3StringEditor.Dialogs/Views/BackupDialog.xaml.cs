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
    private readonly ReturnBooleanNothingRecipient recipient = new();

    public BackupDialog()
    {
        InitializeComponent();

        DataGrid.SearchHelper.AllowFiltering = true;

        WeakReferenceMessenger.Default.Register<ReturnBooleanNothingRecipient, ReturnBooleanNothingMessage, string>(recipient, "BackupRestore", static (r, m) =>
        {
            r.Receive(m);
            m.Reply(MessageBox.Show(Strings.BackupRestoreMessage,
                                    Strings.Warning,
                                    MessageBoxButton.YesNo,
                                    MessageBoxImage.Warning) ==
                                    MessageBoxResult.Yes);
        });

        WeakReferenceMessenger.Default.Register<ReturnBooleanNothingRecipient, ReturnBooleanNothingMessage, string>(recipient, "BackupDelete", static (r, m) =>
        {
            r.Receive(m);
            m.Reply(MessageBox.Show(Strings.BackupDeleteMessage,
                                    Strings.Warning,
                                    MessageBoxButton.YesNo,
                                    MessageBoxImage.Warning) ==
                                    MessageBoxResult.Yes);
        });
    }

    private void Window_Closed(object sender, EventArgs e)
        => WeakReferenceMessenger.Default.UnregisterAll(recipient);

    private void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        DataGrid.SearchHelper.Search(args.QueryText);
    }

    private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (string.IsNullOrEmpty(sender.Text)) DataGrid.SearchHelper.ClearSearch();
    }
}