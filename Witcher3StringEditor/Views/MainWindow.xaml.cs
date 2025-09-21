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
using Witcher3StringEditor.Common.Constants;
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
    /// <summary>
    ///     Initializes a new instance of the MainWindow class
    ///     Sets up the main window components, data grid, and message handlers
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();
        SetupSearchHelper();
        RegisterMessageHandlers();
        RegisterThemeChangedHandler();
        DataContext = Ioc.Default.GetService<MainWindowViewModel>();
        SfDataGrid.ItemsSourceChanged += OnDataGridItemsSourceChanged;
    }

    /// <summary>
    ///     Handles the ItemsSourceChanged event of the data grid
    ///     Registers a handler for collection changes in the data grid view
    /// </summary>
    /// <param name="sender">The source of the event</param>
    /// <param name="e">The event arguments</param>
    private void OnDataGridItemsSourceChanged(object? sender, EventArgs e)
    {
        SfDataGrid.View.CollectionChanged += OnDataGridViewCollectionChanged;
    }

    /// <summary>
    ///     Handles the CollectionChanged event of the data grid view
    ///     Sends messages when items are added or removed from the data grid
    /// </summary>
    /// <param name="sender">The source of the event</param>
    /// <param name="e">The event arguments containing information about the change</param>
    private static void OnDataGridViewCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e)
        {
            case { Action: NotifyCollectionChangedAction.Add, NewItems: not null }:
            {
                var addedItems = e.NewItems.OfType<RecordEntry>()
                    .Select(x => x.Data).OfType<W3StringItemModel>().ToList();
                WeakReferenceMessenger.Default.Send(new ValueChangedMessage<IList<W3StringItemModel>>(addedItems),
                    MessageTokens.ItemsAdded);
                break;
            }
            case { Action: NotifyCollectionChangedAction.Remove, OldItems: not null }:
            {
                var removedItems = e.OldItems.OfType<RecordEntry>()
                    .Select(x => x.Data).OfType<W3StringItemModel>().ToList();
                WeakReferenceMessenger.Default.Send(new ValueChangedMessage<IList<W3StringItemModel>>(removedItems),
                    MessageTokens.ItemsRemoved);
                break;
            }
        }
    }

    /// <summary>
    ///     Registers a handler for theme change events
    ///     Logs the theme change when it occurs
    /// </summary>
    private static void RegisterThemeChangedHandler()
    {
        ThemeManager.Current.ActualApplicationThemeChanged += (_, _) =>
        {
            Log.Information("Theme changed to {Theme}", ThemeManager.Current.ActualApplicationTheme);
        };
    }

    /// <summary>
    ///     Sets up the search helper for the data grid
    ///     Configures filtering and case sensitivity options
    /// </summary>
    private void SetupSearchHelper()
    {
        SfDataGrid.SearchHelper.AllowFiltering = true;
        SfDataGrid.SearchHelper.AllowCaseSensitiveSearch = false;
    }

    /// <summary>
    ///     Registers all message handlers for the main window
    /// </summary>
    private void RegisterMessageHandlers()
    {
        RegisterFileOpenedMessageHandlers();
        RegisterAsyncRequestMessageHandlers();
        RegisterSearchHandler();
    }

    /// <summary>
    ///     Registers message handlers for search-related operations
    ///     Handles clearing search and refreshing the data grid
    /// </summary>
    private void RegisterSearchHandler()
    {
        WeakReferenceMessenger.Default.Register<ValueChangedMessage<bool>, string>(this, MessageTokens.ClearSearch,
            (_, _) => { SearchBox.Text = string.Empty; });
        WeakReferenceMessenger.Default.Register<ValueChangedMessage<bool>, string>(this, MessageTokens.RefreshDataGrid,
            (_, _) => { SfDataGrid.View.Refresh(); });
    }

    /// <summary>
    ///     Registers message handlers for asynchronous request messages
    ///     Handles messages for main window closing and first run scenarios
    /// </summary>
    private void RegisterAsyncRequestMessageHandlers()
    {
        var requestMessageHandlers = new (string, Func<string>, Func<string>, MessageBoxButton, MessageBoxResult)[]
        {
            (MessageTokens.MainWindowClosing, () => Strings.AppExitMessage, () => Strings.AppExitCaption,
                MessageBoxButton.YesNo,
                MessageBoxResult.No),
            (MessageTokens.FirstRun, () => Strings.FristRunMessage, () => Strings.FristRunCaption, MessageBoxButton.OK,
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

    /// <summary>
    ///     Registers message handlers for file open requests
    ///     Handles messages for reopening files and handling missing files
    /// </summary>
    private void RegisterFileOpenedMessageHandlers()
    {
        var messageHandlers = new (string, Func<string>, Func<string>)[]
        {
            (MessageTokens.ReOpenFile, () => Strings.ReOpenFileMessage, () => Strings.ReOpenFileCaption),
            (MessageTokens.OpenedFileNoFound, () => Strings.FileOpenedNoFoundMessage,
                () => Strings.FileOpenedNoFoundCaption)
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

    /// <summary>
    ///     Handles the QuerySubmitted event of the search box
    ///     Performs a search in the data grid and sends the results
    /// </summary>
    /// <param name="sender">The source of the event</param>
    /// <param name="args">The event arguments containing the query text</param>
    private void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        if (SfDataGrid.ItemsSource == null) return;
        SfDataGrid.SearchHelper.Search(args.QueryText);
        var searchResults = SfDataGrid.View.Records
            .Select(x => x.Data).OfType<W3StringItemModel>().ToList();
        WeakReferenceMessenger.Default.Send(new ValueChangedMessage<IList<W3StringItemModel>?>(searchResults),
            MessageTokens.SearchResultsUpdated);
        Log.Information("Search query submitted: {QueryText}", args.QueryText);
    }

    /// <summary>
    ///     Handles the TextChanged event of the search box
    ///     Clears the search when the text is empty
    /// </summary>
    /// <param name="sender">The source of the event</param>
    /// <param name="args">The event arguments</param>
    private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (!string.IsNullOrEmpty(sender.Text)) return;
        WeakReferenceMessenger.Default.Send(new ValueChangedMessage<IList<W3StringItemModel>?>(null),
            MessageTokens.SearchResultsUpdated);
        SfDataGrid.SearchHelper.ClearSearch();
    }

    /// <summary>
    ///     Handles the Closed event of the window
    ///     Unregisters message handlers and disposes resources
    /// </summary>
    /// <param name="sender">The source of the event</param>
    /// <param name="e">The event arguments</param>
    private void Window_Closed(object sender, EventArgs e)
    {
        WeakReferenceMessenger.Default.UnregisterAll(this);
        SfDataGrid.SearchHelper.Dispose();
        SfDataGrid.Dispose();
        SfDataPager.Dispose();
    }

    /// <summary>
    ///     Handles the Loaded event of the app title bar
    ///     Sets up regions for custom title bar if extended view is enabled
    /// </summary>
    /// <param name="sender">The source of the event</param>
    /// <param name="e">The event arguments</param>
    private void AppTitleBar_OnLoaded(object sender, RoutedEventArgs e)
    {
        if (TitleBar.GetExtendViewIntoTitleBar(this)) SetRegionsForCustomTitleBar();
    }

    /// <summary>
    ///     Handles the SizeChanged event of the app title bar
    ///     Updates regions for custom title bar when size changes
    /// </summary>
    /// <param name="sender">The source of the event</param>
    /// <param name="e">The event arguments containing size change information</param>
    private void AppTitleBar_OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (TitleBar.GetExtendViewIntoTitleBar(this)) SetRegionsForCustomTitleBar();
    }

    /// <summary>
    ///     Sets regions for custom title bar
    ///     Adjusts the right padding column width based on system overlay inset
    /// </summary>
    private void SetRegionsForCustomTitleBar()
    {
        RightPaddingColumn.Width = new GridLength(TitleBar.GetSystemOverlayRightInset(this));
    }

    /// <summary>
    ///     Handles the Click event of the theme switch button
    ///     Toggles between light and dark themes
    /// </summary>
    /// <param name="sender">The source of the event</param>
    /// <param name="e">The event arguments</param>
    private void ThemeSwitchBtn_OnClick(object sender, RoutedEventArgs e)
    {
        ThemeManager.Current.ApplicationTheme =
            ThemeManager.Current.ActualApplicationTheme == ApplicationTheme.Light
                ? ApplicationTheme.Dark
                : ApplicationTheme.Light;
    }
}