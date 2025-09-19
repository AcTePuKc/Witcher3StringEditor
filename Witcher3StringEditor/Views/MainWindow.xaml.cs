using System.Collections.Specialized;
using System.Windows;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using iNKORE.UI.WPF.Modern;
using iNKORE.UI.WPF.Modern.Controls;
using iNKORE.UI.WPF.Modern.Controls.Primitives;
using Serilog;
using Syncfusion.Data;
using Witcher3StringEditor.Dialogs.Messaging;
using Witcher3StringEditor.Locales;
using Witcher3StringEditor.Models;
using Witcher3StringEditor.ViewModels;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

namespace Witcher3StringEditor.Views;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();
        SetupSearchHelper();
        RegisterMessageHandlers();
        RegisterThemeChangedHandler();
        DataContext = Ioc.Default.GetService<MainWindowViewModel>();
        SfDataGrid.ItemsSourceChanged += OnDataGridItemsSourceChanged;
    }

    private void OnDataGridItemsSourceChanged(object? sender, EventArgs e)
    {
        SfDataGrid.View.CollectionChanged += OnDataGridViewCollectionChanged;
    }

    private static void OnDataGridViewCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e)
        {
            case { Action: NotifyCollectionChangedAction.Add, NewItems: not null }:
            {
                var addedItems = e.NewItems.OfType<RecordEntry>()
                    .Select(x => x.Data).OfType<W3StringItemModel>().ToList();
                WeakReferenceMessenger.Default.Send(new ValueChangedMessage<IList<W3StringItemModel>>(addedItems),
                    "ItemsAdded");
                break;
            }
            case { Action: NotifyCollectionChangedAction.Remove, OldItems: not null }:
            {
                var removedItems = e.OldItems.OfType<RecordEntry>()
                    .Select(x => x.Data).OfType<W3StringItemModel>().ToList();
                WeakReferenceMessenger.Default.Send(new ValueChangedMessage<IList<W3StringItemModel>>(removedItems),
                    "ItemsRemoved");
                break;
            }
        }
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
        RegisterFileOpenedMessageHandlers();
        RegisterAsyncRequestMessageHandlers();
        RegisterSearchHandler();
    }

    private void RegisterSearchHandler()
    {
        WeakReferenceMessenger.Default.Register<ValueChangedMessage<bool>, string>(this, "ClearSearch",
            (_, _) => { SearchBox.Text = string.Empty; });
        WeakReferenceMessenger.Default.Register<ValueChangedMessage<bool>, string>(this, "RefreshDataGrid",
            (_, _) => { SfDataGrid.View.Refresh(); });
    }

    private void RegisterAsyncRequestMessageHandlers()
    {
        var requestMessageHandlers = new (string, Func<string>, Func<string>, MessageBoxButton, MessageBoxResult)[]
        {
            ("MainWindowClosing", () => Strings.AppExitMessage, () => Strings.AppExitCaption, MessageBoxButton.YesNo,
                MessageBoxResult.No),
            ("FirstRun", () => Strings.FristRunMessage, () => Strings.FristRunCaption, MessageBoxButton.OK,
                MessageBoxResult.OK)
        };

        foreach (var (token, message, caption, button, excepted) in requestMessageHandlers)
            WeakReferenceMessenger.Default.Register<MainWindow, AsyncRequestMessage<bool>, string>(
                this,
                token,
                (_, m) =>
                {
                    m.Reply(MessageBox.Show(message(),
                        caption(),
                        button,
                        MessageBoxImage.Question) == excepted);
                });
    }

    private void RegisterFileOpenedMessageHandlers()
    {
        var messageHandlers = new (string, Func<string>, Func<string>)[]
        {
            ("ReOpenFile", () => Strings.ReOpenFileMessage, () => Strings.ReOpenFileCaption),
            ("OpenedFileNoFound", () => Strings.FileOpenedNoFoundMessage, () => Strings.FileOpenedNoFoundCaption)
        };

        foreach (var (token, message, caption) in messageHandlers)
            WeakReferenceMessenger.Default.Register<MainWindow, AsyncRequestMessage<string, bool>, string>(
                this,
                token,
                (_, m) =>
                {
                    m.Reply(MessageBox.Show(message(), caption(), MessageBoxButton.YesNo, MessageBoxImage.Question) ==
                            MessageBoxResult.Yes);
                });
    }

    private void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        if (SfDataGrid.ItemsSource == null) return;
        SfDataGrid.SearchHelper.Search(args.QueryText);
        var searchResults = SfDataGrid.View.Records
            .Select(x => x.Data).OfType<W3StringItemModel>().ToList();
        WeakReferenceMessenger.Default.Send(new ValueChangedMessage<IList<W3StringItemModel>?>(searchResults),
            "SearchResultsUpdated");
        Log.Information("Search query submitted: {QueryText}", args.QueryText);
    }

    private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (!string.IsNullOrEmpty(sender.Text)) return;
        ClearSearchResults();
    }

    private void ClearSearchResults()
    {
        WeakReferenceMessenger.Default.Send(new ValueChangedMessage<IList<W3StringItemModel>?>(null),
            "SearchResultsUpdated");
        SfDataGrid.SearchHelper.ClearSearch();
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        WeakReferenceMessenger.Default.UnregisterAll(this);
        SfDataGrid.SearchHelper.Dispose();
        SfDataGrid.Dispose();
        SfDataPager.Dispose();
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