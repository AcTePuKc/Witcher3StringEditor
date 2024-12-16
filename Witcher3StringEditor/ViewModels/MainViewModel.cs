using CommandLine;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using Serilog;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using Witcher3StringEditor.Core;
using Witcher3StringEditor.Dialogs.ViewModels;
using Witcher3StringEditor.Locales;
using Witcher3StringEditor.Models;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;

namespace Witcher3StringEditor.ViewModels
{
    internal partial class MainViewModel : ObservableObject
    {
        private readonly IDialogService dialogService;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(OpenWorkingFolderCommand))]
        private string outputFolder = string.Empty;

        public ObservableCollection<W3ItemModel> W3Items { get; set; } = [];

        public MainViewModel(IDialogService dialogService)
        {
            this.dialogService = dialogService;

            W3Items.CollectionChanged += (_, _) =>
            {
                AddCommand.NotifyCanExecuteChanged();
                ShowSaveDialogCommand.NotifyCanExecuteChanged();
            };
        }

        [RelayCommand]
        private async Task WindowLoaded()
        {
            var settings = ConfigureManger.Load();
            var newSettings = new SettingsModel();
            if (settings == newSettings)
            {
                var dialogViewModel = new SettingDialogViewModel(newSettings);
                await dialogService.ShowDialogAsync(this, dialogViewModel);
            }
            else if (!File.Exists(settings.GameExePath) || !File.Exists(settings.W3StringsPath))
            {
                newSettings.PreferredFileType = settings.PreferredFileType;
                newSettings.PreferredLanguage = settings.PreferredLanguage;
                newSettings.GameExePath = File.Exists(settings.GameExePath) ? settings.GameExePath : string.Empty;
                newSettings.W3StringsPath = File.Exists(settings.W3StringsPath) ? settings.W3StringsPath : string.Empty;
                var dialogViewModel = new SettingDialogViewModel(newSettings);
                await dialogService.ShowDialogAsync(this, dialogViewModel);
            }
        }

        [RelayCommand]
        private async Task OpenFile()
        {
            var storageFile = await dialogService.ShowOpenFileDialogAsync(this, new OpenFileDialogSettings
            {
                Filters = [new FileFilter(Strings.AllSupportedFormat, [".csv",".w3strings"])
                , new FileFilter(Strings.TextFile, ".csv"), new FileFilter(Strings.Witcher3StringsFile, ".w3strings")]
            });

            if (storageFile != null)
            {
                if (W3Items.Any())
                {
                    W3Items.Clear();
                }

                foreach (var item in await W3Serializer.Deserialize(storageFile.LocalPath))
                {
                    W3Items.Add(new W3ItemModel(item));
                }

                OutputFolder = Path.GetDirectoryName(storageFile.LocalPath) ?? string.Empty;
            }
        }

        [RelayCommand(CanExecute = nameof(CanShowSaveDialog))]
        private async Task Add()
        {
            var dialogViewModel = new EditDataDialogViewModel();
            var result = await dialogService.ShowDialogAsync(this, dialogViewModel);
            if (result == true && dialogViewModel.W3Item != null)
            {
                W3Items.Add(dialogViewModel.W3Item);
            }
        }

        [RelayCommand]
        private async Task Edit(W3ItemModel w3Item)
        {
            var dialogViewModel = new EditDataDialogViewModel(w3Item);
            var result = await dialogService.ShowDialogAsync(this, dialogViewModel);
            if (result == true && dialogViewModel.W3Item != null)
            {
                var first = W3Items.First(x => x.Id == w3Item.Id);
                var index = W3Items.IndexOf(first);
                W3Items[index] = dialogViewModel.W3Item;
            }
        }

        [RelayCommand]
        private async Task Delete(IEnumerable<object> items)
        {
            var w3Items = items.Cast<W3ItemModel>().ToArray();
            if (w3Items.Length != 0)
            {
                var dialogViewModel = new DeleteDataDialogViewModel(w3Items);
                var result = await dialogService.ShowDialogAsync(this, dialogViewModel);
                if (result == true)
                {
                    foreach (var t in w3Items)
                    {
                        W3Items.Remove(t);
                    }
                }
            }
        }

        [RelayCommand]
        private async Task ShowBackupDialog()
        {
            var dialogViewModel = new BackupDialogViewModel();
            await dialogService.ShowDialogAsync(this, dialogViewModel);
        }

        private bool CanShowSaveDialog() => W3Items.Any();

        [RelayCommand(CanExecute = nameof(CanShowSaveDialog))]
        private async Task ShowSaveDialog()
        {
            var dialogViewModel = new SaveDialogViewModel(W3Items, OutputFolder);
            await dialogService.ShowDialogAsync(this, dialogViewModel);
        }

        [RelayCommand]
        private async Task ShowLogDialog()
        {
            var dialogViewModel = new LogDialogViewModel();
            await dialogService.ShowDialogAsync<LogDialogViewModel>(this, dialogViewModel);
        }

        [RelayCommand]
        private async Task ShowSettingsDialog()
        {
            var settings = ConfigureManger.Load();
            var dialogViewModel = new SettingDialogViewModel(settings);
            await dialogService.ShowDialogAsync(this, dialogViewModel);
        }

        [RelayCommand]
        private static async Task PlayGame()
        {
            var filename = ConfigureManger.Load().GameExePath;
            using var process = new Process();
            process.EnableRaisingEvents = true;
            process.StartInfo = new ProcessStartInfo
            {
                FileName = filename,
                WorkingDirectory = Path.GetDirectoryName(filename),
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };
            process.ErrorDataReceived += Process_ErrorDataReceived;
            process.OutputDataReceived += Process_OutputDataReceived;
            process.Start();
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();
            await process.WaitForExitAsync();
        }

        private static void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                Log.Error(e.Data);
            }
        }

        private static void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                Log.Information(e.Data);
            }
        }

        [RelayCommand]
        private static void SfDataGridDragEnter(DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;

            e.Handled = true;
        }

        [RelayCommand]
        private static void SfDataGridDragOver(DragEventArgs e)
        {
            // 可选：提供视觉反馈
            // 注意: 在某些情况下，你可能需要转换坐标系
            e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;

            e.Handled = true;
        }

        [RelayCommand]
        private async Task SfDataGridDrop(DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var file = e.Data.GetData(DataFormats.FileDrop).Cast<IEnumerable<string>>().ToArray()[0];
                var ext = Path.GetExtension(file);
                if (ext is ".csv" or ".w3strings")
                {
                    if (W3Items.Any())
                        W3Items.Clear();
                    foreach (var item in await W3Serializer.Deserialize(file))
                    {
                        W3Items.Add(new W3ItemModel(item));
                    }
                }
            }

            e.Handled = true;
        }

        [RelayCommand]
        private static async Task ShowAbout()
        {
            var buildTime = RetrieveTimestampAsDateTime();
            var runtime = RuntimeInformation.FrameworkDescription;
            var version = ThisAssembly.AssemblyInformationalVersion;
            var os = $"{RuntimeInformation.OSDescription}({RuntimeInformation.OSArchitecture})";
            await MessageBox.ShowAsync($"{Strings.Version}: {version}\n{Strings.BuildTime}: {buildTime}\n{Strings.OS}; {os}\n{Strings.Runtime}: {runtime}", Strings.About, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private static string RetrieveTimestamp()
        {
            var attribute = Assembly.GetExecutingAssembly()
                .GetCustomAttributesData()
                .First(x => x.AttributeType.Name == "TimestampAttribute");

            return attribute.ConstructorArguments.First().Value as string ?? string.Empty;
        }

        private static DateTime RetrieveTimestampAsDateTime()
        {
            var timestamp = RetrieveTimestamp();
            return timestamp == string.Empty ? new DateTime() : DateTime.ParseExact(timestamp, "yyyy-MM-ddTHH:mm:ss.fffZ", null, DateTimeStyles.AssumeUniversal).ToLocalTime();
        }

        [RelayCommand(CanExecute = nameof(CanOpenWorkingFolder))]
        private void OpenWorkingFolder()
        {
            Process.Start("explorer.exe", OutputFolder);
        }

        private bool CanOpenWorkingFolder() => Directory.Exists(OutputFolder);

        [RelayCommand]
        private static void OpenNexusMods()
        {
            Process.Start("explorer.exe", "https://www.nexusmods.com/witcher3/mods/10032");
        }
    }
}