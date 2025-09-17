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
using CommunityToolkit.Mvvm.Messaging.Messages;
using GTranslate.Translators;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Wpf;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Syncfusion.Licensing;
using Witcher3StringEditor.Common.Abstractions;
using Witcher3StringEditor.Dialogs.ViewModels;
using Witcher3StringEditor.Dialogs.Views;
using Witcher3StringEditor.Locales;
using Witcher3StringEditor.Models;
using Witcher3StringEditor.Serializers;
using Witcher3StringEditor.Serializers.Abstractions;
using Witcher3StringEditor.Services;
using Witcher3StringEditor.ViewModels;
using Witcher3StringEditor.Views;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

namespace Witcher3StringEditor;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public sealed partial class App : IDisposable
{
    private IAppSettings? appSettings;
    private IConfigService? configService;
    private bool disposedValue;
    private ObserverBase<LogEvent>? logObserver;
    private Mutex? mutex;

    private static bool IsDebug =>
        Assembly.GetExecutingAssembly().GetCustomAttribute<DebuggableAttribute>()?.IsJITTrackingEnabled == true;

    public void Dispose()
    {
        Dispose(true);
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        if (IsAnotherInstanceRunning())
        {
            if (MessageBox.Show(Strings.MultipleInstanceMessage, Strings.MultipleInstanceCaption,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Information) == MessageBoxResult.Yes) ActivateExistingInstance();
            Shutdown();
        }
        else
        {
            InitializeApplication();
        }
    }

    private void InitializeApplication()
    {
        SetupExceptionHandling();
        InitializeServices(GetAppSettingsPath());
        InitializeAppSettings();
        InitializeLogging();
        RegisterSyncfusionLicense();
        InitializeCulture();
        new MainWindow().Show();
    }

    private void InitializeLogging()
    {
        logObserver = new AnonymousObserver<LogEvent>(static x =>
            WeakReferenceMessenger.Default.Send(new ValueChangedMessage<LogEvent>(x)));
        InitializeLogging(logObserver);
    }

    private void InitializeAppSettings()
    {
        configService = Ioc.Default.GetRequiredService<IConfigService>();
        appSettings = Ioc.Default.GetRequiredService<IAppSettings>();
    }

    private static string GetAppSettingsPath()
    {
        var configFolderPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            IsDebug ? "Witcher3StringEditor_Debug" : "Witcher3StringEditor");
        var configPath = Path.Combine(configFolderPath, "AppSettings.Json");
        if (!Directory.Exists(configFolderPath))
            Directory.CreateDirectory(configFolderPath);
        return configPath;
    }

    private void InitializeCulture()
    {
        var cultureInfo = appSettings!.Language == string.Empty
            ? Ioc.Default.GetRequiredService<ICultureResolver>().ResolveSupportedCulture()
            : new CultureInfo(appSettings.Language);
        if (appSettings.Language == string.Empty)
            appSettings.Language = cultureInfo.Name;
        I18NExtension.Culture = cultureInfo;
    }

    private static void RegisterSyncfusionLicense()
    {
        using var stream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream("Witcher3StringEditor.License.txt")!;
        using var reader = new StreamReader(stream);
        SyncfusionLicenseProvider.RegisterLicense(reader.ReadToEnd());
    }

    private void SetupExceptionHandling()
    {
        DispatcherUnhandledException += static (_, e) =>
        {
            e.Handled = true;
            var exception = e.Exception;
            Log.Error(exception, "Unhandled exception: {ExceptionMessage}", exception.Message);
        };
        TaskScheduler.UnobservedTaskException += static (_, e) =>
        {
            e.SetObserved();
            var exception = e.Exception;
            Log.Error(exception, "Unobserved task exception: {ExceptionMessage}", exception.Message);
        };
    }

    private bool IsAnotherInstanceRunning()
    {
        mutex = new Mutex(true, IsDebug ? "Witcher3StringEditor_Debug" : "Witcher3StringEditor",
            out var createdNew);
        return !createdNew;
    }

    private static void InitializeLogging(IObserver<LogEvent> observer)
    {
        Log.Logger = new LoggerConfiguration().WriteTo.File(Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
                    , IsDebug ? "Witcher3StringEditor_Debug" : "Witcher3StringEditor", "Logs", "log.txt"),
                rollingInterval: RollingInterval.Day, formatProvider: CultureInfo.InvariantCulture)
            .WriteTo.Debug(formatProvider: CultureInfo.InvariantCulture)
            .WriteTo.Observers(observable => observable.Subscribe(observer))
            .CreateLogger();
    }

    private static void ActivateExistingInstance()
    {
        using var existingProcess = FindExistingProcessInstance();
        var mainWindowHandle = new HWND(existingProcess.MainWindowHandle);
        ActivateExistingInstanceWindow(mainWindowHandle);
    }

    private static void ActivateExistingInstanceWindow(HWND mainWindowHandle)
    {
        var placement = new WINDOWPLACEMENT();
        placement.length = (uint)Marshal.SizeOf(placement);
        if (PInvoke.GetWindowPlacement(mainWindowHandle, ref placement).Value == 0) return;
        if (placement.showCmd == SHOW_WINDOW_CMD.SW_SHOWMINIMIZED)
            PInvoke.ShowWindow(mainWindowHandle, SHOW_WINDOW_CMD.SW_RESTORE);
        PInvoke.SetForegroundWindow(mainWindowHandle);
    }

    private static Process FindExistingProcessInstance()
    {
        using var currentProcess = Process.GetCurrentProcess();
        return Process.GetProcessesByName(currentProcess.ProcessName).First(p => p.Id != currentProcess.Id);
    }

    private static void InitializeServices(string configPath)
    {
        Ioc.Default.ConfigureServices(new ServiceCollection()
            .AddLogging(builder => builder.AddSerilog())
            .AddSingleton<IViewLocator, StrongViewLocator>(_ => CreatStrongViewLocator())
            .AddSingleton<IAppSettings, AppSettings>(_ =>
                Ioc.Default.GetRequiredService<IConfigService>().Load<AppSettings>())
            .AddSingleton<IBackupService, BackupService>()
            .AddSingleton<ICultureResolver, CultureResolver>()
            .AddSingleton<IConfigService, ConfigService>(_ => new ConfigService(configPath))
            .AddSingleton<IDialogManager, DialogManager>()
            .AddSingleton<IDialogService, DialogService>()
            .AddSingleton<IW3Serializer, W3Serializer>()
            .AddScoped<IExplorerService, ExplorerService>()
            .AddScoped<IPlayGameService, PlayGameService>()
            .AddScoped<ICheckUpdateService, CheckUpdateService>()
            .AddTransient<ITranslator, MicrosoftTranslator>()
            .AddTransient<ITranslator, GoogleTranslator>()
            .AddTransient<ITranslator, YandexTranslator>()
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
        configService?.Save(appSettings);
        Log.Information("Application exited.");
        Log.CloseAndFlush();
        Dispose();
    }

    private void Dispose(bool disposing)
    {
        if (disposedValue) return;
        if (disposing)
        {
            mutex?.Dispose();
            logObserver?.Dispose();
        }

        disposedValue = true;
    }
}