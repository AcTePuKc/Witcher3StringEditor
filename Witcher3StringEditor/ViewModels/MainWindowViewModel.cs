using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Witcher3StringEditor.Dialogs.Recipients;
using Witcher3StringEditor.Dialogs.Validators;
using Witcher3StringEditor.Dialogs.ViewModels;
using Witcher3StringEditor.Interfaces;
using Witcher3StringEditor.Locales;
using Witcher3StringEditor.Models;
using Witcher3StringEditor.Translators;

namespace Witcher3StringEditor.ViewModels;

internal partial class MainWindowViewModel : ObservableObject
{
    private readonly IAppSettings appSettings;
    private readonly IW3Serializer w3Serializer;
    private readonly IBackupService backupService;
    private readonly IDialogService dialogService;
    private readonly ICheckUpdateService checkUpdateService;
    private readonly IPlayGameService playGameService;
    private readonly IExplorerService explorerService;
    private readonly NotificationRecipient<LogEvent> logEventRecipient = new();
    private readonly AsyncRequestRecipient<bool> recentFileOpenedRecipient = new();

    [ObservableProperty]
    private bool isUpdateAvailable;

    [ObservableProperty]
    private string[] dropFileData = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(OpenWorkingFolderCommand))]
    private string outputFolder = string.Empty;

    private ObservableCollection<LogEvent> LogEvents { get; } = [];

    public ObservableCollection<IW3Item> W3Items { get; set; } = [];

    public MainWindowViewModel(IAppSettings appSettings,
                               IBackupService backupService,
                               IW3Serializer w3Serializer,
                               IDialogService dialogService,
                               ICheckUpdateService checkUpdateService,
                               IPlayGameService playGameService,
                               IExplorerService explorerService)
    {
        this.appSettings = appSettings;
        this.w3Serializer = w3Serializer;
        this.backupService = backupService;
        this.dialogService = dialogService;
        this.checkUpdateService = checkUpdateService;
        this.playGameService = playGameService;
        this.explorerService = explorerService;
        WeakReferenceMessenger.Default.Register<NotificationRecipient<LogEvent>, NotificationMessage<LogEvent>>(logEventRecipient, (r, m) =>
        {
            r.Receive(m);
            LogEvents.Add(m.Message);
        });
        WeakReferenceMessenger.Default.Register<AsyncRequestRecipient<bool>, FileOpenedMessage, string>(recentFileOpenedRecipient, "RecentFileOpened", async void (r, m) =>
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
            ShowBatchTranslateDialogCommand.NotifyCanExecuteChanged();
        };
    }

    [RelayCommand]
    private async Task WindowLoaded()
    {
        await CheckSettings(appSettings);
        IsUpdateAvailable = await checkUpdateService.CheckUpdate();
    }

    private async Task CheckSettings(IAppSettings settings)
    {
        if (!(await AppSettingsValidator.Instance.ValidateAsync(settings)).IsValid)
            await dialogService.ShowDialogAsync(this, new SettingDialogViewModel(appSettings, dialogService));
    }

    [RelayCommand]
    private async Task WindowClosing(CancelEventArgs e)
    {
        if (W3Items.Any() && await WeakReferenceMessenger.Default.Send(new AsyncRequestMessage<bool>(), "MainWindowClosing"))
            e.Cancel = true;
    }

    [RelayCommand]
    private void WindowClosed()
        => WeakReferenceMessenger.Default.UnregisterAll(recentFileOpenedRecipient);

    [RelayCommand]
    private async Task DropFile()
    {
        if (DropFileData.Length > 0)
        {
            var file = DropFileData[0];
            var ext = Path.GetExtension(file);
            if (ext is ".csv" or ".w3strings")
                await OpenFile(file);
        }
    }

    [RelayCommand]
    private async Task OpenFile()
    {
        using var storageFile = await dialogService.ShowOpenFileDialogAsync(this, new OpenFileDialogSettings
        {
            Filters =
            [
                new FileFilter(Strings.FileFormatSupported, [".csv", ".w3strings"]),
                new FileFilter(Strings.FileFormatTextFile, ".csv"),
                new FileFilter(Strings.FileFormatWitcher3StringsFile, ".w3strings")
            ]
        });
        if (storageFile != null && Path.GetExtension(storageFile.LocalPath) is ".csv" or ".w3strings")
            await OpenFile(storageFile.LocalPath);
    }

    private async Task OpenFile(string fileName)
    {
        try
        {
            if (W3Items.Any())
                if (await WeakReferenceMessenger.Default.Send(new FileOpenedMessage(fileName), "FileOpened"))
                    W3Items.Clear();
                else
                    return;
            (await w3Serializer.Deserialize(fileName)).OrderBy(x => x.StrId).ForEach(W3Items.Add);
            OutputFolder = Path.GetDirectoryName(fileName) ?? string.Empty;
            var foundItem = appSettings.RecentItems
                .FirstOrDefault(x => x.FilePath == fileName);
            if (foundItem == null)
                appSettings.RecentItems.Add(new RecentItem(fileName, DateTime.Now));
            else
                foundItem.OpenedTime = DateTime.Now;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to open file: {0}", fileName);
        }
    }

    [RelayCommand(CanExecute = nameof(W3ItemsHaveItems))]
    private async Task Add()
    {
        var dialogViewModel = new EditDataDialogViewModel(new W3Item());
        if (await dialogService.ShowDialogAsync(this, dialogViewModel) == true
            && dialogViewModel.W3Item != null)
            W3Items.Add(dialogViewModel.W3Item);
    }

    [RelayCommand]
    private async Task Edit(IW3Item w3Item)
    {
        var dialogViewModel = new EditDataDialogViewModel(w3Item);
        if (await dialogService.ShowDialogAsync(this, dialogViewModel) == true
            && dialogViewModel.W3Item != null)
        {
            var found = W3Items.First(x => x.Id == w3Item.Id);
            W3Items[W3Items.IndexOf(found)] = dialogViewModel.W3Item;
        }
    }

    [RelayCommand]
    private async Task Delete(IEnumerable<object> items)
    {
        var w3Items = items.Cast<IW3Item>().ToArray();
        if (w3Items.Length > 0
            && await dialogService.ShowDialogAsync(this, new DeleteDataDialogViewModel(w3Items)) == true)
        {
            w3Items.ForEach(item => W3Items.Remove(item));
        }
    }

    [RelayCommand]
    private async Task ShowBackupDialog()
        => await dialogService.ShowDialogAsync(this, new BackupDialogViewModel(appSettings, backupService));

    private bool W3ItemsHaveItems => W3Items.Any();

    [RelayCommand(CanExecute = nameof(W3ItemsHaveItems))]
    private async Task ShowSaveDialog()
    {
        await dialogService.ShowDialogAsync(this, new SaveDialogViewModel(w3Serializer, new W3Job
        {
            Path = OutputFolder,
            W3Items = W3Items,
            W3FileType = appSettings.PreferredW3FileType,
            Language = appSettings.PreferredLanguage
        }));
    }

    [RelayCommand]
    private async Task ShowLogDialog()
        => await dialogService.ShowDialogAsync<LogDialogViewModel>(this, new LogDialogViewModel(LogEvents));

    [RelayCommand]
    private async Task ShowSettingsDialog()
        => await dialogService.ShowDialogAsync(this, new SettingDialogViewModel(appSettings, dialogService));

    [RelayCommand]
    private async Task PlayGame()
        => await playGameService.PlayGame();

    [RelayCommand]
    private async Task ShowAbout()
    {
        await dialogService.ShowDialogAsync(this, new AboutDialogViewModel(new Dictionary<string, object?>
        {
            { "Version", ThisAssembly.AssemblyInformationalVersion.Trim() },
            { "BuildTime", RetrieveTimestampAsDateTime() },
            { "OS", $"{RuntimeInformation.OSDescription} ({RuntimeInformation.OSArchitecture})" },
            { "Runtime", RuntimeInformation.FrameworkDescription },
            { "Package",DependencyContext.Default?.RuntimeLibraries.Where(static x => x.Type == "package")}
        }));
    }

    private static DateTime RetrieveTimestampAsDateTime()
    {
        try
        {
            var timestamp = Assembly.GetExecutingAssembly().GetCustomAttributesData()
                .FirstOrDefault(static x => x.AttributeType.Name == "TimestampAttribute")?.ConstructorArguments
                .FirstOrDefault().Value as string ?? string.Empty;
            return !string.IsNullOrWhiteSpace(timestamp)
                ? DateTime.ParseExact(timestamp, "yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal).ToLocalTime()
                : DateTime.MinValue;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve the build time of the application.");
            return DateTime.MinValue;
        }
    }

    [RelayCommand(CanExecute = nameof(CanOpenWorkingFolder))]
    private void OpenWorkingFolder()
        => explorerService.Open(OutputFolder);

    private bool CanOpenWorkingFolder
        => Directory.Exists(OutputFolder);

    [RelayCommand]
    private void OpenNexusMods()
        => explorerService.Open(appSettings.NexusModUrl);

    [RelayCommand]
    private async Task ShowRecentDialog()
        => await dialogService.ShowDialogAsync(this, new RecentDialogViewModel(appSettings));

    [RelayCommand]
    private async Task ShowTranslateDialog(object item)
    {
        if (item is not W3Item w3Item) return;
        await dialogService.ShowDialogAsync(this, new TranslateDiaglogViewModel(W3Items, W3Items.IndexOf(w3Item), appSettings, appSettings.IsUseAiTranslate
            ? new AiTranslator(appSettings.ModelSettings) : Ioc.Default.GetRequiredService<MicrosoftTranslator>()));
    }

    [RelayCommand(CanExecute = nameof(W3ItemsHaveItems))]
    private async Task ShowBatchTranslateDialog(object? item = null)
    {
        var startIndex = item != null ? W3Items.IndexOf(item) + 1 : 1;
        await dialogService.ShowDialogAsync(this, new BatchTranslateDialogViewModel(W3Items, startIndex, appSettings, appSettings.IsUseAiTranslate
            ? new AiTranslator(appSettings.ModelSettings) : Ioc.Default.GetRequiredService<MicrosoftTranslator>()));
    }
}