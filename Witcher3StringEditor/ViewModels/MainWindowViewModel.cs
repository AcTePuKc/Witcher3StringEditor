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
    IRecipient<ValueChangedMessage<LogEvent>>
{
    private readonly IAppSettings _appSettings;
    private readonly IBackupService _backupService;
    private readonly ICheckUpdateService _checkUpdateService;
    private readonly ICultureResolver _cultureResolver;
    private readonly IDialogService _dialogService;
    private readonly IExplorerService _explorerService;
    private readonly IPlayGameService _playGameService;
    private readonly IEnumerable<ITranslator> _translators;
    private readonly IW3Serializer _w3Serializer;

    [ObservableProperty] private string[]? _dropFileData;

    [ObservableProperty] private bool _isUpdateAvailable;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(OpenWorkingFolderCommand))]
    private string _outputFolder = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddCommand))]
    [NotifyCanExecuteChangedFor(nameof(EditCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
    [NotifyCanExecuteChangedFor(nameof(ShowSaveDialogCommand))]
    [NotifyCanExecuteChangedFor(nameof(ShowTranslateDialogCommand))]
    private ObservableCollection<W3StringItemModel>? _w3StringItems;

    public MainWindowViewModel(IAppSettings appSettings, IBackupService backupService,
        ICheckUpdateService checkUpdateService, IDialogService dialogService, IExplorerService explorerService,
        IPlayGameService playGameService, IW3Serializer w3Serializer, IEnumerable<ITranslator> translators,
        ICultureResolver cultureResolver)
    {
        _appSettings = appSettings;
        _translators = translators;
        _w3Serializer = w3Serializer;
        _backupService = backupService;
        _checkUpdateService = checkUpdateService;
        _dialogService = dialogService;
        _playGameService = playGameService;
        _explorerService = explorerService;
        _cultureResolver = cultureResolver;
        RegisterMessengerHandlers();
        SetupAppSettingsEventHandlers();
    }

    private ObservableCollection<LogEvent> LogEvents { get; } = [];

    private bool HasW3StringItems => W3StringItems?.Any() == true;

    private bool CanOpenWorkingFolder
        => Directory.Exists(OutputFolder);

    private bool CanPlayGame => File.Exists(_appSettings.GameExePath);

    private bool CanOpenFile => File.Exists(_appSettings.W3StringsPath);

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

    public void Receive(ValueChangedMessage<LogEvent> message)
    {
        Application.Current.Dispatcher.Invoke(() => LogEvents.Add(message.Value));
    }

    private void SetupAppSettingsEventHandlers()
    {
        if (_appSettings is INotifyPropertyChanged notifyPropertyChanged)
            notifyPropertyChanged.PropertyChanged += (_, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(_appSettings.W3StringsPath):
                        OpenFileCommand.NotifyCanExecuteChanged();
                        DropFileCommand.NotifyCanExecuteChanged();
                        break;
                    case nameof(_appSettings.GameExePath):
                        PlayGameCommand.NotifyCanExecuteChanged();
                        break;
                    case nameof(_appSettings.Translator):
                        ApplyTranslatorChange(_appSettings);
                        break;
                    case nameof(_appSettings.Language):
                        ApplyLanguageChange(_appSettings.Language);
                        break;
                }
            };
    }

    private void RegisterMessengerHandlers()
    {
        WeakReferenceMessenger.Default.Register<MainWindowViewModel, ValueChangedMessage<LogEvent>>(
            this, (r, m) => { r.Receive(m); });
        WeakReferenceMessenger.Default.Register<MainWindowViewModel, FileOpenedMessage, string>(
            this, "RecentFileOpened", (r, m) => { r.Receive(m); });
    }

    private static void ApplyTranslatorChange(IAppSettings appSettings)
    {
        Log.Information("Translator changed to {Translator}", appSettings.Translator);
    }

    private static void ApplyLanguageChange(string language)
    {
        try
        {
            I18NExtension.Culture = new CultureInfo(language);
            Log.Information("Language changed to {Language}.", language);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to change language.");
        }
    }

    [RelayCommand]
    private async Task WindowLoaded()
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
            string.Join(", ", _cultureResolver.SupportedCultures.Select(x => x.Name)));
        Log.Information("Current Language: {Language}", _appSettings.Language);
        await CheckSettings(_appSettings);
        IsUpdateAvailable = await _checkUpdateService.CheckUpdate();
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
            Log.Information("The W3Strings path has been set to {Path}.", settings.W3StringsPath);
            if (string.IsNullOrWhiteSpace(settings.GameExePath))
                Log.Warning("The game executable path is not set.");
            else
                Log.Information("The game executable path has been set to {Path}.", settings.GameExePath);
            Log.Information("The preferred filetype is {Filetype}", settings.PreferredW3FileType);
            Log.Information("The preferred language is {Language}", settings.PreferredLanguage);
            Log.Information("Current translator is {Translator}.", settings.Translator);
            Log.Information("Settings are correct.");
        }
    }

    [RelayCommand]
    private async Task WindowClosing(CancelEventArgs e)
    {
        if (W3StringItems?.Any() == true &&
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
        using var storageFile = await _dialogService.ShowOpenFileDialogAsync(this, new OpenFileDialogSettings
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

    private async Task OpenFile(string fileName)
    {
        try
        {
            if (W3StringItems?.Any() == true &&
                !await WeakReferenceMessenger.Default.Send(new FileOpenedMessage(fileName), "ReOpenFile")) return;
            Log.Information("The file {FileName} is being opened...", fileName);
            W3StringItems = new ObservableCollection<W3StringItemModel>(
            [
                .. (await _w3Serializer.Deserialize(fileName)).OrderBy(x => x.StrId)
                .Select(x => new W3StringItemModel(x))
            ]);
            Guard.IsGreaterThan(W3StringItems.Count, 0);
            var folder = Path.GetDirectoryName(fileName);
            Guard.IsNotNull(folder);
            OutputFolder = folder;
            Log.Information("Working directory set to {Folder}.", folder);
            var foundItem = _appSettings.RecentItems.FirstOrDefault(x => x.FilePath == fileName);
            if (foundItem == null)
            {
                _appSettings.RecentItems.Add(new RecentItem(fileName, DateTime.Now));
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

    [RelayCommand(CanExecute = nameof(HasW3StringItems))]
    private async Task Add()
    {
        var dialogViewModel = new EditDataDialogViewModel(new W3StringItemModel());
        if (await _dialogService.ShowDialogAsync(this, dialogViewModel) == true
            && dialogViewModel.W3Item != null)
        {
            W3StringItems!.Add(dialogViewModel.W3Item.Cast<W3StringItemModel>());
            Log.Information("New W3Item added.");
        }
        else
        {
            Log.Information("The W3Item has not been added.");
        }
    }

    [RelayCommand(CanExecute = nameof(HasW3StringItems))]
    private async Task Edit(ITrackableW3StringItem w3StringItem)
    {
        var dialogViewModel = new EditDataDialogViewModel(w3StringItem);
        if (await _dialogService.ShowDialogAsync(this, dialogViewModel) == true && dialogViewModel.W3Item != null)
        {
            var found = W3StringItems!.First(x => x.TrackingId == w3StringItem.TrackingId);
            W3StringItems![W3StringItems.IndexOf(found)] = dialogViewModel.W3Item.Cast<W3StringItemModel>();
            Log.Information("The W3Item has been updated.");
        }
        else
        {
            Log.Information("The W3Item has not been updated.");
        }
    }

    [RelayCommand(CanExecute = nameof(HasW3StringItems))]
    private async Task Delete(IEnumerable<object> items)
    {
        var w3Items = items.Cast<ITrackableW3StringItem>().ToArray();
        if (w3Items.Length > 0 &&
            await _dialogService.ShowDialogAsync(this, new DeleteDataDialogViewModel(w3Items)) == true)
            w3Items.ForEach(item => W3StringItems!.Remove(item.Cast<W3StringItemModel>()));
    }

    [RelayCommand]
    private async Task ShowBackupDialog()
    {
        _ = await _dialogService.ShowDialogAsync(this,
            new BackupDialogViewModel(_appSettings, _backupService));
    }

    [RelayCommand(CanExecute = nameof(HasW3StringItems))]
    private async Task ShowSaveDialog()
    {
        _ = await _dialogService.ShowDialogAsync(this,
            new SaveDialogViewModel(_appSettings, _w3Serializer, W3StringItems!, OutputFolder));
    }

    [RelayCommand]
    private async Task ShowLogDialog()
    {
        _ = await _dialogService.ShowDialogAsync<LogDialogViewModel>(this, new LogDialogViewModel(LogEvents));
    }

    [RelayCommand]
    private async Task ShowSettingsDialog()
    {
        _ = await _dialogService.ShowDialogAsync(this,
            new SettingDialogViewModel(_appSettings, _dialogService, _translators, _cultureResolver));
    }

    [RelayCommand(CanExecute = nameof(CanPlayGame))]
    private async Task PlayGame()
    {
        await _playGameService.PlayGame();
    }

    [RelayCommand]
    private async Task ShowAbout()
    {
        _ = await _dialogService.ShowDialogAsync(this, new AboutDialogViewModel(new Dictionary<string, object?>
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
        _explorerService.Open(OutputFolder);
        Log.Information("Working folder opened.");
    }

    [RelayCommand]
    private void OpenNexusMods()
    {
        _explorerService.Open(_appSettings.NexusModUrl);
        Log.Information("NexusMods opened.");
    }

    [RelayCommand]
    private async Task ShowRecentDialog()
    {
        _ = await _dialogService.ShowDialogAsync(this,
            new RecentDialogViewModel(_appSettings));
    }

    [RelayCommand(CanExecute = nameof(HasW3StringItems))]
    private async Task ShowTranslateDialog(IW3StringItem? w3Item)
    {
        _ = await _dialogService.ShowDialogAsync(this,
            new TranslateDialogViewModel(_appSettings,
                _translators.First(x => x.Name == _appSettings.Translator),
                W3StringItems!, w3Item != null ? W3StringItems.IndexOf(w3Item) : 0));
    }
}