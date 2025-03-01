using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using FluentValidation;
using GTranslate.Translators;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using Microsoft.Extensions.DependencyModel;
using Serilog;
using Serilog.Events;
using Syncfusion.Data.Extensions;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Witcher3StringEditor.Dialogs.Recipients;
using Witcher3StringEditor.Dialogs.ViewModels;
using Witcher3StringEditor.Interfaces;
using Witcher3StringEditor.Locales;
using Witcher3StringEditor.Models;
using Witcher3StringEditor.Translators;

namespace Witcher3StringEditor.ViewModels;

internal partial class MainWindowViewModel : ObservableObject
{
    private readonly IAppSettings appSettings;
    private readonly IValidator<IAppSettings> appSettingsValidator;
    private readonly IBackupService backupService;
    private readonly ICheckUpdateService checkUpdateService;
    private readonly IDialogService dialogService;
    private readonly IExplorerService explorerService;
    private readonly NotificationRecipient<LogEvent> logEventRecipient = new();
    private readonly IValidator<IModelSettings> modelSettingsValidator;
    private readonly IPlayGameService playGameService;
    private readonly AsyncRequestRecipient<bool> recentFileOpenedRecipient = new();
    private readonly IW3Serializer w3Serializer;

    [ObservableProperty] private string[] dropFileData = [];

    [ObservableProperty] private bool isUpdateAvailable;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(OpenWorkingFolderCommand))]
    private string outputFolder = string.Empty;

    [ObservableProperty] private object? selectedItem;

    public MainWindowViewModel(IAppSettings appSettings, IBackupService backupService, IW3Serializer w3Serializer,
        IDialogService dialogService, ICheckUpdateService checkUpdateService, IPlayGameService playGameService,
        IExplorerService explorerService, IValidator<IAppSettings> appSettingsValidator,
        IValidator<IModelSettings> modelSettingsValidator)
    {
        this.appSettings = appSettings;
        this.w3Serializer = w3Serializer;
        this.backupService = backupService;
        this.dialogService = dialogService;
        this.playGameService = playGameService;
        this.explorerService = explorerService;
        this.checkUpdateService = checkUpdateService;
        this.appSettingsValidator = appSettingsValidator;
        this.modelSettingsValidator = modelSettingsValidator;
        WeakReferenceMessenger.Default.Register<NotificationRecipient<LogEvent>, NotificationMessage<LogEvent>>(
            logEventRecipient, (r, m) =>
            {
                r.Receive(m);
                LogEvents.Add(m.Message);
            });
        WeakReferenceMessenger.Default.Register<AsyncRequestRecipient<bool>, FileOpenedMessage, string>(
            recentFileOpenedRecipient, "RecentFileOpened", async void (r, m) =>
            {
                try
                {
                    r.Receive(m);
                    await OpenFile(m.FileName);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failed to open file: {0}", m.FileName);
                }
            });
        W3Items.CollectionChanged += (_, _) =>
        {
            AddCommand.NotifyCanExecuteChanged();
            ShowSaveDialogCommand.NotifyCanExecuteChanged();
            ShowTranslateDialogCommand.NotifyCanExecuteChanged();
        };
    }

    private ObservableCollection<LogEvent> LogEvents { get; } = [];

    public ObservableCollection<IW3Item> W3Items { get; set; } = [];

    private bool W3ItemsHaveItems => W3Items.Any();

    private bool CanOpenWorkingFolder
        => Directory.Exists(OutputFolder);

    [RelayCommand]
    private async Task WindowLoaded()
    {
        Log.Information("Application started.");
        Log.Information("Application version: {0}", ThisAssembly.AssemblyFileVersion);
        Log.Information("OS version: {0}", $"{RuntimeInformation.OSDescription} ({RuntimeInformation.OSArchitecture})");
        Log.Information(".Net Runtime: {0}", RuntimeInformation.FrameworkDescription);
        await CheckSettings(appSettings);
        IsUpdateAvailable = await checkUpdateService.CheckUpdate();
        Log.Information("New version detected: {0}.", IsUpdateAvailable);
    }

    private async Task CheckSettings(IAppSettings settings)
    {
        Log.Information("Checking whether the settings are correct.");
        if (!(await appSettingsValidator.ValidateAsync(settings)).IsValid)
        {
            Log.Error("Settings are incorrect or initial setup is incomplete.");
            _ = await dialogService.ShowDialogAsync(this,
                new SettingDialogViewModel(appSettings, dialogService, appSettingsValidator, modelSettingsValidator));
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
        WeakReferenceMessenger.Default.UnregisterAll(recentFileOpenedRecipient);
    }

    [RelayCommand]
    private async Task DropFile()
    {
        if (DropFileData.Length > 0)
        {
            var file = DropFileData[0];
            var ext = Path.GetExtension(file);
            if (ext is ".csv" or ".w3strings" or ".xlsx") await OpenFile(file);
        }
    }

    [RelayCommand]
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
            Log.Information("The file {0} is being opened...", fileName);
            (await w3Serializer.Deserialize(fileName)).OrderBy(x => x.StrId).ForEach(W3Items.Add);
            Guard.IsGreaterThan(W3Items.Count, 0);
            var folder = Path.GetDirectoryName(fileName);
            Guard.IsNotNull(folder);
            OutputFolder = folder;
            Log.Information("Working directory set to {0}.", folder);
            var foundItem = appSettings.RecentItems.FirstOrDefault(x => x.FilePath == fileName);
            if (foundItem == null)
            {
                appSettings.RecentItems.Add(new RecentItem(fileName, DateTime.Now));
                Log.Information("Added {0} to recent items.", fileName);
            }
            else
            {
                foundItem.OpenedTime = DateTime.Now;
                Log.Information("The last opened time for file {0} has been updated.", fileName);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to open file: {0}.", fileName);
        }
    }

    [RelayCommand(CanExecute = nameof(W3ItemsHaveItems))]
    private async Task Add()
    {
        var dialogViewModel = new EditDataDialogViewModel(new W3Item());
        if (await dialogService.ShowDialogAsync(this, dialogViewModel) == true
            && dialogViewModel.W3Item != null)
        {
            W3Items.Add(dialogViewModel.W3Item);
            Log.Information("New W3Item added.");
        }
    }

    [RelayCommand]
    private async Task Edit(IW3Item w3Item)
    {
        var dialogViewModel = new EditDataDialogViewModel(w3Item);
        if (await dialogService.ShowDialogAsync(this, dialogViewModel) == true && dialogViewModel.W3Item != null)
        {
            var found = W3Items.First(x => x.Id == w3Item.Id);
            W3Items[W3Items.IndexOf(found)] = dialogViewModel.W3Item;
            Log.Information("The W3Item has been updated.");
        }
    }

    [RelayCommand]
    private async Task Delete(IEnumerable<object> items)
    {
        var w3Items = items.Cast<IW3Item>().ToArray();
        if (w3Items.Length > 0 &&
            await dialogService.ShowDialogAsync(this, new DeleteDataDialogViewModel(w3Items)) == true)
        {
            w3Items.ForEach(item => W3Items.Remove(item));
            Log.Information("The selected W3Items have been deleted.");
        }
    }

    [RelayCommand]
    private async Task ShowBackupDialog()
    {
        _ = await dialogService.ShowDialogAsync(this, new BackupDialogViewModel(appSettings, backupService));
    }

    [RelayCommand(CanExecute = nameof(W3ItemsHaveItems))]
    private async Task ShowSaveDialog()
    {
        _ = await dialogService.ShowDialogAsync(this,
            new SaveDialogViewModel(w3Serializer,
                new W3Job
                {
                    Path = OutputFolder,
                    W3Items = W3Items,
                    W3FileType = appSettings.PreferredW3FileType,
                    Language = appSettings.PreferredLanguage
                }));
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
            new SettingDialogViewModel(appSettings, dialogService, appSettingsValidator, modelSettingsValidator));
    }

    [RelayCommand]
    private async Task PlayGame()
    {
        Log.Information("Starting the game.");
        await playGameService.PlayGame();
        Log.Information("Game has exited.");
    }

    [RelayCommand]
    private async Task ShowAbout()
    {
        _ = await dialogService.ShowDialogAsync(this, new AboutDialogViewModel(new Dictionary<string, object?>
        {
            { "Version", ThisAssembly.AssemblyInformationalVersion.Trim() },
            { "BuildTime", RetrieveTimestampAsDateTime() },
            { "OS", $"{RuntimeInformation.OSDescription} ({RuntimeInformation.OSArchitecture})" },
            { "Runtime", RuntimeInformation.FrameworkDescription },
            { "Package", DependencyContext.Default?.RuntimeLibraries.Where(static x => x.Type == "package") }
        }));
    }

    private static DateTime RetrieveTimestampAsDateTime()
    {
        try
        {
            Guard.IsTrue(DateTime.TryParseExact(Assembly.GetExecutingAssembly().GetCustomAttributesData()
                    .FirstOrDefault(static x => x.AttributeType.Name == "TimestampAttribute")?.ConstructorArguments
                    .FirstOrDefault().Value as string, "yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal, out var buildTime));
            return buildTime.ToLocalTime();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve the build time of the application.");
            return DateTime.MinValue;
        }
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
        _ = await dialogService.ShowDialogAsync(this, new RecentDialogViewModel(appSettings));
    }

    [RelayCommand(CanExecute = nameof(W3ItemsHaveItems))]
    private async Task ShowTranslateDialog()
    {
        _ = await dialogService.ShowDialogAsync(this,
            new TranslateDialogViewModel(W3Items, SelectedItem != null ? W3Items.IndexOf(SelectedItem) : 0, appSettings,
                appSettings.IsUseAiTranslate
                    ? new AiTranslator(appSettings.ModelSettings)
                    : Ioc.Default.GetRequiredService<MicrosoftTranslator>()));
    }
}