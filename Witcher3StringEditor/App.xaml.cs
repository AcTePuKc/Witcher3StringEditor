using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reactive;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using GTranslate.Translators;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Wpf;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Resourcer;
using Serilog;
using Serilog.Events;
using Syncfusion.Licensing;
using Witcher3StringEditor.Dialogs.Recipients;
using Witcher3StringEditor.Dialogs.ViewModels;
using Witcher3StringEditor.Dialogs.Views;
using Witcher3StringEditor.Locales;
using Witcher3StringEditor.Models;
using Witcher3StringEditor.Serializers;
using Witcher3StringEditor.Serializers.Abstractions;
using Witcher3StringEditor.Services;
using Witcher3StringEditor.Shared.Abstractions;
using Witcher3StringEditor.ViewModels;
using WPFLocalizeExtension.Engine;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

namespace Witcher3StringEditor;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App
{
    private readonly AppSettings appSettings;
    private readonly string configFolderPath;
    private readonly string configPath;
    private ILogger<App>? logger;
    private ObserverBase<LogEvent>? logObserver;

    public App()
    {
        configFolderPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            IsDebug ? "Witcher3StringEditor_Debug" : "Witcher3StringEditor");
        configPath = Path.Combine(configFolderPath, "AppSettings.Json");
        appSettings = LoadAppSettings();
    }

    private static bool IsDebug =>
        Assembly.GetExecutingAssembly().GetCustomAttribute<DebuggableAttribute>()?.IsJITTrackingEnabled == true;

    protected override void OnStartup(StartupEventArgs e)
    {
        if (IsAnotherInstanceRunning())
        {
            if (MessageBox.Show(Strings.MultipleInstanceMessage, Strings.MultipleInstanceCaption,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Information) == MessageBoxResult.Yes) ActivateExistingInstance();
            Current.Shutdown();
            return;
        }

        SetupExceptionHandling();
        InitializeServices();
        InitializeLogging();
        SyncfusionLicenseProvider.RegisterLicense(Resource.AsString("License.txt"));
        LocalizeDictionary.Instance.Culture = Thread.CurrentThread.CurrentCulture;
    }

    private void SetupExceptionHandling()
    {
        DispatcherUnhandledException += (_, e) =>
        {
            e.Handled = true;
            var exception = e.Exception;
            logger?.LogError(exception, "Unhandled exception: {ExceptionMessage}", exception.Message);
        };
        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            e.SetObserved();
            var exception = e.Exception;
            logger?.LogError(exception, "Unobserved task exception: {ExceptionMessage}", exception.Message);
        };
    }

    private static bool IsAnotherInstanceRunning()
    {
        using var _ = new Mutex(true, IsDebug ? "Witcher3StringEditor_Debug" : "Witcher3StringEditor",
            out var createdNew);
        return !createdNew;
    }

    private void InitializeLogging()
    {
        logObserver = new AnonymousObserver<LogEvent>(x =>
            WeakReferenceMessenger.Default.Send(new NotificationMessage<LogEvent>(x)));
        Log.Logger = new LoggerConfiguration().WriteTo.File(Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
                    , IsDebug ? "Witcher3StringEditor_Debug" : "Witcher3StringEditor", "Logs", "log.txt"),
                rollingInterval: RollingInterval.Day, formatProvider: CultureInfo.InvariantCulture)
            .WriteTo.Debug(formatProvider: CultureInfo.InvariantCulture).WriteTo
            .Observers(observable => observable.Subscribe(logObserver)).Enrich.FromLogContext()
            .CreateLogger();
        logger = Ioc.Default.GetRequiredService<ILogger<App>>();
    }

    private static void ActivateExistingInstance()
    {
        using var currentProcess = Process.GetCurrentProcess();
        using var existingProcess =
            Process.GetProcessesByName(currentProcess.ProcessName).First(p => p.Id != currentProcess.Id);
        var mainWindowHandle = new HWND(existingProcess.MainWindowHandle);
        var placement = new WINDOWPLACEMENT();
        placement.length = (uint)Marshal.SizeOf(placement);
        if (PInvoke.GetWindowPlacement(mainWindowHandle, ref placement).Value != 0)
            _ = placement.showCmd switch
            {
                SHOW_WINDOW_CMD.SW_SHOWMINIMIZED or SHOW_WINDOW_CMD.SW_SHOWMINNOACTIVE => PInvoke.ShowWindow(
                    mainWindowHandle, SHOW_WINDOW_CMD.SW_RESTORE),
                SHOW_WINDOW_CMD.SW_HIDE => PInvoke.ShowWindow(mainWindowHandle, SHOW_WINDOW_CMD.SW_SHOW),
                _ => new BOOL()
            };
    }


    private AppSettings LoadAppSettings()
    {
        return File.Exists(configPath)
            ? JsonConvert.DeserializeObject<AppSettings>(File.ReadAllText(configPath)) ?? new AppSettings()
            : new AppSettings();
    }

    private void InitializeServices()
    {
        Ioc.Default.ConfigureServices(new ServiceCollection()
            .AddLogging(builder => builder.AddSerilog())
            .AddSingleton<IViewLocator, StrongViewLocator>(_ => CreatStrongViewLocator())
            .AddSingleton<IAppSettings, AppSettings>(_ => appSettings)
            .AddSingleton<IBackupService, BackupService>()
            .AddSingleton<ICheckUpdateService, CheckUpdateService>()
            .AddSingleton<IDialogManager, DialogManager>()
            .AddSingleton<IDialogService, DialogService>()
            .AddSingleton<IExplorerService, ExplorerService>()
            .AddSingleton<IPlayGameService, PlayGameService>()
            .AddSingleton<IW3Serializer, W3Serializer>()
            .AddSingleton<ITranslator, MicrosoftTranslator>()
            .AddTransient<MainWindowViewModel>()
            .BuildServiceProvider());
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
        return viewLocator;
    }

    protected override void OnExit(ExitEventArgs e)
    {
        if (!Directory.Exists(configFolderPath))
            _ = Directory.CreateDirectory(configFolderPath);
        File.WriteAllText(configPath, JsonConvert.SerializeObject(Ioc.Default.GetRequiredService<IAppSettings>(),
            Formatting.Indented,
            new StringEnumConverter()));
        logger?.LogInformation("Application exited.");
        logObserver?.Dispose();
        Log.CloseAndFlush();
    }
}