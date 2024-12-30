using AngleSharp;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using Serilog;
using Serilog.Events;
using Syncfusion.Data.Extensions;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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
using Witcher3StringEditor.Serializers;
using Witcher3StringEditor.Services;

namespace Witcher3StringEditor.ViewModels;

internal partial class MainWindowViewModel : ObservableObject
{
    private W3Serializer? serializer;
    private readonly IAppSettings appSettings;
    private readonly IDialogService dialogService;
    private readonly IBackupService backupService;
    private readonly LogEventRecipient logEventRecipient = new();
    private readonly FileOpenedRecipient recentFileOpenedRecipient = new();

    [ObservableProperty]
    private bool isUpdateAvailable;

    [ObservableProperty]
    private string[] dropFileData = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(OpenWorkingFolderCommand))]
    private string outputFolder = string.Empty;

    private ObservableCollection<LogEvent> LogEvents { get; } = [];

    public ObservableCollection<IW3Item> W3Items { get; set; } = [];

    public MainWindowViewModel(IAppSettings appSettings, IDialogService dialogService)
    {
        this.appSettings = appSettings;
        this.dialogService = dialogService;
        backupService = new BackupService(appSettings);
        WeakReferenceMessenger.Default.Register<LogEventRecipient, LogEvent>(logEventRecipient, (r, m) =>
        {
            r.Receive(m);
            LogEvents.Add(m);
        });
        WeakReferenceMessenger.Default.Register<FileOpenedRecipient, FileOpenedMessage, string>(recentFileOpenedRecipient, "RecentFileOpened", async void (r, m) =>
        {
            try
            {
                r.Receive(m);
                await OpenFile(m.FileName);
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
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
        serializer = new W3Serializer(appSettings, backupService);
        IsUpdateAvailable = await CheckUpdate();
    }

    [RelayCommand]
    private async Task WindowClosing(CancelEventArgs e)
    {
        if (W3Items.Any() && await WeakReferenceMessenger.Default.Send(new WindowClosingMessage(), "MainWindowClosing"))
            e.Cancel = true;
    }

    [RelayCommand]
    private void WindowClosed()
    => WeakReferenceMessenger.Default.UnregisterAll(recentFileOpenedRecipient);

    private async Task CheckSettings(IAppSettings settings)
    {
        if (!(await AppSettingsValidator.Instance.ValidateAsync(settings)).IsValid)
        {
            var dialogViewModel = new SettingDialogViewModel(appSettings, dialogService);
            await dialogService.ShowDialogAsync(this, dialogViewModel);
        }
    }

    [RelayCommand]
    private async Task DropFile()
    {
        if (DropFileData.Length != 0)
        {
            var file = DropFileData[0];
            var ext = Path.GetExtension(file);
            if (ext is ".csv" or ".w3strings")
            {
                if (serializer == null) return;
                if (W3Items.Any() && await WeakReferenceMessenger.Default.Send(new FileOpenedMessage(file), "FileOpened"))
                {
                    W3Items.Clear();
                    (await serializer.Deserialize(file)).ForEach(W3Items.Add);
                }
            }
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
                new FileFilter(Strings.FileFormatTextFile, ".csv"), new FileFilter(Strings.FileFormatWitcher3StringsFile, ".w3strings")
            ]
        });
        if (storageFile != null)
            await OpenFile(storageFile.LocalPath);
    }

    private async Task OpenFile(string fileName)
    {
        if (serializer == null) return;
        if (W3Items.Any() && await WeakReferenceMessenger.Default.Send(new FileOpenedMessage(fileName), "FileOpened")) W3Items.Clear();
        (await serializer.Deserialize(fileName)).ForEach(W3Items.Add);
        OutputFolder = Path.GetDirectoryName(fileName) ?? string.Empty;
        if (appSettings.RecentItems.Count != 0)
        {
            try
            {
                var foundItem = appSettings.RecentItems.First(x => x.FilePath == fileName);
                foundItem.OpenedTime = DateTime.Now;
            }
            catch (Exception)
            {
                appSettings.RecentItems.Add(new RecentItem(fileName, DateTime.Now));
            }
        }
        else
        {
            appSettings.RecentItems.Add(new RecentItem(fileName, DateTime.Now));
        }
    }

    [RelayCommand(CanExecute = nameof(CanShowDialog))]
    private async Task Add()
    {
        var dialogViewModel = new EditDataDialogViewModel(new W3Item());
        var result = await dialogService.ShowDialogAsync(this, dialogViewModel);
        if (result == true && dialogViewModel.W3Item != null)
            W3Items.Add(dialogViewModel.W3Item);
    }

    [RelayCommand]
    private async Task Edit(IW3Item w3Item)
    {
        var dialogViewModel = new EditDataDialogViewModel(w3Item);
        var result = await dialogService.ShowDialogAsync(this, dialogViewModel);
        if (result == true && dialogViewModel.W3Item != null)
        {
            var found = W3Items.First(x => x.Id == w3Item.Id);
            found = dialogViewModel.W3Item;
        }
    }

    [RelayCommand]
    private async Task Delete(IEnumerable<object> items)
    {
        var w3Items = items.Cast<IW3Item>().ToArray();
        if (w3Items.Length != 0)
        {
            if (await dialogService.ShowDialogAsync(this, new DeleteDataDialogViewModel(w3Items)) == true)
                w3Items.ForEach(item => W3Items.Remove(item));
        }
    }

    [RelayCommand]
    private async Task ShowBackupDialog()
        => await dialogService.ShowDialogAsync(this, new BackupDialogViewModel(backupService, appSettings));

    private bool CanShowDialog() => W3Items.Any();

    [RelayCommand(CanExecute = nameof(CanShowDialog))]
    private async Task ShowSaveDialog()
    {
        if (serializer == null) return;
        await dialogService.ShowDialogAsync(this, new SaveDialogViewModel(new W3Job
        {
            Path = OutputFolder,
            W3Items = [.. W3Items],
            FileType = appSettings.PreferredFileType,
            Language = appSettings.PreferredLanguage
        }, serializer));
    }

    [RelayCommand]
    private async Task ShowLogDialog()
        => await dialogService.ShowDialogAsync<LogDialogViewModel>(this, new LogDialogViewModel(LogEvents));

    [RelayCommand]
    private async Task ShowSettingsDialog()
        => await dialogService.ShowDialogAsync(this, new SettingDialogViewModel(appSettings, dialogService));

    [RelayCommand]
    private async Task PlayGame()
    {
        using var process = new Process();
        process.EnableRaisingEvents = true;
        process.StartInfo = new ProcessStartInfo
        {
            FileName = appSettings.GameExePath,
            WorkingDirectory = Path.GetDirectoryName(appSettings.GameExePath),
            RedirectStandardError = true,
            RedirectStandardOutput = true
        };
        process.ErrorDataReceived += Process_ErrorDataReceived;
        process.OutputDataReceived += Process_OutputDataReceived;
        process.Start();
        process.BeginErrorReadLine();
        process.BeginOutputReadLine();
        await process.WaitForExitAsync();
    }

    private static void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (e.Data != null) Log.Error(e.Data);
    }

    private static void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (e.Data != null) Log.Information(e.Data);
    }

    [RelayCommand]
    private static void ShowAbout()
    {
        var buildTime = RetrieveTimestampAsDateTime();
        var runtime = RuntimeInformation.FrameworkDescription;
        var version = ThisAssembly.AssemblyInformationalVersion.Trim();
        var os = $"{RuntimeInformation.OSDescription}({RuntimeInformation.OSArchitecture})";
        var message = $"{Strings.Version}: {version}\n{Strings.AppBuildTime}: {buildTime}\n{Strings.OS}: {os}\n{Strings.Runtime}: {runtime}";
        WeakReferenceMessenger.Default.Send(new SimpleStringMessage(message), "AboutInformation");
    }

    private static string RetrieveTimestamp()
    {
        var attribute = Assembly.GetExecutingAssembly()
            .GetCustomAttributesData()
            .First(static x => x.AttributeType.Name == "TimestampAttribute");
        return attribute.ConstructorArguments.First().Value as string ?? string.Empty;
    }

    private static DateTime RetrieveTimestampAsDateTime()
    {
        var timestamp = RetrieveTimestamp();
        return timestamp == string.Empty
            ? new DateTime()
            : DateTime.ParseExact(timestamp, "yyyy-MM-ddTHH:mm:ss.fffZ", null, DateTimeStyles.AssumeUniversal)
                .ToLocalTime();
    }

    [RelayCommand(CanExecute = nameof(CanOpenWorkingFolder))]
    private void OpenWorkingFolder()
        => Process.Start("explorer.exe", OutputFolder);

    private bool CanOpenWorkingFolder()
        => Directory.Exists(OutputFolder);

    [RelayCommand]
    private static void OpenNexusMods()
        => Process.Start("explorer.exe", "https://www.nexusmods.com/witcher3/mods/10032");

    private static async Task<bool> CheckUpdate()
    {
        var context = BrowsingContext.New(Configuration.Default.WithDefaultLoader());
        var document = await context.OpenAsync("https://www.nexusmods.com/witcher3/mods/10032");
        var element = document.QuerySelector("#pagetitle>ul.stats.clearfix>li.stat-version>div>div.stat");
        return element != null && new Version(element.InnerHtml) > new Version(ThisAssembly.AssemblyFileVersion);
    }

    [RelayCommand]
    private async Task ShowRecentDialog()
        => await dialogService.ShowDialogAsync(this, new RecentDialogViewModel(appSettings));

    [RelayCommand]
    private async Task ShowTranslateDialog(object item)
    {
        if (item is not W3Item w3Item) return;
        await dialogService.ShowDialogAsync(this, new TranslateDiaglogViewModel(W3Items, W3Items.IndexOf(w3Item), appSettings));
    }

    [RelayCommand(CanExecute = nameof(CanShowDialog))]
    private async Task ShowBatchTranslateDialog()
        => await dialogService.ShowDialogAsync(this, new BatchTranslateDialogViewModel(W3Items, appSettings));
}