using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using cmdwtf;
using CommandLine;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using GTranslate.Translators;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using Microsoft.Extensions.DependencyModel;
using Serilog;
using Serilog.Events;
using Syncfusion.Data.Extensions;
using Witcher3StringEditor.Common.Abstractions;
using Witcher3StringEditor.Dialogs.Messaging;
using Witcher3StringEditor.Dialogs.ViewModels;
using Witcher3StringEditor.Locales;
using Witcher3StringEditor.Models;
using Witcher3StringEditor.Serializers.Abstractions;

namespace Witcher3StringEditor.ViewModels;

internal partial class MainWindowViewModel : ObservableObject, IRecipient<FileOpenedMessage>,
    IRecipient<ValueChangedMessage<LogEvent>>, IRecipient<ValueChangedMessage<CultureInfo>>
{
    private readonly IAppSettings appSettings;
    private readonly IBackupService backupService;
    private readonly ICheckUpdateService checkUpdateService;
    private readonly ICultureResolver cultureResolver;
    private readonly IDialogService dialogService;
    private readonly IExplorerService explorerService;
    private readonly IPlayGameService playGameService;
    private readonly IEnumerable<ITranslator> translators;
    private readonly IW3Serializer w3Serializer;

    [ObservableProperty] private string[]? dropFileData;

    [ObservableProperty] private bool isUpdateAvailable;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(OpenWorkingFolderCommand))]
    private string outputFolder = string.Empty;

    public MainWindowViewModel(IAppSettings appSettings, IBackupService backupService,
        ICheckUpdateService checkUpdateService, IDialogService dialogService, IExplorerService explorerService,
        IPlayGameService playGameService, IW3Serializer w3Serializer, IEnumerable<ITranslator> translators,
        ICultureResolver cultureResolver)
    {
        this.appSettings = appSettings;
        this.translators = translators;
        this.w3Serializer = w3Serializer;
        this.backupService = backupService;
        this.checkUpdateService = checkUpdateService;
        this.dialogService = dialogService;
        this.playGameService = playGameService;
        this.explorerService = explorerService;
        this.cultureResolver = cultureResolver;
        WeakReferenceMessenger.Default.Register<MainWindowViewModel, ValueChangedMessage<LogEvent>>(
            this, (r, m) => { r.Receive(m); });
        WeakReferenceMessenger.Default.Register<MainWindowViewModel, FileOpenedMessage, string>(
            this, "RecentFileOpened", (r, m) => { r.Receive(m); });
        WeakReferenceMessenger.Default.Register<MainWindowViewModel,
            ValueChangedMessage<CultureInfo>>(this, (r, m) => { r.Receive(m); });

        W3Items.CollectionChanged += (_, _) =>
        {
            AddCommand.NotifyCanExecuteChanged();
            ShowSaveDialogCommand.NotifyCanExecuteChanged();
            ShowTranslateDialogCommand.NotifyCanExecuteChanged();
        };
        ((INotifyPropertyChanged)appSettings).PropertyChanged += (_, e) =>
        {
            switch (e.PropertyName)
            {
                case nameof(appSettings.W3StringsPath) when !string.IsNullOrWhiteSpace(appSettings.W3StringsPath):
                    OpenFileCommand.NotifyCanExecuteChanged();
                    DropFileCommand.NotifyCanExecuteChanged();
                    break;
                case nameof(appSettings.GameExePath) when !string.IsNullOrWhiteSpace(appSettings.GameExePath):
                    PlayGameCommand.NotifyCanExecuteChanged();
                    break;
            }
        };
    }

    private ObservableCollection<LogEvent> LogEvents { get; } = [];

    public ObservableCollection<W3ItemModel> W3Items { get; set; } = [];

    private bool W3ItemsHaveItems => W3Items.Any();

    private bool CanOpenWorkingFolder
        => Directory.Exists(OutputFolder);

    private bool CanPlayGame => !string.IsNullOrWhiteSpace(appSettings.GameExePath);

    private bool CanOpenFile => !string.IsNullOrWhiteSpace(appSettings.W3StringsPath);

    private static bool IsDebug =>
        Assembly.GetExecutingAssembly().GetCustomAttribute<DebuggableAttribute>()?.IsJITTrackingEnabled == true;

    public async void Receive(FileOpenedMessage message)
    {
        try
        {
            await OpenFile(message.FileName);
        }
        catch (Exception)
        {
            //ignored
        }
    }

    public void Receive(ValueChangedMessage<CultureInfo> message)
    {
        try
        {
            I18NExtension.Culture = message.Value;
            Log.Information("Language changed to {Language}.", message.Value.Name);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to change language.");
        }
    }

    public void Receive(ValueChangedMessage<LogEvent> message)
    {
        Application.Current.Dispatcher.Invoke(() => LogEvents.Add(message.Value));
    }

    [RelayCommand]
    private async Task WindowLoaded()
    {
        Log.Information("Application started.");
        Log.Information("Application Version: {Version}", ThisAssembly.AssemblyFileVersion);
        Log.Information("OS Version: {Version}", $"{RuntimeInformation.OSDescription} ({RuntimeInformation.OSArchitecture})");
        Log.Information(".Net Runtime: {Runtime}", RuntimeInformation.FrameworkDescription);
        Log.Information("Current Directory: {Directory}", Environment.CurrentDirectory);
        Log.Information("AppData Folder: {Folder}",
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                IsDebug ? "Witcher3StringEditor_Debug" : "Witcher3StringEditor"));
        Log.Information("Current Language: {Language}", appSettings.Language);
        await CheckSettings(appSettings);
        IsUpdateAvailable = await checkUpdateService.CheckUpdate();
    }

    private static async Task CheckSettings(IAppSettings settings)
    {
        Log.Information("Checking whether the settings are correct.");
        if (string.IsNullOrWhiteSpace(settings.W3StringsPath))
        {
            Log.Error("Settings are incorrect or initial setup is incomplete.");
            _ = await WeakReferenceMessenger.Default.Send(new AsyncRequestMessage<bool>(), "FirstRun");
        }
        else
        {
            Log.Information("Settings are correct.");
            Log.Information("The W3Strings path has been set to {Path}.", settings.W3StringsPath);
            if (string.IsNullOrWhiteSpace(settings.GameExePath))
                Log.Warning("The game executable path is not set.");
            else
                Log.Information("The game executable path has been set to {Path}.", settings.GameExePath);
        }
    }

    [RelayCommand]
    private async Task WindowClosing(CancelEventArgs e)
    {
        if (W3Items.Any() &&
            await WeakReferenceMessenger.Default.Send(new AsyncRequestMessage<bool>(), "MainWindowClosing"))
            e.Cancel = true;
    }

    [RelayCommand]
    private void WindowClosed()
    {
        WeakReferenceMessenger.Default.UnregisterAll(this);
    }

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

    [RelayCommand(CanExecute = nameof(CanOpenFile))]
    private async Task OpenFile()
    {
        using var storageFile = await dialogService.ShowOpenFileDialogAsync(this, new OpenFileDialogSettings
        {
            Filters =
            [
                new FileFilter(Strings.FileFormatSupported, [".csv", ".xlsx", ".w3strings"]),
                new FileFilter(Strings.FileFormatTextFile, ".csv"),
                new FileFilter(Strings.FileFormatExcelWorkSheets, ".xlsx"),
                new FileFilter(Strings.FileFormatWitcher3StringsFile, ".w3strings")
            ]
        });
        if (storageFile != null && Path.GetExtension(storageFile.LocalPath) is ".csv" or ".w3strings" or ".xlsx")
            await OpenFile(storageFile.LocalPath);
    }

    private async Task OpenFile(string fileName)
    {
        try
        {
            if (W3Items.Any())
            {
                if (!await WeakReferenceMessenger.Default.Send(new FileOpenedMessage(fileName), "FileOpened")) return;
                W3Items.Clear();
            }

            Log.Information("The file {FileName} is being opened...", fileName);
            (await w3Serializer.Deserialize(fileName)).OrderBy(x => x.StrId)
                .ForEach(i => W3Items.Add(new W3ItemModel(i)));
            Guard.IsGreaterThan(W3Items.Count, 0);
            var folder = Path.GetDirectoryName(fileName);
            Guard.IsNotNull(folder);
            OutputFolder = folder;
            Log.Information("Working directory set to {Folder}.", folder);
            var foundItem = appSettings.RecentItems.FirstOrDefault(x => x.FilePath == fileName);
            if (foundItem == null)
            {
                appSettings.RecentItems.Add(new RecentItem(fileName, DateTime.Now));
                Log.Information("Added {FileName} to recent items.", fileName);
            }
            else
            {
                foundItem.OpenedTime = DateTime.Now;
                Log.Information("The last opened time for file {FileName} has been updated.", fileName);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to open file: {FileName}.", fileName);
        }
    }

    [RelayCommand(CanExecute = nameof(W3ItemsHaveItems))]
    private async Task Add()
    {
        var dialogViewModel = new EditDataDialogViewModel(new W3ItemModel());
        if (await dialogService.ShowDialogAsync(this, dialogViewModel) == true
            && dialogViewModel.W3Item != null)
        {
            W3Items.Add(dialogViewModel.W3Item.Cast<W3ItemModel>());
            Log.Information("New W3Item added.");
        }
        else
        {
            Log.Information("The W3Item has not been added.");
        }
    }

    [RelayCommand]
    private async Task Edit(IEditW3Item w3Item)
    {
        var dialogViewModel = new EditDataDialogViewModel(w3Item);
        if (await dialogService.ShowDialogAsync(this, dialogViewModel) == true && dialogViewModel.W3Item != null)
        {
            var found = W3Items.First(x => x.Id == w3Item.Id);
            W3Items[W3Items.IndexOf(found)] = dialogViewModel.W3Item.Cast<W3ItemModel>();
            Log.Information("The W3Item has been updated.");
        }
        else
        {
            Log.Information("The W3Item has not been updated.");
        }
    }

    [RelayCommand]
    private async Task Delete(IEnumerable<object> items)
    {
        var w3Items = items.Cast<IEditW3Item>().ToArray();
        if (w3Items.Length > 0 &&
            await dialogService.ShowDialogAsync(this, new DeleteDataDialogViewModel(w3Items)) == true)
            w3Items.ForEach(item => W3Items.Remove(item.Cast<W3ItemModel>()));
    }

    [RelayCommand]
    private async Task ShowBackupDialog()
    {
        _ = await dialogService.ShowDialogAsync(this,
            new BackupDialogViewModel(appSettings, backupService));
    }

    [RelayCommand(CanExecute = nameof(W3ItemsHaveItems))]
    private async Task ShowSaveDialog()
    {
        _ = await dialogService.ShowDialogAsync(this,
            new SaveDialogViewModel(appSettings, w3Serializer, W3Items, OutputFolder));
    }

    [RelayCommand]
    private async Task ShowLogDialog()
    {
        _ = await dialogService.ShowDialogAsync<LogDialogViewModel>(this, new LogDialogViewModel(LogEvents));
    }

    [RelayCommand]
    private async Task ShowSettingsDialog()
    {
        _ = await dialogService.ShowDialogAsync(this,
            new SettingDialogViewModel(appSettings, dialogService, translators, cultureResolver));
    }

    [RelayCommand(CanExecute = nameof(CanPlayGame))]
    private async Task PlayGame()
    {
        await playGameService.PlayGame();
    }

    [RelayCommand]
    private async Task ShowAbout()
    {
        _ = await dialogService.ShowDialogAsync(this, new AboutDialogViewModel(new Dictionary<string, object?>
        {
            { "Version", ThisAssembly.AssemblyInformationalVersion },
            { "BuildTime", BuildTimestamp.BuildTime.ToLocalTime() },
            { "OS", $"{RuntimeInformation.OSDescription} ({RuntimeInformation.OSArchitecture})" },
            { "Runtime", RuntimeInformation.FrameworkDescription },
            { "Package", DependencyContext.Default?.RuntimeLibraries.Where(static x => x.Type == "package") }
        }));
    }

    [RelayCommand(CanExecute = nameof(CanOpenWorkingFolder))]
    private void OpenWorkingFolder()
    {
        explorerService.Open(OutputFolder);
        Log.Information("Working folder opened.");
    }

    [RelayCommand]
    private void OpenNexusMods()
    {
        explorerService.Open(appSettings.NexusModUrl);
        Log.Information("NexusMods opened.");
    }

    [RelayCommand]
    private async Task ShowRecentDialog()
    {
        _ = await dialogService.ShowDialogAsync(this,
            new RecentDialogViewModel(appSettings));
    }

    [RelayCommand(CanExecute = nameof(W3ItemsHaveItems))]
    private async Task ShowTranslateDialog(IW3Item? w3Item)
    {
        _ = await dialogService.ShowDialogAsync(this,
            new TranslateDialogViewModel(appSettings,
                translators.First(x => x.Name == appSettings.Translator),
                W3Items, w3Item != null ? W3Items.IndexOf(w3Item) : 0));
    }
}