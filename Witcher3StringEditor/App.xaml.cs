using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using GTranslate.Translators;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Wpf;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Resourcer;
using Serilog;
using Serilog.Events;
using Syncfusion.Licensing;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reactive;
using System.Reflection;
using System.Windows;
using Witcher3StringEditor.Dialogs.Recipients;
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
    private Mutex? mutex;

    private ObserverBase<LogEvent>? logObserver;

    private readonly string configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                                      Debugger.IsAttached ? "Witcher3StringEditor_Debug" : "Witcher3StringEditor",
                                                      "AppSettings.Json");

    protected override void OnStartup(StartupEventArgs e)
    {
        logObserver = new AnonymousObserver<LogEvent>(x => WeakReferenceMessenger.Default.Send(new NotificationMessage<LogEvent>(x)));
        Log.Logger = new LoggerConfiguration().WriteTo.File(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
            , "Witcher3StringEditor", "Logs", "log.txt"), rollingInterval: RollingInterval.Day, formatProvider: CultureInfo.InvariantCulture)
            .WriteTo.Debug(formatProvider: CultureInfo.InvariantCulture).WriteTo.Observers(observable => observable.Subscribe(logObserver)).Enrich.FromLogContext()
            .CreateLogger();
        SyncfusionLicenseProvider.RegisterLicense(Resource.AsString("License.txt"));
        Ioc.Default.ConfigureServices(InitializeServices(configPath));
        LocalizeDictionary.Instance.Culture = Thread.CurrentThread.CurrentCulture;
        DispatcherUnhandledException += (_, eventArgs) => Log.Error(eventArgs.Exception, "Unhandled exception occurred.");
        TaskScheduler.UnobservedTaskException += (_, eventArgs) => Log.Error(eventArgs.Exception, "Unhandled exception occurred.");
        mutex = new Mutex(true, Assembly.GetExecutingAssembly().GetName().Name, out var createdNew);
        if (!createdNew) Current.Shutdown();
    }

    private static AppSettings LoadAppSettings(string path)
        => File.Exists(path) ? JsonConvert.DeserializeObject<AppSettings>(File.ReadAllText(path)) ?? new AppSettings() : new AppSettings();

    private static ServiceProvider InitializeServices(string path)
    {
        return new ServiceCollection()
            .AddLogging(builder => builder.AddSerilog())
            .AddSingleton<IAppSettings, AppSettings>(_ => LoadAppSettings(path))
            .AddSingleton<IViewLocator, StrongViewLocator>(_ => CreatStrongViewLocator())
            .AddSingleton<IDialogManager, DialogManager>()
            .AddSingleton<IDialogService, DialogService>()
            .AddSingleton<IBackupService, BackupService>()
            .AddSingleton<IW3Serializer, W3Serializer>()
            .AddSingleton<ICheckUpdateService, CheckUpdateService>()
            .AddSingleton<IPlayGameService, PlayGameService>()
            .AddSingleton<IExplorerService, ExplorerService>()
            .AddSingleton<MicrosoftTranslator>()
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
        viewLocator.Register<TranslateDialogViewModel, TranslateDialog>();
        viewLocator.Register<RecentDialogViewModel, RecentDialog>();
        viewLocator.Register<AboutDialogViewModel, AboutDialog>();
        viewLocator.Register<ModelSettingsDialogViewModel, ModelSettingsDialog>();
        viewLocator.Register<PromptsSettingDialogViewModel, PromotsSettingDialog>();
        return viewLocator;
    }

    protected override void OnExit(ExitEventArgs e)
    {
        var configFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Debugger.IsAttached ? "Witcher3StringEditor_Debug" : "Witcher3StringEditor");
        if (!Directory.Exists(configFolderPath))
            _ = Directory.CreateDirectory(configFolderPath);
        File.WriteAllText(configPath, JsonConvert.SerializeObject(Ioc.Default.GetRequiredService<IAppSettings>(),
            Formatting.Indented,
            new StringEnumConverter()));
        logObserver?.Dispose();
        mutex?.Dispose();
    }
}