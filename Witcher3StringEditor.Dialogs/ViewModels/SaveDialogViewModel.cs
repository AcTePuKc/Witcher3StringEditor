using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using HanumanInstitute.MvvmDialogs;
using System.Text.RegularExpressions;
using Witcher3StringEditor.Dialogs.Recipients;
using Witcher3StringEditor.Interfaces;

namespace Witcher3StringEditor.Dialogs.ViewModels;

public partial class SaveDialogViewModel : ObservableObject, IModalDialogViewModel, ICloseable
{
    private readonly IW3Serializer serializer;

    [ObservableProperty] private IW3Job w3Job;

    public SaveDialogViewModel(IW3Serializer serializer, IW3Job w3Job)
    {
        W3Job = w3Job;
        W3Job.IdSpace = FindIdSpace(W3Job.W3Items.First());
        this.serializer = serializer;
    }

    public event EventHandler? RequestClose;

    public bool? DialogResult { get; private set; }

    [RelayCommand]
    private async Task Save()
    {
        var result = await serializer.Serialize(W3Job);
        var message = new SaveResultMessage(result);
        WeakReferenceMessenger.Default.Send(message);
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
        if (!match.Success) return -1;
        // 如果匹配成功，则提取捕获组中的值
        var foundIdSpace = match.Groups[1].Value;
        return int.Parse(foundIdSpace);
    }

    [GeneratedRegex(@"^211(\d{4})\d{3}$")]
    private static partial Regex IdSpaceRegex();
}