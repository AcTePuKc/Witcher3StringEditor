using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using System.ComponentModel;
using System.IO;
using System.Windows;
using Witcher3StringEditor.Core;
using Witcher3StringEditor.Locales;
using Witcher3StringEditor.Models;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

namespace Witcher3StringEditor.Dialogs.ViewModels
{
    internal partial class SettingDialogViewModel(SettingsModel settings) : ObservableObject, IModalDialogViewModel
    {
        public bool? DialogResult => true;

        [ObservableProperty]
        private SettingsModel settingsModel = settings;

        private readonly IDialogService dialogService = Ioc.Default.GetRequiredService<IDialogService>();

        [RelayCommand]
        private async Task SetW3StringsPath()
        {
            var dialogSettings = new OpenFileDialogSettings
            {
                Filters = [new FileFilter("w3strings.exe", ".exe")],
                Title = Strings.SelectW3Strings,
                SuggestedFileName = "w3strings"
            };
            var storageFile = await dialogService.ShowOpenFileDialogAsync(this, dialogSettings);
            if (storageFile != null && storageFile.Name == "w3strings.exe")
            {
                SettingsModel.W3StringsPath = storageFile.LocalPath;
            }
        }

        [RelayCommand]
        private async Task SetGameExePath()
        {
            var dialogSettings = new OpenFileDialogSettings
            {
                Filters = [new FileFilter("witcher3.exe", ".exe")],
                Title = Strings.SelectGameExe,
                SuggestedFileName = "witcher3"
            };
            var storageFile = await dialogService.ShowOpenFileDialogAsync(this, dialogSettings);
            if (storageFile != null && storageFile.Name == "witcher3.exe")
            {
                SettingsModel.GameExePath = storageFile.LocalPath;
            }
        }

        [RelayCommand]
        private async Task WindowClosingCancel(CancelEventArgs cancelEvent)
        {
            if (Path.GetFileName(SettingsModel.GameExePath) != "witcher3.exe"
                || Path.GetFileName(SettingsModel.W3StringsPath) != "w3strings.exe")
            {
                cancelEvent.Cancel = true;

                if (await MessageBox.ShowAsync(Strings.PleaseCheckSettings, Strings.Warning, System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    Environment.Exit(0);
                }
            }
            else
            {
                ConfigureManger.Save(SettingsModel);
            }
        }
    }
}