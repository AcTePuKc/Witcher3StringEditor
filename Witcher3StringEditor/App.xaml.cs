using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Wpf;
using Microsoft.Extensions.DependencyInjection;
using Resourcer;
using Serilog;
using Serilog.Events;
using Syncfusion.Licensing;
using System.IO;
using System.Reactive;
using System.Text.Json;
using System.Windows;
using Witcher3StringEditor.Core.Interfaces;
using Witcher3StringEditor.Dialogs.ViewModels;
using Witcher3StringEditor.Dialogs.Views;
using Witcher3StringEditor.Models;
using Witcher3StringEditor.ViewModels;
using WPFLocalizeExtension.Engine;

namespace Witcher3StringEditor;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App
{
    private const string ConfigPath = "Config.json";
    private IAppSettings? appSettings;

    protected override void OnStartup(StartupEventArgs e)
    {
        appSettings = File.Exists(ConfigPath)
            ? JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(ConfigPath)) ?? new AppSettings()
            : (IAppSettings)new AppSettings();
        var observer = new AnonymousObserver<LogEvent>(x => WeakReferenceMessenger.Default.Send(x));
        Log.Logger = new LoggerConfiguration().WriteTo.File(".\\Logs\\log.txt", rollingInterval: RollingInterval.Day)
            .WriteTo.Debug().WriteTo.Observers(observable => observable.Subscribe(observer)).Enrich.FromLogContext()
            .CreateLogger();
        SyncfusionLicenseProvider.RegisterLicense(Resource.AsString("License.txt"));
        Ioc.Default.ConfigureServices(
            new ServiceCollection()
                .AddLogging(builder => builder.AddSerilog())
                .AddSingleton<IDialogService>(new DialogService(new DialogManager(CreatStrongViewLocator()), Ioc.Default.GetService))
                .AddSingleton(appSettings)
                .AddTransient<MainWindowViewModel>()
                .BuildServiceProvider());

        LocalizeDictionary.Instance.Culture = Thread.CurrentThread.CurrentUICulture;
    }

    protected override void OnExit(ExitEventArgs e)
    {
        File.WriteAllText(ConfigPath, JsonSerializer.Serialize(appSettings));
        base.OnExit(e);
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
        return viewLocator;
    }
}