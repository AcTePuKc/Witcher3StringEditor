using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using iNKORE.UI.WPF.Modern.Controls;
using System.Windows;
using Witcher3StringEditor.Dialogs.Recipients;
using Witcher3StringEditor.Locales;
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
    private readonly ReturnBooleanNothingRecipient openFileRecipient = new();
    private readonly ReturnNothingStringRecipient aboutInformationRecipient = new();

    public MainWindow()
    {
        InitializeComponent();
        DataGrid.SearchHelper.AllowFiltering = true;
        DataContext = Ioc.Default.GetService<MainWindowViewModel>();

        WeakReferenceMessenger.Default.Register<ReturnBooleanNothingRecipient, ReturnBooleanNothingMessage, string>(openFileRecipient, "FileOpened", static (r, m) =>
        {
            r.Receive(m);
            m.Reply(MessageBox.Show(Strings.OpenFileWarning, Strings.Warning, MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes);
        });

        WeakReferenceMessenger.Default.Register<ReturnNothingStringRecipient, ReturnNothingStringMessage, string>(aboutInformationRecipient, "AboutInformation", static (r, m) =>
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
        WeakReferenceMessenger.Default.UnregisterAll(openFileRecipient);
        WeakReferenceMessenger.Default.UnregisterAll(aboutInformationRecipient);
    }
}