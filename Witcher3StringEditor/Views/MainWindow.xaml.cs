using System.Windows;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using iNKORE.UI.WPF.Modern;
using iNKORE.UI.WPF.Modern.Controls;
using iNKORE.UI.WPF.Modern.Controls.Primitives;
using Microsoft.Extensions.Logging;
using Witcher3StringEditor.Dialogs.Recipients;
using Witcher3StringEditor.Locales;
using Witcher3StringEditor.ViewModels;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

namespace Witcher3StringEditor.Views;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    private readonly ILogger<MainWindow> logger = Ioc.Default.GetRequiredService<ILogger<MainWindow>>();
    private readonly AsyncRequestRecipient<bool> recipient = new();

    public MainWindow()
    {
        InitializeComponent();
        SfDataGrid.SearchHelper.AllowFiltering = true;
        SfDataGrid.SearchHelper.AllowCaseSensitiveSearch = false;
        DataContext = Ioc.Default.GetService<MainWindowViewModel>();

        var messageHandlers = new[]
        {
            ("FileOpened", Strings.FileOpenedMessage, Strings.FileOpenedCaption),
            ("OpenedFileNoFound", Strings.FileOpenedNoFoundMessage, Strings.FileOpenedNoFoundCaption)
        };

        foreach (var (token, message, caption) in messageHandlers)
            WeakReferenceMessenger.Default.Register<AsyncRequestRecipient<bool>, FileOpenedMessage, string>(
                recipient,
                token,
                (r, m) =>
                {
                    r.Receive(m);
                    m.Reply(MessageBox.Show(message, caption, MessageBoxButton.YesNo, MessageBoxImage.Question) ==
                            MessageBoxResult.Yes);
                });

        WeakReferenceMessenger.Default.Register<AsyncRequestRecipient<bool>, AsyncRequestMessage<bool>, string>(
            recipient, "MainWindowClosing", static (r, m) =>
            {
                r.Receive(m);
                m.Reply(MessageBox.Show(Strings.AppExitMessage,
                    Strings.AppExitCaption,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question) == MessageBoxResult.No);
            });
        WeakReferenceMessenger.Default.Register<AsyncRequestRecipient<bool>, AsyncRequestMessage<bool>, string>(
            recipient, "FirstRun", static (r, m) =>
            {
                r.Receive(m);
                m.Reply(MessageBox.Show(Strings.FristRunMessage,
                    Strings.FristRunCaption,
                    MessageBoxButton.OK,
                    MessageBoxImage.Question) == MessageBoxResult.OK);
            });
    }

    private void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        SfDataGrid.SearchHelper.Search(args.QueryText);
        logger.LogInformation("Search query submitted: {QueryText}", args.QueryText);
    }

    private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (string.IsNullOrEmpty(sender.Text)) SfDataGrid.SearchHelper.ClearSearch();
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        WeakReferenceMessenger.Default.UnregisterAll(recipient);
    }

    private void AppTitleBar_OnLoaded(object sender, RoutedEventArgs e)
    {
        if (TitleBar.GetExtendViewIntoTitleBar(this)) SetRegionsForCustomTitleBar();
    }

    private void AppTitleBar_OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (TitleBar.GetExtendViewIntoTitleBar(this)) SetRegionsForCustomTitleBar();
    }

    private void SetRegionsForCustomTitleBar()
    {
        RightPaddingColumn.Width = new GridLength(TitleBar.GetSystemOverlayRightInset(this));
    }

    private void ThemeSwitchBtn_OnClick(object sender, RoutedEventArgs e)
    {
        ThemeManager.Current.ApplicationTheme =
            ThemeManager.Current.ActualApplicationTheme == ApplicationTheme.Light
                ? ApplicationTheme.Dark
                : ApplicationTheme.Light;
    }
}