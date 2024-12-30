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
    private readonly WindowClosingRecipient closingRecipient = new();
    private readonly FileOpenedRecipient fileOpenedRecipient = new();
    private readonly SimpleStringRecipient aboutInformationRecipient = new();

    public MainWindow()
    {
        InitializeComponent();
        SfDataGrid.SearchHelper.AllowFiltering = true;
        SfDataGrid.SearchHelper.AllowCaseSensitiveSearch = false;
        DataContext = Ioc.Default.GetService<MainWindowViewModel>();
        WeakReferenceMessenger.Default.Register<WindowClosingRecipient, WindowClosingMessage, string>(closingRecipient, "MainWindowClosing", static (r, m) =>
        {
            r.Receive(m);
            m.Reply(MessageBox.Show(Strings.AppExitMessage,
                                    Strings.AppExitCaption,
                                    MessageBoxButton.YesNo,
                                    MessageBoxImage.Question) == MessageBoxResult.No);
        });
        WeakReferenceMessenger.Default.Register<FileOpenedRecipient, FileOpenedMessage, string>(fileOpenedRecipient, "FileOpened", static (r, m) =>
        {
            r.Receive(m);
            m.Reply(MessageBox.Show(Strings.FileOpenedMessage,
                                    Strings.FileOpenedCaption,
                                    MessageBoxButton.YesNo,
                                    MessageBoxImage.Question) == MessageBoxResult.Yes);
        });
        WeakReferenceMessenger.Default.Register<FileOpenedRecipient, FileOpenedMessage, string>(fileOpenedRecipient, "OpenedFileNoFound", static (r, m) =>
        {
            r.Receive(m);
            m.Reply(MessageBox.Show(Strings.FileOpenedNoFoundMessage,
                                    Strings.FileOpenedNoFoundCaption,
                                    MessageBoxButton.YesNo,
                                    MessageBoxImage.Question) == MessageBoxResult.Yes);
        });
        WeakReferenceMessenger.Default.Register<SimpleStringRecipient, SimpleStringMessage, string>(aboutInformationRecipient, "AboutInformation", static (r, m) =>
        {
            r.Receive(m);
            MessageBox.Show(m.Message,
                            Strings.AboutCaption,
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
        });
    }

    private void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args) 
        => SfDataGrid.SearchHelper.Search(args.QueryText);

    private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (string.IsNullOrEmpty(sender.Text)) SfDataGrid.SearchHelper.ClearSearch();
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        WeakReferenceMessenger.Default.UnregisterAll(fileOpenedRecipient);
        WeakReferenceMessenger.Default.UnregisterAll(aboutInformationRecipient);
    }
}