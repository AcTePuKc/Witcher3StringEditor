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
    private readonly IAppSettings appSettings;
    private readonly ObserverBase<LogEvent> logObserver;

    private readonly string configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                                      "Witcher3StringEditor",
                                                      "AppSettings.Json");

    public App()
    {
        appSettings = File.Exists(configPath)
            ? JsonConvert.DeserializeObject<AppSettings>(File.ReadAllText(configPath)) ?? new AppSettings()
            : new AppSettings();
        logObserver = new AnonymousObserver<LogEvent>(x => WeakReferenceMessenger.Default.Send(x));
        Log.Logger = new LoggerConfiguration().WriteTo.File(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
            , "Witcher3StringEditor", "Logs", "log.txt"), rollingInterval: RollingInterval.Day)
            .WriteTo.Debug().WriteTo.Observers(observable => observable.Subscribe(logObserver)).Enrich.FromLogContext()
            .CreateLogger();
        SyncfusionLicenseProvider.RegisterLicense(Resource.AsString("License.txt"));
        Ioc.Default.ConfigureServices(InitializeServices(appSettings));
        LocalizeDictionary.Instance.Culture = Thread.CurrentThread.CurrentCulture;
        DispatcherUnhandledException += (_, e) => Log.Error(e.Exception, "Unhandled exception occurred.");
        TaskScheduler.UnobservedTaskException += (_, e) => Log.Error(e.Exception, "Unhandled exception occurred.");
        Exit += App_Exit;
    }

    private static ServiceProvider InitializeServices(IAppSettings appSettings)
    {
        return new ServiceCollection()
            .AddSingleton(appSettings)
            .AddSingleton<IBackupService, BackupService>(x => new BackupService(appSettings))
            .AddSingleton<IW3Serializer, W3Serializer>(x => new W3Serializer(appSettings, new BackupService(appSettings)))
            .AddSingleton<IDialogService, DialogService>(x => new DialogService(new DialogManager(CreatStrongViewLocator()), Ioc.Default.GetService))
            .AddSingleton<ICheckUpdateService, CheckUpdateService>(x => new CheckUpdateService(appSettings))
            .AddSingleton<IPlayGameService, PlayGameService>(x => new PlayGameService(appSettings))
            .AddSingleton<IExplorerService, ExplorerService>(x => new ExplorerService())
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

    private void App_Exit(object sender, ExitEventArgs e)
    {
        var configFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Witcher3StringEditor");
        if (!Directory.Exists(configFolderPath))
            Directory.CreateDirectory(configFolderPath);
        File.WriteAllText(configPath, JsonConvert.SerializeObject(appSettings,
            Formatting.Indented,
            new StringEnumConverter()));
        logObserver.Dispose();
    }
}