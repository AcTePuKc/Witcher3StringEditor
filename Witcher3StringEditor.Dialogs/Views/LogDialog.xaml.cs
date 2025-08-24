using CommunityToolkit.Mvvm.DependencyInjection;
using iNKORE.UI.WPF.Modern.Controls;
using Microsoft.Extensions.Logging;

namespace Witcher3StringEditor.Dialogs.Views;

/// <summary>
///     LogDialog.xaml 的交互逻辑
/// </summary>
public partial class LogDialog
{
    private readonly ILogger<LogDialog> logger;

    public LogDialog()
    {
        InitializeComponent();
        logger = Ioc.Default.GetRequiredService<ILogger<LogDialog>>();
        SfDataGrid.SearchHelper.AllowFiltering = true;
        SfDataGrid.SearchHelper.AllowCaseSensitiveSearch = false;
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