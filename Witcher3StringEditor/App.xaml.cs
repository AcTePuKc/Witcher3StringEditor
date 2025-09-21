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
using Witcher3StringEditor.Serializers.Implementation;
using Witcher3StringEditor.Services;
using Witcher3StringEditor.ViewModels;
using Witcher3StringEditor.Views;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

namespace Witcher3StringEditor;

/// <summary>
///     Interaction logic for App.xaml
///     Main application class that handles startup, initialization, and shutdown processes
/// </summary>
public sealed partial class App : IDisposable
{
    private IAppSettings? appSettings;
    private IConfigService? configService;
    private bool disposedValue;
    private ObserverBase<LogEvent>? logObserver;
    private Mutex? mutex;

    /// <summary>
    ///     Gets a value indicating whether the application is running in debug mode
    /// </summary>
    private static bool IsDebug =>
        Assembly.GetExecutingAssembly().GetCustomAttribute<DebuggableAttribute>()?.IsJITTrackingEnabled == true;

    /// <summary>
    ///     Disposes of the resources used by the application
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
    }

    /// <summary>
    ///     Handles the startup of the application
    ///     Checks for existing instances and initializes the application if none is running
    /// </summary>
    /// <param name="e">Startup event arguments</param>
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

    /// <summary>
    ///     Initializes the application components
    ///     Sets up exception handling, services, settings, logging, and culture
    /// </summary>
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

    /// <summary>
    ///     Initializes the logging system
    ///     Sets up observers to capture log events
    /// </summary>
    private void InitializeLogging()
    {
        logObserver = new AnonymousObserver<LogEvent>(static x =>
            WeakReferenceMessenger.Default.Send(new ValueChangedMessage<LogEvent>(x)));
        InitializeLogging(logObserver);
    }

    /// <summary>
    ///     Initializes the application settings
    ///     Loads configuration service and application settings from the IoC container
    /// </summary>
    private void InitializeAppSettings()
    {
        configService = Ioc.Default.GetRequiredService<IConfigService>();
        appSettings = Ioc.Default.GetRequiredService<IAppSettings>();
    }

    /// <summary>
    ///     Gets the path to the application settings file
    ///     Creates the configuration folder if it doesn't exist
    /// </summary>
    /// <returns>The full path to the application settings file</returns>
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

    /// <summary>
    ///     Initializes the application culture (language)
    ///     Sets the culture based on saved settings or resolves the supported culture
    /// </summary>
    private void InitializeCulture()
    {
        var cultureInfo = appSettings!.Language == string.Empty
            ? Ioc.Default.GetRequiredService<ICultureResolver>().ResolveSupportedCulture()
            : new CultureInfo(appSettings.Language);
        if (appSettings.Language == string.Empty)
            appSettings.Language = cultureInfo.Name;
        I18NExtension.Culture = cultureInfo;
    }

    /// <summary>
    ///     Registers the Syncfusion license
    ///     Reads the license from embedded resources and registers it with Syncfusion
    /// </summary>
    private static void RegisterSyncfusionLicense()
    {
        using var stream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream("Witcher3StringEditor.License.txt")!;
        using var reader = new StreamReader(stream);
        SyncfusionLicenseProvider.RegisterLicense(reader.ReadToEnd());
    }

    /// <summary>
    ///     Sets up global exception handling
    ///     Registers handlers for unhandled exceptions and unobserved task exceptions
    /// </summary>
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

    /// <summary>
    ///     Checks if another instance of the application is already running
    ///     Uses a mutex to determine if this is the only instance
    /// </summary>
    /// <returns>True if another instance is running, false otherwise</returns>
    private bool IsAnotherInstanceRunning()
    {
        mutex = new Mutex(true, IsDebug ? "Witcher3StringEditor_Debug" : "Witcher3StringEditor",
            out var createdNew);
        return !createdNew;
    }

    /// <summary>
    ///     Initializes the logging system with the specified observer
    ///     Configures Serilog to write to file, debug output, and the specified observer
    /// </summary>
    /// <param name="observer">The observer to subscribe to log events</param>
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

    /// <summary>
    ///     Activates an existing instance of the application
    ///     Finds the existing process and brings its window to the foreground
    /// </summary>
    private static void ActivateExistingInstance()
    {
        using var existingProcess = FindExistingProcessInstance();
        var mainWindowHandle = new HWND(existingProcess.MainWindowHandle);
        ActivateExistingInstanceWindow(mainWindowHandle);
    }

    /// <summary>
    ///     Activates the window of an existing application instance
    ///     Restores the window if minimized and brings it to the foreground
    /// </summary>
    /// <param name="mainWindowHandle">The handle to the main window of the existing instance</param>
    private static void ActivateExistingInstanceWindow(HWND mainWindowHandle)
    {
        var placement = new WINDOWPLACEMENT();
        placement.length = (uint)Marshal.SizeOf(placement);
        if (PInvoke.GetWindowPlacement(mainWindowHandle, ref placement).Value == 0) return;
        if (placement.showCmd == SHOW_WINDOW_CMD.SW_SHOWMINIMIZED)
            PInvoke.ShowWindow(mainWindowHandle, SHOW_WINDOW_CMD.SW_RESTORE);
        PInvoke.SetForegroundWindow(mainWindowHandle);
    }

    /// <summary>
    ///     Finds an existing process instance of the application
    /// </summary>
    /// <returns>The existing process instance</returns>
    private static Process FindExistingProcessInstance()
    {
        using var currentProcess = Process.GetCurrentProcess();
        return Process.GetProcessesByName(currentProcess.ProcessName).First(p => p.Id != currentProcess.Id);
    }

    /// <summary>
    ///     Initializes the dependency injection services
    ///     Registers all services, view models, and other dependencies with the IoC container
    /// </summary>
    /// <param name="configPath">The path to the configuration file</param>
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
            .AddSingleton<ICsvW3Serializer, CsvW3Serializer>()
            .AddSingleton<IExcelW3Serializer, ExcelW3Serializer>()
            .AddSingleton<IW3StringsSerializer, W3StringsSerializer>()
            .AddSingleton<IW3Serializer, W3SerializerCoordinator>()
            .AddSingleton<IFileManagerService, FileManagerService>()
            .AddScoped<IExplorerService, ExplorerService>()
            .AddScoped<IPlayGameService, PlayGameService>()
            .AddScoped<ICheckUpdateService, CheckUpdateService>()
            .AddTransient<ITranslator, MicrosoftTranslator>()
            .AddTransient<ITranslator, GoogleTranslator>()
            .AddTransient<ITranslator, YandexTranslator>()
            .AddTransient<ISettingsManagerService, SettingsManagerService>()
            .AddTransient<MainWindowViewModel>()
            .BuildServiceProvider());
    }

    /// <summary>
    ///     Creates and configures the strong view locator
    ///     Registers view models with their corresponding views
    /// </summary>
    /// <returns>The configured StrongViewLocator</returns>
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

    /// <summary>
    ///     Handles the application exit event
    ///     Saves settings, flushes logs, and disposes resources
    /// </summary>
    /// <param name="e">Exit event arguments</param>
    protected override void OnExit(ExitEventArgs e)
    {
        configService?.Save(appSettings);
        Log.Information("Application exited.");
        Log.CloseAndFlush();
        Dispose();
    }

    /// <summary>
    ///     Disposes of the resources used by the application
    /// </summary>
    /// <param name="disposing">True if called from Dispose(), false if called from finalizer</param>
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