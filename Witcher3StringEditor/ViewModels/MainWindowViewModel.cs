using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Threading;
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
    ///     Gets or sets a value indicating whether a search operation is currently active
    ///     This property is used to track search state and control UI behavior during search operations
    /// </summary>
    [ObservableProperty] private bool isSearched;

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
    ///     Gets or sets the page size
    /// </summary>
    [ObservableProperty] private int pageSize;

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
        this.serviceProvider = serviceProvider; // Store the service provider
        appSettings = serviceProvider.GetRequiredService<IAppSettings>(); // Get application settings service
        backupService = serviceProvider.GetRequiredService<IBackupService>(); // Get backup service
        dialogService = serviceProvider.GetRequiredService<IDialogService>(); // Get dialog service
        fileManagerService = serviceProvider.GetRequiredService<IFileManagerService>(); // Get file manager service
        settingsManagerService =
            serviceProvider.GetRequiredService<ISettingsManagerService>(); // Get settings manager service
        PageSize = appSettings.PageSize; // Set page size
        RegisterMessengerHandlers(); // Register all message handlers
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
                (_, _) => OpenFileCommand
                    .NotifyCanExecuteChanged()); // Update OpenFile command state when W3Strings path changes

        // Register handler for GameExe path change notifications
        WeakReferenceMessenger.Default
            .Register<MainWindowViewModel, ValueChangedMessage<bool>, string>(this, "GameExePathChanged",
                (_, _) => PlayGameCommand
                    .NotifyCanExecuteChanged()); // Update PlayGame command state when GameExe path changes
    }

    /// <summary>
    ///     Registers all message handlers for the view model
    /// </summary>
    private void RegisterMessengerHandlers()
    {
        RegisterLogMessageHandlers(); // Register log message handlers
        RegisterFileMessageHandlers(); // Register file message handlers
        RegisterSettingsMessageHandlers(); // Register settings message handlers
        RegisterSearchStateMessageHandler(); // Register search state message handler
    }

    /// <summary>
    ///     Registers message handler for search state change notifications
    ///     Handles messages that indicate when the search state has changed
    /// </summary>
    private void RegisterSearchStateMessageHandler()
    {
        WeakReferenceMessenger.Default.Register<ValueChangedMessage<bool>, string>(this,
            MessageTokens.SearchStateChanged,
            (_, m) => { IsSearched = m.Value; });
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
            async void (_, m) => { await OpenFile(m.Request); }); // Open the requested file
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
            async void (_, m) =>
            {
                await Dispatcher.CurrentDispatcher.InvokeAsync(() => LogEvents.Add(m.Value));
            }); // Add log event to collection on UI thread
    }

    /// <summary>
    ///     Handles the window loaded event
    ///     Logs application startup information and checks for updates
    /// </summary>
    [RelayCommand]
    private async Task WindowLoaded()
    {
        LogApplicationStartupInfo(); // Log application startup information
        await settingsManagerService.CheckSettings(); // Check application settings
        IsUpdateAvailable =
            await serviceProvider.GetRequiredService<ICheckUpdateService>().CheckUpdate(); // Check for updates
    }

    /// <summary>
    ///     Logs application startup information including version, OS, and other diagnostic data
    /// </summary>
    private void LogApplicationStartupInfo()
    {
        Log.Information("Application started."); // Log application start
        Log.Information("Application Version: {Version}", ThisAssembly.AssemblyFileVersion); // Log application version
        Log.Information("OS Version: {Version}", // Log OS version
            $"{RuntimeInformation.OSDescription} ({RuntimeInformation.OSArchitecture})");
        Log.Information(".Net Runtime: {Runtime}", RuntimeInformation.FrameworkDescription); // Log .NET runtime version
        Log.Information("Is Debug: {IsDebug}", IsDebug); // Log debug mode status
        Log.Information("Current Directory: {Directory}", Environment.CurrentDirectory); // Log current directory
        Log.Information("AppData Folder: {Folder}", // Log AppData folder path
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                IsDebug ? "Witcher3StringEditor_Debug" : "Witcher3StringEditor"));
        Log.Information("Installed Language Packs: {Languages}", // Log installed language packs
            string.Join(", ",
                serviceProvider.GetRequiredService<ICultureResolver>().SupportedCultures.Select(x => x.Name)));
        Log.Information("Current Language: {Language}", appSettings.Language); // Log current language
    }

    /// <summary>
    ///     Handles the window closing event
    ///     Cancels the close if there are unsaved changes
    /// </summary>
    /// <param name="e">The event arguments for the cancel event</param>
    [RelayCommand]
    private async Task WindowClosing(CancelEventArgs e)
    {
        if (W3StringItems?.Any() == true && // Check if there are any W3String items
            await WeakReferenceMessenger.Default.Send(new AsyncRequestMessage<bool>(),
                MessageTokens.MainWindowClosing)) // Send close request
            e.Cancel = true; // Cancel window closing if requested
    }

    /// <summary>
    ///     Handles the window closed event
    ///     Unregisters all message handlers for this view model
    /// </summary>
    [RelayCommand]
    private void WindowClosed()
    {
        WeakReferenceMessenger.Default.UnregisterAll(this); // Unregister all message handlers
    }

    /// <summary>
    ///     Handles dropped files
    ///     Opens the file if it has a supported extension
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanOpenFile))]
    private async Task DropFile()
    {
        if (DropFileData?.Length > 0) // Check if any files were dropped
        {
            var file = DropFileData[0]; // Get the first dropped file
            var ext = Path.GetExtension(file); // Get the file extension
            if (ext is ".csv" or ".w3strings" or ".xlsx") await OpenFile(file); // Open file if extension is supported
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
        if (storageFile is not null && Path.GetExtension(storageFile.LocalPath) is ".csv" or ".w3strings" or ".xlsx")
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
            if (!await HandleReOpenFile(fileName)) return; // Handle reopening file logic
            W3StringItems = await fileManagerService.DeserializeW3StringItems(fileName); // Deserialize file contents
            fileManagerService.SetOutputFolder(fileName,
                folder => OutputFolder = folder); // Set output folder based on file location
            fileManagerService.UpdateRecentItems(fileName); // Update recent items list
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to open file: {FileName}.", fileName); // Log any errors during file opening
        }
    }

    /// <summary>
    ///     Handles reopening a file when another file is already open
    /// </summary>
    /// <param name="fileName">The path to the file to reopen</param>
    /// <returns>True if the file can be reopened, false otherwise</returns>
    private async Task<bool> HandleReOpenFile(string fileName)
    {
        if (W3StringItems?.Any() != true) return true; // Return true if no items currently loaded
        return await WeakReferenceMessenger.Default.Send(
            new AsyncRequestMessage<string, bool>(fileName), // Send reopen file request
            MessageTokens.ReOpenFile); // Wait for response
    }

    /// <summary>
    ///     Adds a new The Witcher 3 string item
    /// </summary>
    [RelayCommand(CanExecute = nameof(HasW3StringItems))]
    private async Task Add()
    {
        var dialogViewModel = new EditDataDialogViewModel(new W3StringItemModel()); // Create new item view model
        if (await dialogService.ShowDialogAsync(this, dialogViewModel) == true // Show add dialog
            && dialogViewModel.Item is not null) // Check if user confirmed
        {
            W3StringItems!.Add(dialogViewModel.Item.Cast<W3StringItemModel>()); // Add new item to collection
            Log.Information("New W3Item added."); // Log successful addition
        }
        else
        {
            Log.Information("The W3Item has not been added."); // Log cancelled addition
        }
    }

    /// <summary>
    ///     Edits the selected The Witcher 3 string item
    /// </summary>
    /// <param name="selectedItem">The item to edit</param>
    [RelayCommand(CanExecute = nameof(HasW3StringItems))]
    private async Task Edit(W3StringItemModel selectedItem)
    {
        var dialogViewModel = new EditDataDialogViewModel(selectedItem); // Create edit dialog view model
        if (await dialogService.ShowDialogAsync(this, // Show edit dialog
                dialogViewModel) == true && dialogViewModel.Item is not null) // Check if user confirmed changes
        {
            var found = W3StringItems! // Find the item in the collection
                .First(x => x.TrackingId == selectedItem.TrackingId);
            var index = W3StringItems!.IndexOf(found); // Get the item index
            W3StringItems[index].StrId = dialogViewModel.Item.StrId;
            W3StringItems[index].KeyHex = dialogViewModel.Item.KeyHex;
            W3StringItems[index].KeyName = dialogViewModel.Item.KeyName;
            W3StringItems[index].Text = dialogViewModel.Item.Text;
            if (IsSearched)
                WeakReferenceMessenger.Default.Send(new ValueChangedMessage<bool>(true), MessageTokens.RefreshDataGrid);
            Log.Information("The W3Item has been updated."); // Log successful update
        }
        else
        {
            Log.Information("The W3Item has not been updated."); // Log cancelled update
        }
    }

    /// <summary>
    ///     Deletes the selected The Witcher 3 string items
    /// </summary>
    /// <param name="selectedItems">The items to delete</param>
    [RelayCommand(CanExecute = nameof(HasW3StringItems))]
    private async Task Delete(IEnumerable<object> selectedItems)
    {
        var w3Items = selectedItems.OfType<ITrackableW3StringItem>().ToArray(); // Filter and convert to trackable items
        if (w3Items.Length > 0 && // Check if any items to delete
            await dialogService.ShowDialogAsync(this, new DeleteDataDialogViewModel(w3Items)) ==
            true) // Show confirmation dialog
            w3Items.ForEach(item =>
            {
                var stringItem = item.Cast<W3StringItemModel>(); // Cast to string item model
                W3StringItems!.Remove(stringItem); // Remove from main collection
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
        using var logDialogViewModel = new LogDialogViewModel(LogEvents);
        await dialogService.ShowDialogAsync<LogDialogViewModel>(this, logDialogViewModel);
    }

    /// <summary>
    ///     Shows the settings dialog
    /// </summary>
    [RelayCommand]
    private async Task ShowSettingsDialog()
    {
        var translators = serviceProvider.GetServices<ITranslator>().ToArray(); // Get all available translators
        var names = translators.Select(x => x.Name); // Extract translator names
        translators.ForEach(x => x.Cast<IDisposable>().Dispose()); // Dispose of translator instances
        await dialogService.ShowDialogAsync(this, // Show the settings dialog
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
        await dialogService.ShowDialogAsync(this, // Show the about dialog
            new AboutDialogViewModel(new Dictionary<string, object?> // Create view model with application information
            {
                { "Version", ThisAssembly.AssemblyInformationalVersion }, // Application version
                { "BuildTime", BuildTimestamp.BuildTime.ToLocalTime() }, // Build time
                { "OS", $"{RuntimeInformation.OSDescription} ({RuntimeInformation.OSArchitecture})" }, // OS information
                { "Runtime", RuntimeInformation.FrameworkDescription }, // Runtime information
                {
                    "Package", DependencyContext.Default? // Package information
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
        using var recentDialogViewModel = new RecentDialogViewModel(appSettings);
        await dialogService.ShowDialogAsync(this, recentDialogViewModel);
    }

    /// <summary>
    ///     Shows the translate dialog
    /// </summary>
    /// <param name="selectedItem">The initially selected item in the dialog</param>
    [RelayCommand(CanExecute = nameof(HasW3StringItems))]
    private async Task ShowTranslateDialog(IW3StringItem? selectedItem)
    {
        var selectedIndex =
            selectedItem is not null
                ? W3StringItems.IndexOf(selectedItem)
                : 0; // Set selected index based on provided item
        var translator = serviceProvider.GetServices<ITranslator>() // Get the configured translator
            .First(x => x.Name == appSettings.Translator);
        await dialogService.ShowDialogAsync(this, // Show the translate dialog
            new TranslateDialogViewModel(appSettings, translator, W3StringItems!, selectedIndex));
        if (translator is IDisposable disposable) disposable.Dispose(); // Dispose of the translator if it's disposable
        if (IsSearched)
            WeakReferenceMessenger.Default.Send(new ValueChangedMessage<bool>(true), MessageTokens.RefreshDataGrid);
    }
}