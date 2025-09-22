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
        InitializeComponent(); // Initialize the UI components
        SetupSearchHelper(); // Set up the search helper functionality
        RegisterMessageHandlers(); // Register message handlers for inter-component communication
        RegisterThemeChangedHandler(); // Register handler for theme change notifications
        DataContext = Ioc.Default.GetService<MainWindowViewModel>(); // Set the data context to the main view model
        SfDataGrid.ItemsSourceChanged +=
            OnDataGridItemsSourceChanged; // Register event handler for data grid items source changes
    }

    /// <summary>
    ///     Handles the ItemsSourceChanged event of the data grid
    ///     Registers a handler for collection changes in the data grid view
    /// </summary>
    /// <param name="sender">The source of the event</param>
    /// <param name="e">The event arguments</param>
    private void OnDataGridItemsSourceChanged(object? sender, EventArgs e)
    {
        // Subscribe to collection changes in the data grid view.
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
        // Process add or remove actions in the data grid collection.
        switch (e)
        {
            case { Action: NotifyCollectionChangedAction.Add, NewItems: not null }: // Handle item addition
            {
                var addedItems = e.NewItems.OfType<RecordEntry>() // Convert to RecordEntry objects
                    .Select(x => x.Data).OfType<W3StringItemModel>().ToList(); // Extract W3StringItemModel data
                WeakReferenceMessenger.Default.Send(
                    new ValueChangedMessage<IList<W3StringItemModel>>(addedItems), // Send added items message
                    MessageTokens.ItemsAdded);
                break;
            }
            case { Action: NotifyCollectionChangedAction.Remove, OldItems: not null }: // Handle item removal
            {
                var removedItems = e.OldItems.OfType<RecordEntry>() // Convert to RecordEntry objects
                    .Select(x => x.Data).OfType<W3StringItemModel>().ToList(); // Extract W3StringItemModel data
                WeakReferenceMessenger.Default.Send(
                    new ValueChangedMessage<IList<W3StringItemModel>>(removedItems), // Send removed items message
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
        // Subscribe to theme change events and log the new theme
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
        // Enable filtering and disable case-sensitive search for the data grid search helper
        SfDataGrid.SearchHelper.AllowFiltering = true;
        SfDataGrid.SearchHelper.AllowCaseSensitiveSearch = false;
    }

    /// <summary>
    ///     Registers all message handlers for the main window
    /// </summary>
    private void RegisterMessageHandlers()
    {
        RegisterFileOpenedMessageHandlers(); // Register file opened message handlers
        RegisterAsyncRequestMessageHandlers(); // Register async request message handlers
        RegisterSearchHandler(); // Register search-related message handlers
    }

    /// <summary>
    ///     Registers message handlers for search-related operations
    ///     Handles clearing search and refreshing the data grid
    /// </summary>
    private void RegisterSearchHandler()
    {
        // Register handler to clear search text when requested
        WeakReferenceMessenger.Default.Register<ValueChangedMessage<bool>, string>(this, MessageTokens.ClearSearch,
            (_, _) => { SearchBox.Text = string.Empty; });

        // Register handler to refresh the data grid view when requested
        WeakReferenceMessenger.Default.Register<ValueChangedMessage<bool>, string>(this, MessageTokens.RefreshDataGrid,
            (_, _) => { SfDataGrid.View.Refresh(); });
    }

    /// <summary>
    ///     Registers message handlers for asynchronous request messages
    ///     Handles messages for main window closing and first run scenarios
    /// </summary>
    private void RegisterAsyncRequestMessageHandlers()
    {
        // Define message handlers for different scenarios
        var requestMessageHandlers = new (string, Func<string>, Func<string>, MessageBoxButton, MessageBoxResult)[]
        {
            (MessageTokens.MainWindowClosing, () => Strings.AppExitMessage, () => Strings.AppExitCaption,
                MessageBoxButton.YesNo,
                MessageBoxResult.No),
            (MessageTokens.FirstRun, () => Strings.FristRunMessage, () => Strings.FristRunCaption, MessageBoxButton.OK,
                MessageBoxResult.OK)
        };

        // Register handlers for each scenario
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
        // Define message handlers for file operations
        var messageHandlers = new (string, Func<string>, Func<string>)[]
        {
            (MessageTokens.ReOpenFile, () => Strings.ReOpenFileMessage, () => Strings.ReOpenFileCaption),
            (MessageTokens.OpenedFileNoFound, () => Strings.FileOpenedNoFoundMessage,
                () => Strings.FileOpenedNoFoundCaption)
        };

        // Register handlers for each file operation scenario
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
        // Ensure there's data to search before proceeding
        if (SfDataGrid.ItemsSource == null) return;

        // Perform the search and collect results
        SfDataGrid.SearchHelper.Search(args.QueryText);
        var searchResults = SfDataGrid.View.Records
            .Select(x => x.Data).OfType<W3StringItemModel>().ToList();

        // Send search results to other components
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
        if (!string.IsNullOrEmpty(sender.Text)) return; // Return if search text is not empty
        WeakReferenceMessenger.Default.Send(
            new ValueChangedMessage<IList<W3StringItemModel>?>(null), // Send null search results
            MessageTokens.SearchResultsUpdated);
        SfDataGrid.SearchHelper.ClearSearch(); // Clear the search helper results
    }

    /// <summary>
    ///     Handles the Closed event of the window
    ///     Unregisters message handlers and disposes resources
    /// </summary>
    /// <param name="sender">The source of the event</param>
    /// <param name="e">The event arguments</param>
    private void Window_Closed(object sender, EventArgs e)
    {
        // Clean up resources and unregister message handlers
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
        // Configure regions for custom title bar if needed
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
        // Toggle between light and dark themes
        ThemeManager.Current.ApplicationTheme =
            ThemeManager.Current.ActualApplicationTheme == ApplicationTheme.Light
                ? ApplicationTheme.Dark
                : ApplicationTheme.Light;
    }
}