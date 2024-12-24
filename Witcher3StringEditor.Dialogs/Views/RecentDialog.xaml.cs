using iNKORE.UI.WPF.Modern.Controls;
using System.Windows.Controls;

namespace Witcher3StringEditor.Dialogs.Views;

/// <summary>
/// RecentDialog.xaml 的交互逻辑
/// </summary>
public partial class RecentDialog
{
    public RecentDialog()
    {
        InitializeComponent();

        SfDataGrid.SearchHelper.AllowFiltering = true;
    }

    private void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args) 
        => SfDataGrid.SearchHelper.Search(args.QueryText);

    private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (string.IsNullOrEmpty(sender.Text)) 
            SfDataGrid.SearchHelper.ClearSearch();
    }
}