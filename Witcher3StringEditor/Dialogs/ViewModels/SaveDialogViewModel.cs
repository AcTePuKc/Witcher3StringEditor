using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.MvvmDialogs;
using System.Text.RegularExpressions;
using System.Windows;
using Witcher3StringEditor.Core;
using Witcher3StringEditor.Core.Interfaces;
using Witcher3StringEditor.Dialogs.Models;
using Witcher3StringEditor.Locales;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

namespace Witcher3StringEditor.Dialogs.ViewModels;

internal partial class SaveDialogViewModel : ObservableObject, IModalDialogViewModel, ICloseable
{
    [ObservableProperty] private IW3Job w3Job;

    public SaveDialogViewModel(IEnumerable<IW3Item> w3Items, string path)
    {
        W3Job = new W3Job
        {
            Path = path,
            W3Items = w3Items,
            FileType = SettingsManager.LoadConfiguration().PreferredFileType,
            Language = SettingsManager.LoadConfiguration().PreferredLanguage,
            IdSpace = FindIdSpace(w3Items.First())
        };
    }

    public event EventHandler? RequestClose;

    public bool? DialogResult { get; private set; }

    [RelayCommand]
    private async Task Save()
    {
        var result = await W3Serializer.Serialize(W3Job);

        if (result)
            await MessageBox.ShowAsync(Strings.SaveSuccess, Strings.SaveResult, MessageBoxButton.OK,
                MessageBoxImage.Information);
        else
            await MessageBox.ShowAsync(Strings.SaveFailure, Strings.SaveResult, MessageBoxButton.OK,
                MessageBoxImage.Information);

        DialogResult = true;
        RequestClose?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void Cancel()
    {
        DialogResult = false;
        RequestClose?.Invoke(this, EventArgs.Empty);
    }

    private static int FindIdSpace(IW3Item w3Item)
    {
        // 使用 Match 方法尝试匹配输入字符串
        var match = IdSpaceRegex().Match(w3Item.StrId);
        if (!match.Success) return 1;
        // 如果匹配成功，则提取捕获组中的值
        var foundIdSpace = match.Groups[1].Value;
        return int.Parse(foundIdSpace);
    }

    [GeneratedRegex(@"^211(\d{4})\d{3}$")]
    private static partial Regex IdSpaceRegex();
}