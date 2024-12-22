using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using iNKORE.UI.WPF.Modern.Controls;
using System.Windows;
using Witcher3StringEditor.Locales;
using Witcher3StringEditor.Recipients;
using Witcher3StringEditor.ViewModels;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;

namespace Witcher3StringEditor.Views;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    private readonly ReloadW3ItemsRecipient reloadW3ItemsRecipient;
    private readonly AboutInformationRecipient aboutInformationRecipient;

    public MainWindow()
    {
        InitializeComponent();
        DataGrid.SearchHelper.AllowFiltering = true;
        DataContext = Ioc.Default.GetService<MainWindowViewModel>();

        reloadW3ItemsRecipient = new ReloadW3ItemsRecipient();
        WeakReferenceMessenger.Default.Register<ReloadW3ItemsRecipient, ReloadW3ItemsMessage>(reloadW3ItemsRecipient, (r, m) =>
        {
            r.Receive(m);
            m.Reply(MessageBox.Show(Strings.OpenFileWarning, Strings.Warning, MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes);
        });

        aboutInformationRecipient = new AboutInformationRecipient();
        WeakReferenceMessenger.Default.Register<AboutInformationRecipient, AboutInformationMessage>(aboutInformationRecipient, (r, m) =>
        {
            r.Receive(m);
            MessageBox.Show(m.Message, Strings.About, MessageBoxButton.OK, MessageBoxImage.Information);
        });
    }

    private void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        DataGrid.SearchHelper.Search(args.QueryText);
    }

    private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (string.IsNullOrEmpty(sender.Text)) DataGrid.SearchHelper.ClearSearch();
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        WeakReferenceMessenger.Default.UnregisterAll(aboutInformationRecipient);
        WeakReferenceMessenger.Default.UnregisterAll(reloadW3ItemsRecipient);
    }
}