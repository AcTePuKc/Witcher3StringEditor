using iNKORE.UI.WPF.Modern.Controls;

namespace Witcher3StringEditor.Dialogs.Views;

/// <summary>
///     LogDialog.xaml 的交互逻辑
/// </summary>
public partial class LogDialog
{
    public LogDialog()
    {
        InitializeComponent();
        SfDataGrid.SearchHelper.AllowFiltering = true;
        SfDataGrid.SearchHelper.AllowCaseSensitiveSearch = false;
    }

    private void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        => SfDataGrid.SearchHelper.Search(args.QueryText);

    private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (string.IsNullOrEmpty(sender.Text))
            SfDataGrid.SearchHelper.ClearSearch();
    }
}