using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using HanumanInstitute.MvvmDialogs;
using Microsoft.Extensions.Logging;
using Witcher3StringEditor.Dialogs.Recipients;
using Witcher3StringEditor.Interfaces;

namespace Witcher3StringEditor.Dialogs.ViewModels;

public partial class SaveDialogViewModel
    : ObservableObject, IModalDialogViewModel, ICloseable
{
    private readonly ILogger<SaveDialogViewModel> logger;
    private readonly IW3Serializer serializer;

    [ObservableProperty] private IW3Job w3Job;

    public SaveDialogViewModel(IW3Serializer serializer, ILogger<SaveDialogViewModel> logger, IW3Job w3Job)
    {
        W3Job = w3Job;
        W3Job.IdSpace = FindIdSpace(W3Job.W3Items.First());
        this.serializer = serializer;
        this.logger = logger;
    }

    public event EventHandler? RequestClose;

    public bool? DialogResult { get; private set; }

    [RelayCommand]
    private async Task Save()
    {
        var saveResult = await serializer.Serialize(W3Job);
        logger.LogInformation("Target filetype: {FileType}.", W3Job.W3FileType);
        logger.LogInformation("Target language: {Language}.", W3Job.Language);
        logger.LogInformation("Sve result: {Result}.", saveResult);
        WeakReferenceMessenger.Default.Send(new NotificationMessage<bool>(saveResult), "Save");
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
        var match = IdSpaceRegex().Match(w3Item.StrId);
        if (!match.Success) return -1;
        var foundIdSpace = match.Groups[1].Value;
        return int.Parse(foundIdSpace);
    }

    [GeneratedRegex(@"^211(\d{4})\d{3}$")]
    private static partial Regex IdSpaceRegex();
}