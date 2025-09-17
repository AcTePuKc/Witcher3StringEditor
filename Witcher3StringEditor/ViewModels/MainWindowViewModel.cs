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
using CommunityToolkit.Mvvm.DependencyInjection;
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
using Witcher3StringEditor.Dialogs.Messaging;
using Witcher3StringEditor.Dialogs.ViewModels;
using Witcher3StringEditor.Locales;
using Witcher3StringEditor.Models;
using Witcher3StringEditor.Serializers.Abstractions;
using W3StringItemModel = Witcher3StringEditor.Models.W3StringItemModel;

namespace Witcher3StringEditor.ViewModels;

internal partial class MainWindowViewModel : ObservableObject
{
    private readonly IAppSettings appSettings;
    private readonly IBackupService backupService;
    private readonly IDialogService dialogService;

    [ObservableProperty] private string[]? dropFileData;

    [ObservableProperty] private bool isUpdateAvailable;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(OpenWorkingFolderCommand))]
    private string outputFolder = string.Empty;

    private IEnumerable<W3StringItemModel>? searchResults;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddCommand))]
    [NotifyCanExecuteChangedFor(nameof(EditCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
    [NotifyCanExecuteChangedFor(nameof(ShowSaveDialogCommand))]
    [NotifyCanExecuteChangedFor(nameof(ShowTranslateDialogCommand))]
    private ObservableCollection<W3StringItemModel>? w3StringItems;

    public MainWindowViewModel(IAppSettings appSettings,
        IBackupService backupService,
        IDialogService dialogService)
    {
        this.appSettings = appSettings;
        this.backupService = backupService;
        this.dialogService = dialogService;
        RegisterMessengerHandlers();
        SetupAppSettingsEventHandlers();
    }

    private ObservableCollection<LogEvent> LogEvents { get; } = [];

    private bool HasW3StringItems => W3StringItems?.Any() == true;

    private bool CanOpenWorkingFolder
        => Directory.Exists(OutputFolder);

    private bool CanPlayGame => File.Exists(appSettings.GameExePath);

    private bool CanOpenFile => File.Exists(appSettings.W3StringsPath);

    private static bool IsDebug =>
        Assembly.GetExecutingAssembly().GetCustomAttribute<DebuggableAttribute>()?.IsJITTrackingEnabled == true;


    private void SetupAppSettingsEventHandlers()
    {
        if (appSettings is INotifyPropertyChanged notifyPropertyChanged)
            notifyPropertyChanged.PropertyChanged += (_, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(appSettings.W3StringsPath):
                        OpenFileCommand.NotifyCanExecuteChanged();
                        DropFileCommand.NotifyCanExecuteChanged();
                        break;
                    case nameof(appSettings.GameExePath):
                        PlayGameCommand.NotifyCanExecuteChanged();
                        break;
                    case nameof(appSettings.Translator):
                        ApplyTranslatorChange(appSettings);
                        break;
                    case nameof(appSettings.Language):
                        ApplyLanguageChange(appSettings.Language);
                        break;
                }
            };
    }

    private void RegisterMessengerHandlers()
    {
        WeakReferenceMessenger.Default.Register<MainWindowViewModel, ValueChangedMessage<LogEvent>>(
            // ReSharper disable once AsyncVoidMethod
            this,
            async void (_, m) => { await Application.Current.Dispatcher.InvokeAsync(() => LogEvents.Add(m.Value)); });
        WeakReferenceMessenger.Default.Register<MainWindowViewModel, AsyncRequestMessage<string, bool>, string>(
            // ReSharper disable once AsyncVoidMethod
            this,
            "RecentFileOpened",
            async void (_, m) => { await OpenFile(m.Request); });
        WeakReferenceMessenger.Default
            .Register<MainWindowViewModel, ValueChangedMessage<IEnumerable<W3StringItemModel>?>,
                string>(this, "SearchResultsUpdated", (_, m) => { searchResults = m.Value; });
        WeakReferenceMessenger.Default
            .Register<MainWindowViewModel, ValueChangedMessage<ITrackableW3StringItem>, string>(
                this,
                "TranslationSaved",
                (_, m) =>
                {
                    var item = m.Value;
                    var found = W3StringItems!
                        .First(x => x.TrackingId == item.TrackingId);
                    found.Text = item.Text;
                });
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
            string.Join(", ",
                Ioc.Default.GetRequiredService<ICultureResolver>().SupportedCultures.Select(x => x.Name)));
        Log.Information("Current Language: {Language}", appSettings.Language);
        await CheckSettings(appSettings);
        IsUpdateAvailable = await Ioc.Default.GetRequiredService<ICheckUpdateService>().CheckUpdate();
    }

    private static async Task CheckSettings(IAppSettings settings)
    {
        Log.Information("Checking whether the settings are correct.");
        if (string.IsNullOrWhiteSpace(settings.W3StringsPath))
        {
            Log.Error("Settings are incorrect or initial setup is incomplete.");
            await WeakReferenceMessenger.Default.Send(new AsyncRequestMessage<bool>(), "FirstRun");
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

    private async Task OpenFile(string fileName)
    {
        try
        {
            if (!await HandleReOpenFile(fileName)) return;
            Log.Information("The file {FileName} is being opened...", fileName);
            await LoadW3StringItems(fileName);
            SetOutputFolder(fileName);
            UpdateRecentItems(fileName);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to open file: {FileName}.", fileName);
        }
    }

    private async Task<bool> HandleReOpenFile(string fileName)
    {
        if (W3StringItems?.Any() != true) return true;
        if (!await WeakReferenceMessenger.Default.Send(new AsyncRequestMessage<string, bool>(fileName),
                "ReOpenFile")) return false;
        WeakReferenceMessenger.Default.Send(new ValueChangedMessage<bool>(true), "ClearSearch");
        return true;
    }

    private async Task LoadW3StringItems(string fileName)
    {
        W3StringItems = new ObservableCollection<W3StringItemModel>(
        [
            .. (await Ioc.Default.GetRequiredService<IW3Serializer>()
                .Deserialize(fileName)).OrderBy(x => x.StrId)
            .Select(x => new W3StringItemModel(x))
        ]);
        Guard.IsGreaterThan(W3StringItems.Count, 0);
    }

    private void SetOutputFolder(string fileName)
    {
        var folder = Path.GetDirectoryName(fileName);
        Guard.IsNotNull(folder);
        OutputFolder = folder;
        Log.Information("Working directory set to {Folder}.", folder);
    }

    private void UpdateRecentItems(string fileName)
    {
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

    [RelayCommand(CanExecute = nameof(HasW3StringItems))]
    private async Task Delete(IEnumerable<object> selectedItems)
    {
        var w3Items = selectedItems.Cast<ITrackableW3StringItem>().ToArray();
        if (w3Items.Length > 0 &&
            await dialogService.ShowDialogAsync(this,
                new DeleteDataDialogViewModel(w3Items)) == true)
            w3Items.ForEach(item => W3StringItems!.Remove(item.Cast<W3StringItemModel>()));
    }

    [RelayCommand]
    private async Task ShowBackupDialog()
    {
        await dialogService.ShowDialogAsync(this,
            new BackupDialogViewModel(appSettings, backupService));
    }

    [RelayCommand(CanExecute = nameof(HasW3StringItems))]
    private async Task ShowSaveDialog()
    {
        await dialogService.ShowDialogAsync(this,
            new SaveDialogViewModel(appSettings,
                Ioc.Default.GetRequiredService<IW3Serializer>(),
                W3StringItems!,
                OutputFolder));
    }

    [RelayCommand]
    private async Task ShowLogDialog()
    {
        await dialogService.ShowDialogAsync<LogDialogViewModel>(this,
            new LogDialogViewModel(LogEvents));
    }

    [RelayCommand]
    private async Task ShowSettingsDialog()
    {
        var translators = Ioc.Default.GetServices<ITranslator>().ToArray();
        var names = translators.Select(x => x.Name);
        Array.ForEach(translators, x => (x as IDisposable)?.Dispose());
        await dialogService.ShowDialogAsync(this,
            new SettingDialogViewModel(appSettings, dialogService, names,
                Ioc.Default.GetRequiredService<ICultureResolver>().SupportedCultures));
    }

    [RelayCommand(CanExecute = nameof(CanPlayGame))]
    private async Task PlayGame()
    {
        await Ioc.Default.GetRequiredService<IPlayGameService>().PlayGame();
    }

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

    [RelayCommand(CanExecute = nameof(CanOpenWorkingFolder))]
    private void OpenWorkingFolder()
    {
        Ioc.Default.GetRequiredService<IExplorerService>().Open(OutputFolder);
        Log.Information("Working folder opened.");
    }

    [RelayCommand]
    private void OpenNexusMods()
    {
        Ioc.Default.GetRequiredService<IExplorerService>()
            .Open(appSettings.NexusModUrl);
        Log.Information("NexusMods opened.");
    }

    [RelayCommand]
    private async Task ShowRecentDialog()
    {
        await dialogService.ShowDialogAsync(this,
            new RecentDialogViewModel(appSettings));
    }

    [RelayCommand(CanExecute = nameof(HasW3StringItems))]
    private async Task ShowTranslateDialog(IW3StringItem? selectedItem)
    {
        var items = searchResults ?? W3StringItems!;
        var itemsList = items.Cast<ITrackableW3StringItem>().ToList();
        var selectedIndex = selectedItem != null ? itemsList.IndexOf(selectedItem) : 0;
        var translator = Ioc.Default.GetServices<ITranslator>()
            .First(x => x.Name == appSettings.Translator);
        await dialogService.ShowDialogAsync(this,
            new TranslateDialogViewModel(appSettings, translator, itemsList, selectedIndex));
        if (translator is IDisposable disposable) disposable.Dispose();
        WeakReferenceMessenger.Default.Send(new ValueChangedMessage<bool>(true), "RefreshDataGrid");
    }
}