using CommunityToolkit.Mvvm.DependencyInjection;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Wpf;
using Microsoft.Extensions.DependencyInjection;
using Resourcer;
using Serilog;
using Serilog.Events;
using Syncfusion.Licensing;
using System.Reactive;
using System.Windows;
using Witcher3StringEditor.Core;
using Witcher3StringEditor.Dialogs.ViewModels;
using Witcher3StringEditor.Dialogs.Views;
using Witcher3StringEditor.ViewModels;
using WPFLocalizeExtension.Engine;

namespace Witcher3StringEditor;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App
{
    protected override void OnStartup(StartupEventArgs e)
    {
        var observer = new AnonymousObserver<LogEvent>(LogManger.Log);
        Log.Logger = new LoggerConfiguration().WriteTo.File(".\\Logs\\log.txt", rollingInterval: RollingInterval.Day)
            .WriteTo.Debug().WriteTo.Observers(observable => observable.Subscribe(observer)).Enrich.FromLogContext()
            .CreateLogger();
        SyncfusionLicenseProvider.RegisterLicense(Resource.AsString("License.txt"));
        var viewLocator = new StrongViewLocator();
        viewLocator.Register<EditDataDialogViewModel, EditDataDialog>();
        viewLocator.Register<DeleteDataDialogViewModel, DeleteDataDialog>();
        viewLocator.Register<BackupDialogViewModel, BackupDialog>();
        viewLocator.Register<SaveDialogViewModel, SaveDialog>();
        viewLocator.Register<LogDialogViewModel, LogDialog>();
        viewLocator.Register<SettingDialogViewModel, SettingsDialog>();
        Ioc.Default.ConfigureServices(
            new ServiceCollection()
                .AddLogging(builder => builder.AddSerilog())
                .AddSingleton<IDialogService>(new DialogService(new DialogManager(viewLocator), Ioc.Default.GetService))
                .AddTransient<MainViewModel>()
                .BuildServiceProvider());

        LocalizeDictionary.Instance.Culture = Thread.CurrentThread.CurrentUICulture;
    }
}