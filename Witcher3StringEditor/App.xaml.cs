using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Wpf;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Resourcer;
using Serilog;
using Serilog.Events;
using Syncfusion.Licensing;
using System.IO;
using System.Reactive;
using System.Windows;
using Witcher3StringEditor.Dialogs.ViewModels;
using Witcher3StringEditor.Dialogs.Views;
using Witcher3StringEditor.Interfaces;
using Witcher3StringEditor.Models;
using Witcher3StringEditor.Serializers;
using Witcher3StringEditor.Services;
using Witcher3StringEditor.ViewModels;
using WPFLocalizeExtension.Engine;

namespace Witcher3StringEditor;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App
{
    private IAppSettings? appSettings;
    private ObserverBase<LogEvent>? logObserver;

    private readonly string configPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "Witcher3StringEditor",
        "AppSettings.Json");

    protected override void OnStartup(StartupEventArgs e)
    {
        appSettings = File.Exists(configPath)
            ? JsonConvert.DeserializeObject<AppSettings>(File.ReadAllText(configPath)) ?? new AppSettings()
            : new AppSettings();
        var logObserver = new AnonymousObserver<LogEvent>(x => WeakReferenceMessenger.Default.Send(x));
        Log.Logger = new LoggerConfiguration().WriteTo.File(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
            , "Witcher3StringEditor", "Logs", "log.txt"), rollingInterval: RollingInterval.Day)
            .WriteTo.Debug().WriteTo.Observers(observable => observable.Subscribe(logObserver)).Enrich.FromLogContext()
            .CreateLogger();
        SyncfusionLicenseProvider.RegisterLicense(Resource.AsString("License.txt"));
        Ioc.Default.ConfigureServices(InitializeServices(appSettings));
        LocalizeDictionary.Instance.Culture = Thread.CurrentThread.CurrentCulture;
    }

    private static ServiceProvider InitializeServices(IAppSettings appSettings)
    {
        IBackupService backupService = new BackupService(appSettings);
        IW3Serializer w3Serializer = new W3Serializer(appSettings, backupService);
        IDialogManager dialogManager = new DialogManager(CreatStrongViewLocator());
        IDialogService dialogService = new DialogService(dialogManager, Ioc.Default.GetService);
        ICheckUpdateService checkUpdateService = new CheckUpdateService();
        IPlayGameService playGameService = new PlayGameService(appSettings);
        return new ServiceCollection()
                        .AddLogging(builder => builder.AddSerilog())
                        .AddSingleton(appSettings)
                        .AddSingleton(backupService)
                        .AddSingleton(w3Serializer)
                        .AddSingleton(dialogService)
                        .AddSingleton(checkUpdateService)
                        .AddSingleton(playGameService)
                        .AddTransient<MainWindowViewModel>()
                        .BuildServiceProvider();
    }

    private static StrongViewLocator CreatStrongViewLocator()
    {
        var viewLocator = new StrongViewLocator();
        viewLocator.Register<EditDataDialogViewModel, EditDataDialog>();
        viewLocator.Register<DeleteDataDialogViewModel, DeleteDataDialog>();
        viewLocator.Register<BackupDialogViewModel, BackupDialog>();
        viewLocator.Register<SaveDialogViewModel, SaveDialog>();
        viewLocator.Register<LogDialogViewModel, LogDialog>();
        viewLocator.Register<SettingDialogViewModel, SettingsDialog>();
        viewLocator.Register<TranslateDiaglogViewModel, TranslateDiaglog>();
        viewLocator.Register<RecentDialogViewModel, RecentDialog>();
        viewLocator.Register<BatchTranslateDialogViewModel, BatchTranslateDialog>();
        return viewLocator;
    }

    protected override void OnExit(ExitEventArgs e)
    {
        if (appSettings != null)
            File.WriteAllText(configPath, JsonConvert.SerializeObject(appSettings,
                                                                      Formatting.Indented,
                                                                      new StringEnumConverter()));
        logObserver?.Dispose();
    }
}