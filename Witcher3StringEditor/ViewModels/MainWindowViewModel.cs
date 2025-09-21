using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using cmdwtf;
using CommandLine;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using GTranslate.Translators;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using Serilog;
using Serilog.Events;
using Syncfusion.Data.Extensions;
using Witcher3StringEditor.Common.Abstractions;
using Witcher3StringEditor.Common.Constants;
using Witcher3StringEditor.Dialogs.Messaging;
using Witcher3StringEditor.Dialogs.ViewModels;
using Witcher3StringEditor.Locales;
using Witcher3StringEditor.Serializers.Abstractions;
using Witcher3StringEditor.Services;
using W3StringItemModel = Witcher3StringEditor.Models.W3StringItemModel;

namespace Witcher3StringEditor.ViewModels;

/// <summary>
///     Main window view model for the Witcher 3 String Editor application
///     Handles file operations, UI commands, and coordination between services and dialogs
/// </summary>
internal partial class MainWindowViewModel : ObservableObject
{
    private readonly IAppSettings appSettings;
    private readonly IBackupService backupService;
    private readonly IDialogService dialogService;
    private readonly IFileManagerService fileManagerService;
    private readonly IServiceProvider serviceProvider;
    private readonly ISettingsManagerService settingsManagerService;

    /// <summary>
    ///     Gets or sets the data from dropped files
    /// </summary>
    [ObservableProperty] private string[]? dropFileData;

    /// <summary>
    ///     Gets or sets a value indicating whether an update is available
    /// </summary>
    [ObservableProperty] private bool isUpdateAvailable;

    /// <summary>
    ///     Gets or sets the output folder path
    ///     Notifies OpenWorkingFolderCommand when this property changes
    /// </summary>
    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(OpenWorkingFolderCommand))]
    private string outputFolder = string.Empty;

    /// <summary>
    ///     Gets or sets the search results collection
    ///     Notifies ShowTranslateDialogCommand when this property changes
    /// </summary>
    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(ShowTranslateDialogCommand))]
    private ObservableCollection<W3StringItemModel>? searchResults;

    /// <summary>
    ///     Gets or sets the collection of The Witcher 3 string items
    ///     Notifies multiple commands when this property changes
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddCommand))]
    [NotifyCanExecuteChangedFor(nameof(EditCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
    [NotifyCanExecuteChangedFor(nameof(ShowSaveDialogCommand))]
    [NotifyCanExecuteChangedFor(nameof(ShowTranslateDialogCommand))]
    private ObservableCollection<W3StringItemModel>? w3StringItems;

    /// <summary>
    ///     Initializes a new instance of the MainWindowViewModel class
    /// </summary>
    /// <param name="serviceProvider">The service provider used to resolve dependencies</param>
    public MainWindowViewModel(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
        appSettings = serviceProvider.GetRequiredService<IAppSettings>();
        backupService = serviceProvider.GetRequiredService<IBackupService>();
        dialogService = serviceProvider.GetRequiredService<IDialogService>();
        fileManagerService = serviceProvider.GetRequiredService<IFileManagerService>();
        settingsManagerService = serviceProvider.GetRequiredService<ISettingsManagerService>();
        RegisterMessengerHandlers();
    }

    /// <summary>
    ///     Gets the collection of log events
    /// </summary>
    private ObservableCollection<LogEvent> LogEvents { get; } = [];

    /// <summary>
    ///     Gets a value indicating whether there are The Witcher 3 string items
    /// </summary>
    private bool HasW3StringItems => W3StringItems?.Any() == true;

    /// <summary>
    ///     Gets a value indicating whether the working folder can be opened
    /// </summary>
    private bool CanOpenWorkingFolder
        => Directory.Exists(OutputFolder);

    /// <summary>
    ///     Gets a value indicating whether the game can be played
    /// </summary>
    private bool CanPlayGame => File.Exists(appSettings.GameExePath);

    /// <summary>
    ///     Gets a value indicating whether a file can be opened
    /// </summary>
    private bool CanOpenFile => File.Exists(appSettings.W3StringsPath);

    /// <summary>
    ///     Gets a value indicating whether the application is running in debug mode
    /// </summary>
    private static bool IsDebug =>
        Assembly.GetExecutingAssembly().GetCustomAttribute<DebuggableAttribute>()?.IsJITTrackingEnabled == true;

    /// <summary>
    ///     Registers message handlers for settings-related messages
    /// </summary>
    private void RegisterSettingsMessageHandlers()
    {
        // Register handler for W3Strings path change notifications
        WeakReferenceMessenger.Default
            .Register<MainWindowViewModel, ValueChangedMessage<bool>, string>(this, "W3StringsPathChanged",
                (_, _) => OpenFileCommand.NotifyCanExecuteChanged());
        
        // Register handler for GameExe path change notifications
        WeakReferenceMessenger.Default
            .Register<MainWindowViewModel, ValueChangedMessage<bool>, string>(this, "GameExePathChanged",
                (_, _) => PlayGameCommand.NotifyCanExecuteChanged());
    }

    /// <summary>
    ///     Registers all message handlers for the view model
    /// </summary>
    private void RegisterMessengerHandlers()
    {
        RegisterLogMessageHandlers();
        RegisterFileMessageHandlers();
        RegisterSearchMessageHandlers();
        RegisterSettingsMessageHandlers();
        RegisterTranslationMessageHandlers();
    }

    /// <summary>
    ///     Registers message handlers for search-related messages
    /// </summary>
    private void RegisterSearchMessageHandlers()
    {
        // Register handler for search results updates
        WeakReferenceMessenger.Default
            .Register<MainWindowViewModel, ValueChangedMessage<IList<W3StringItemModel>?>, string>(this,
                "SearchResultsUpdated", (_, m) =>
                {
                    var searchItems = m.Value;
                    SearchResults = searchItems != null ? m.Value.ToObservableCollection() : null;
                });
        
        // Register handler for items added notifications
        WeakReferenceMessenger.Default
            .Register<MainWindowViewModel, ValueChangedMessage<IList<W3StringItemModel>>, string>(this,
                "ItemsAdded", (_, m) =>
                {
                    if (SearchResults == null) return;
                    var addedItems = m.Value;
                    if (!addedItems.Any()) return;
                    addedItems.ForEach(SearchResults.Add);
                    ShowTranslateDialogCommand.NotifyCanExecuteChanged();
                });
        
        // Register handler for items removed notifications
        WeakReferenceMessenger.Default
            .Register<MainWindowViewModel, ValueChangedMessage<IList<W3StringItemModel>>, string>(this,
                "ItemsRemoved", (_, m) =>
                {
                    var removedItems = m.Value;
                    if (!removedItems.Any()) return;
                    removedItems.ForEach(x => SearchResults?.Remove(x));
                    ShowTranslateDialogCommand.NotifyCanExecuteChanged();
                });
    }

    /// <summary>
    ///     Registers message handlers for translation-related messages
    /// </summary>
    private void RegisterTranslationMessageHandlers()
    {
        // Register handler for translation saved notifications
        WeakReferenceMessenger.Default
            .Register<MainWindowViewModel, ValueChangedMessage<ITrackableW3StringItem>, string>(
                this,
                MessageTokens.TranslationSaved,
                (_, m) =>
                {
                    var item = m.Value;
                    var found = W3StringItems!
                        .First(x => x.TrackingId == item.TrackingId);
                    found.Text = item.Text;
                });
    }

    /// <summary>
    ///     Registers message handlers for file-related messages
    /// </summary>
    private void RegisterFileMessageHandlers()
    {
        // Register handler for recent file opened notifications
        WeakReferenceMessenger.Default.Register<MainWindowViewModel, AsyncRequestMessage<string, bool>, string>(
            // ReSharper disable once AsyncVoidMethod
            this,
            MessageTokens.RecentFileOpened,
            async void (_, m) => { await OpenFile(m.Request); });
    }

    /// <summary>
    ///     Registers message handlers for log-related messages
    /// </summary>
    private void RegisterLogMessageHandlers()
    {
        // Register handler for log event notifications
        WeakReferenceMessenger.Default.Register<MainWindowViewModel, ValueChangedMessage<LogEvent>>(
            // ReSharper disable once AsyncVoidMethod
            this,
            async void (_, m) => { await Application.Current.Dispatcher.InvokeAsync(() => LogEvents.Add(m.Value)); });
    }

    /// <summary>
    ///     Handles the window loaded event
    ///     Logs application startup information and checks for updates
    /// </summary>
    [RelayCommand]
    private async Task WindowLoaded()
    {
        LogApplicationStartupInfo();
        await settingsManagerService.CheckSettings();
        IsUpdateAvailable = await serviceProvider.GetRequiredService<ICheckUpdateService>().CheckUpdate();
    }

    /// <summary>
    ///     Logs application startup information including version, OS, and other diagnostic data
    /// </summary>
    private void LogApplicationStartupInfo()
    {
        Log.Information("Application started.");
        Log.Information("Application Version: {Version}", ThisAssembly.AssemblyFileVersion);
        Log.Information("OS Version: {Version}",
            $"{RuntimeInformation.OSDescription} ({RuntimeInformation.OSArchitecture})");
        Log.Information(".Net Runtime: {Runtime}", RuntimeInformation.FrameworkDescription);
        Log.Information("Is Debug: {IsDebug}", IsDebug);
        Log.Information("Current Directory: {Directory}", Environment.CurrentDirectory);
        Log.Information("AppData Folder: {Folder}",
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                IsDebug ? "Witcher3StringEditor_Debug" : "Witcher3StringEditor"));
        Log.Information("Installed Language Packs: {Languages}",
            string.Join(", ",
                serviceProvider.GetRequiredService<ICultureResolver>().SupportedCultures.Select(x => x.Name)));
        Log.Information("Current Language: {Language}", appSettings.Language);
    }

    /// <summary>
    ///     Handles the window closing event
    ///     Cancels the close if there are unsaved changes
    /// </summary>
    /// <param name="e">The event arguments for the cancel event</param>
    [RelayCommand]
    private async Task WindowClosing(CancelEventArgs e)
    {
        if (W3StringItems?.Any() == true &&
            await WeakReferenceMessenger.Default.Send(new AsyncRequestMessage<bool>(), MessageTokens.MainWindowClosing))
            e.Cancel = true;
    }

    /// <summary>
    ///     Handles the window closed event
    ///     Unregisters all message handlers for this view model
    /// </summary>
    [RelayCommand]
    private void WindowClosed()
    {
        // Check if there are any W3StringItems loaded and send a message to check for unsaved changes
        WeakReferenceMessenger.Default.UnregisterAll(this);
    }

    /// <summary>
    ///     Handles dropped files
    ///     Opens the file if it has a supported extension
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanOpenFile))]
    private async Task DropFile()
    {
        if (DropFileData?.Length > 0)
        {
            var file = DropFileData[0];
            var ext = Path.GetExtension(file);
            if (ext is ".csv" or ".w3strings" or ".xlsx") await OpenFile(file);
        }
    }

    /// <summary>
    ///     Opens a file using a file dialog
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanOpenFile))]
    private async Task OpenFile()
    {
        using var storageFile = await dialogService.ShowOpenFileDialogAsync(this, new OpenFileDialogSettings
        {
            Filters =
            [
                new FileFilter(Strings.FileFormatSupported, [".csv", ".xlsx", ".w3strings"]),
                new FileFilter(Strings.FileFormatTextFile, ".csv"),
                new FileFilter(Strings.FileFormatExcelWorkbook, ".xlsx"),
                new FileFilter(Strings.FileFormatWitcher3StringsFile, ".w3strings")
            ]
        });
        if (storageFile != null && Path.GetExtension(storageFile.LocalPath) is ".csv" or ".w3strings" or ".xlsx")
            await OpenFile(storageFile.LocalPath);
    }

    /// <summary>
    ///     Opens the specified file and loads its contents
    /// </summary>
    /// <param name="fileName">The path to the file to open</param>
    private async Task OpenFile(string fileName)
    {
        try
        {
            if (!await HandleReOpenFile(fileName)) return;
            W3StringItems = await fileManagerService.DeserializeW3StringItems(fileName);
            fileManagerService.SetOutputFolder(fileName, folder => OutputFolder = folder);
            fileManagerService.UpdateRecentItems(fileName);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to open file: {FileName}.", fileName);
        }
    }

    /// <summary>
    ///     Handles reopening a file when another file is already open
    /// </summary>
    /// <param name="fileName">The path to the file to reopen</param>
    /// <returns>True if the file can be reopened, false otherwise</returns>
    private async Task<bool> HandleReOpenFile(string fileName)
    {
        if (W3StringItems?.Any() != true) return true;
        if (!await WeakReferenceMessenger.Default.Send(new AsyncRequestMessage<string, bool>(fileName),
                MessageTokens.ReOpenFile)) return false;
        WeakReferenceMessenger.Default.Send(new ValueChangedMessage<bool>(true), MessageTokens.ClearSearch);
        return true;
    }

    /// <summary>
    ///     Adds a new The Witcher 3 string item
    /// </summary>
    [RelayCommand(CanExecute = nameof(HasW3StringItems))]
    private async Task Add()
    {
        var dialogViewModel = new EditDataDialogViewModel(new W3StringItemModel());
        if (await dialogService.ShowDialogAsync(this, dialogViewModel) == true
            && dialogViewModel.Item != null)
        {
            W3StringItems!.Add(dialogViewModel.Item.Cast<W3StringItemModel>());
            Log.Information("New W3Item added.");
        }
        else
        {
            Log.Information("The W3Item has not been added.");
        }
    }

    /// <summary>
    ///     Edits the selected The Witcher 3 string item
    /// </summary>
    /// <param name="selectedItem">The item to edit</param>
    [RelayCommand(CanExecute = nameof(HasW3StringItems))]
    private async Task Edit(W3StringItemModel selectedItem)
    {
        var dialogViewModel = new EditDataDialogViewModel(selectedItem);
        if (await dialogService.ShowDialogAsync(this,
                dialogViewModel) == true && dialogViewModel.Item != null)
        {
            var found = W3StringItems!
                .First(x => x.TrackingId == selectedItem.TrackingId);
            var index = W3StringItems!.IndexOf(found);
            W3StringItems[index] = dialogViewModel.Item.Cast<W3StringItemModel>();
            Log.Information("The W3Item has been updated.");
        }
        else
        {
            Log.Information("The W3Item has not been updated.");
        }
    }

    /// <summary>
    ///     Deletes the selected The Witcher 3 string items
    /// </summary>
    /// <param name="selectedItems">The items to delete</param>
    [RelayCommand(CanExecute = nameof(HasW3StringItems))]
    private async Task Delete(IEnumerable<object> selectedItems)
    {
        var w3Items = selectedItems.OfType<ITrackableW3StringItem>().ToArray();
        if (w3Items.Length > 0 &&
            await dialogService.ShowDialogAsync(this, new DeleteDataDialogViewModel(w3Items)) == true)
            w3Items.ForEach(item =>
            {
                var stringItem = item.Cast<W3StringItemModel>();
                W3StringItems!.Remove(stringItem);
                SearchResults?.Remove(stringItem);
            });
    }

    /// <summary>
    ///     Shows the backup dialog
    /// </summary>
    [RelayCommand]
    private async Task ShowBackupDialog()
    {
        await dialogService.ShowDialogAsync(this,
            new BackupDialogViewModel(appSettings, backupService));
    }

    /// <summary>
    ///     Shows the save dialog
    /// </summary>
    [RelayCommand(CanExecute = nameof(HasW3StringItems))]
    private async Task ShowSaveDialog()
    {
        await dialogService.ShowDialogAsync(this,
            new SaveDialogViewModel(appSettings,
                serviceProvider.GetRequiredService<IW3Serializer>(),
                W3StringItems!,
                OutputFolder));
    }

    /// <summary>
    ///     Shows the log dialog
    /// </summary>
    [RelayCommand]
    private async Task ShowLogDialog()
    {
        await dialogService.ShowDialogAsync<LogDialogViewModel>(this,
            new LogDialogViewModel(LogEvents));
    }

    /// <summary>
    ///     Shows the settings dialog
    /// </summary>
    [RelayCommand]
    private async Task ShowSettingsDialog()
    {
        var translators = serviceProvider.GetServices<ITranslator>().ToArray();
        var names = translators.Select(x => x.Name);
        translators.ForEach(x => x.Cast<IDisposable>().Dispose());
        await dialogService.ShowDialogAsync(this,
            new SettingDialogViewModel(appSettings, dialogService, names,
                serviceProvider.GetRequiredService<ICultureResolver>().SupportedCultures));
    }

    /// <summary>
    ///     Starts the game process
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanPlayGame))]
    private async Task PlayGame()
    {
        await serviceProvider.GetRequiredService<IPlayGameService>().PlayGame();
    }

    /// <summary>
    ///     Shows the about dialog
    /// </summary>
    [RelayCommand]
    private async Task ShowAbout()
    {
        await dialogService.ShowDialogAsync(this,
            new AboutDialogViewModel(new Dictionary<string, object?>
            {
                { "Version", ThisAssembly.AssemblyInformationalVersion },
                { "BuildTime", BuildTimestamp.BuildTime.ToLocalTime() },
                { "OS", $"{RuntimeInformation.OSDescription} ({RuntimeInformation.OSArchitecture})" },
                { "Runtime", RuntimeInformation.FrameworkDescription },
                {
                    "Package", DependencyContext.Default?
                        .RuntimeLibraries.Where(static x => x.Type == "package")
                }
            }));
    }

    /// <summary>
    ///     Opens the working folder in Windows Explorer
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanOpenWorkingFolder))]
    private void OpenWorkingFolder()
    {
        serviceProvider.GetRequiredService<IExplorerService>().Open(OutputFolder);
        Log.Information("Working folder opened.");
    }

    /// <summary>
    ///     Opens the NexusMods page in the default browser
    /// </summary>
    [RelayCommand]
    private void OpenNexusMods()
    {
        serviceProvider.GetRequiredService<IExplorerService>()
            .Open(appSettings.NexusModUrl);
        Log.Information("NexusMods opened.");
    }

    /// <summary>
    ///     Shows the recent files dialog
    /// </summary>
    [RelayCommand]
    private async Task ShowRecentDialog()
    {
        await dialogService.ShowDialogAsync(this,
            new RecentDialogViewModel(appSettings));
    }

    /// <summary>
    ///     Determines whether the translate dialog can be shown
    /// </summary>
    /// <returns>True if the translate dialog can be shown, false otherwise</returns>
    private bool CanShowTranslateDialog()
    {
        if (SearchResults != null)
            return SearchResults.Any();
        return W3StringItems?.Any() == true;
    }

    /// <summary>
    ///     Shows the translate dialog
    /// </summary>
    /// <param name="selectedItem">The initially selected item in the dialog</param>
    [RelayCommand(CanExecute = nameof(CanShowTranslateDialog))]
    private async Task ShowTranslateDialog(IW3StringItem? selectedItem)
    {        
        // Determine which items to use (search results or all items)
        var items = SearchResults ?? W3StringItems!;
        var itemsList = items.OfType<ITrackableW3StringItem>().ToList();
        var selectedIndex = selectedItem != null ? itemsList.IndexOf(selectedItem) : 0;
        
        var translator = serviceProvider.GetServices<ITranslator>()
            .First(x => x.Name == appSettings.Translator);
        await dialogService.ShowDialogAsync(this,
            new TranslateDialogViewModel(appSettings, translator, itemsList, selectedIndex));
        if (translator is IDisposable disposable) disposable.Dispose();
        
        // If we're showing search results, refresh the data grid
        if (SearchResults != null)
            WeakReferenceMessenger.Default.Send(new ValueChangedMessage<bool>(true), MessageTokens.RefreshDataGrid);
    }
}