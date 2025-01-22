using CommunityToolkit.Mvvm.Messaging;
using iNKORE.UI.WPF.Modern.Controls;
using System.Windows;
using Witcher3StringEditor.Dialogs.Locales;
using Witcher3StringEditor.Dialogs.Recipients;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

namespace Witcher3StringEditor.Dialogs.Views;

/// <summary>
/// RecentDialog.xaml 的交互逻辑
/// </summary>
public partial class RecentDialog
{
    private readonly RecentItemDeletingRecipient recipient = new();

    public RecentDialog()
    {
        InitializeComponent();
        SfDataGrid.SearchHelper.AllowFiltering = true;
        WeakReferenceMessenger.Default.Register<RecentItemDeletingRecipient, RecentItemDeletingMessage>(recipient, (r, m) =>
        {
            r.Receive(m);
            m.Reply(MessageBox.Show(Strings.RecentItemDeletingMessgae,
                                    Strings.RecentItemDeletingCaption,
                                    MessageBoxButton.YesNo,
                                    MessageBoxImage.Warning) == MessageBoxResult.Yes);
        });
    }

    private void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        => SfDataGrid.SearchHelper.Search(args.QueryText);

    private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (string.IsNullOrEmpty(sender.Text))
            SfDataGrid.SearchHelper.ClearSearch();
    }

    private void Window_Closed(object sender, EventArgs e) 
        => WeakReferenceMessenger.Default.UnregisterAll(recipient);
}