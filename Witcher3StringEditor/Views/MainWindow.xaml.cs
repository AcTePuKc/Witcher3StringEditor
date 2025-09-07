using System.Windows;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using iNKORE.UI.WPF.Modern;
using iNKORE.UI.WPF.Modern.Controls;
using iNKORE.UI.WPF.Modern.Controls.Primitives;
using Serilog;
using Witcher3StringEditor.Dialogs.Messaging;
using Witcher3StringEditor.Locales;
using Witcher3StringEditor.ViewModels;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

namespace Witcher3StringEditor.Views;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : IRecipient<AsyncRequestMessage<bool>>
{
    public MainWindow()
    {
        InitializeComponent();
        SetupSearchHelper();
        RegisterMessageHandlers();
        RegisterThemeChangedHandler();
        DataContext = Ioc.Default.GetService<MainWindowViewModel>();
    }

    public void Receive(AsyncRequestMessage<bool> message)
    {
    }

    private static void RegisterThemeChangedHandler()
    {
        ThemeManager.Current.ActualApplicationThemeChanged += (_, _) =>
        {
            Log.Information("Theme changed to {Theme}", ThemeManager.Current.ActualApplicationTheme);
        };
    }

    private void SetupSearchHelper()
    {
        SfDataGrid.SearchHelper.AllowFiltering = true;
        SfDataGrid.SearchHelper.AllowCaseSensitiveSearch = false;
    }

    private void RegisterMessageHandlers()
    {
        WeakReferenceMessenger.Default.Register<MainWindow, FileOpenedMessage, string>(
            this,
            "ReOpenFile",
            (_, m) =>
            {
                m.Reply(MessageBox.Show(Strings.ReOpenFileMessage, Strings.ReOpenFileCaption, MessageBoxButton.YesNo,
                            MessageBoxImage.Question) ==
                        MessageBoxResult.Yes);
            });

        WeakReferenceMessenger.Default.Register<MainWindow, FileOpenedMessage, string>(
            this,
            "OpenedFileNoFound",
            (_, m) =>
            {
                m.Reply(MessageBox.Show(Strings.FileOpenedNoFoundMessage, Strings.FileOpenedNoFoundCaption,
                            MessageBoxButton.YesNo, MessageBoxImage.Question) ==
                        MessageBoxResult.Yes);
            });

        WeakReferenceMessenger.Default.Register<MainWindow, AsyncRequestMessage<bool>, string>(
            this, "MainWindowClosing", static (_, m) =>
            {
                m.Reply(MessageBox.Show(Strings.AppExitMessage,
                    Strings.AppExitCaption,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question) == MessageBoxResult.No);
            });
        WeakReferenceMessenger.Default.Register<MainWindow, AsyncRequestMessage<bool>, string>(
            this, "FirstRun", static (_, m) =>
            {
                m.Reply(MessageBox.Show(Strings.FristRunMessage,
                    Strings.FristRunCaption,
                    MessageBoxButton.OK,
                    MessageBoxImage.Question) == MessageBoxResult.OK);
            });
    }

    private void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        SfDataGrid.SearchHelper.Search(args.QueryText);
        Log.Information("Search query submitted: {QueryText}", args.QueryText);
    }

    private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (string.IsNullOrEmpty(sender.Text)) SfDataGrid.SearchHelper.ClearSearch();
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        WeakReferenceMessenger.Default.UnregisterAll(this);
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