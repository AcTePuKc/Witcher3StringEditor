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
    private ObserverBase<LogEvent>? logObserver;

    private readonly string configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                                      "Witcher3StringEditor",
                                                      "AppSettings.Json");

    protected override void OnStartup(StartupEventArgs e)
    {
        logObserver = new AnonymousObserver<LogEvent>(x => WeakReferenceMessenger.Default.Send(x));
        Log.Logger = new LoggerConfiguration().WriteTo.File(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
            , "Witcher3StringEditor", "Logs", "log.txt"), rollingInterval: RollingInterval.Day)
            .WriteTo.Debug().WriteTo.Observers(observable => observable.Subscribe(logObserver)).Enrich.FromLogContext()
            .CreateLogger();
        SyncfusionLicenseProvider.RegisterLicense(Resource.AsString("License.txt"));
        Ioc.Default.ConfigureServices(InitializeServices(configPath));
        LocalizeDictionary.Instance.Culture = Thread.CurrentThread.CurrentCulture;
        DispatcherUnhandledException += (_, eventArgs) => Log.Error(eventArgs.Exception, "Unhandled exception occurred.");
        TaskScheduler.UnobservedTaskException += (_, eventArgs) => Log.Error(eventArgs.Exception, "Unhandled exception occurred.");
    }

    private static AppSettings LoadAppSettings(string path)
        => File.Exists(path) ? JsonConvert.DeserializeObject<AppSettings>(File.ReadAllText(path)) ?? new AppSettings() : new AppSettings();

    private static ServiceProvider InitializeServices(string path)
    {
        return new ServiceCollection()
            .AddSingleton<IAppSettings, AppSettings>(_ => LoadAppSettings(path))
            .AddSingleton<IDialogManager, DialogManager>(_ => new DialogManager(CreatStrongViewLocator()))
            .AddSingleton<IBackupService, BackupService>(_ => new BackupService(Ioc.Default.GetRequiredService<IAppSettings>()))
            .AddSingleton<IW3Serializer, W3Serializer>(_ => new W3Serializer(Ioc.Default.GetRequiredService<IAppSettings>(), Ioc.Default.GetRequiredService<IBackupService>()))
            .AddSingleton<IDialogService, DialogService>(_ => new DialogService(Ioc.Default.GetService<IDialogManager>(), Ioc.Default.GetService))
            .AddSingleton<ICheckUpdateService, CheckUpdateService>(_ => new CheckUpdateService(Ioc.Default.GetRequiredService<IAppSettings>()))
            .AddSingleton<IPlayGameService, PlayGameService>(_ => new PlayGameService(Ioc.Default.GetRequiredService<IAppSettings>()))
            .AddSingleton<IExplorerService, ExplorerService>(_ => new ExplorerService())
            .AddLogging(builder => builder.AddSerilog())
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
        viewLocator.Register<AboutDialogViewModel, AboutDialog>();
        return viewLocator;
    }

    protected override void OnExit(ExitEventArgs e)
    {
        var configFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Witcher3StringEditor");
        if (!Directory.Exists(configFolderPath))
            _ = Directory.CreateDirectory(configFolderPath);
        File.WriteAllText(configPath, JsonConvert.SerializeObject(Ioc.Default.GetRequiredService<IAppSettings>(),
            Formatting.Indented,
            new StringEnumConverter()));
        logObserver?.Dispose();
    }
}