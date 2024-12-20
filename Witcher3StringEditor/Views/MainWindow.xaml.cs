using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using iNKORE.UI.WPF.Modern.Controls;
using Witcher3StringEditor.Recipients;
using Witcher3StringEditor.ViewModels;

namespace Witcher3StringEditor.Views;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    private readonly AboutInformationRecipient recipient;

    public MainWindow()
    {
        InitializeComponent();
        DataGrid.SearchHelper.AllowFiltering = true;
        DataContext = Ioc.Default.GetService<MainWindowViewModel>();
        recipient = new AboutInformationRecipient();
        WeakReferenceMessenger.Default.Register(recipient);
    }

    private void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        DataGrid.SearchHelper.Search(args.QueryText);
    }

    private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (string.IsNullOrEmpty(sender.Text)) DataGrid.SearchHelper.ClearSearch();
    }
}