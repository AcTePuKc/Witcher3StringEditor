using System.Globalization;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using HanumanInstitute.MvvmDialogs;
using Serilog;
using Witcher3StringEditor.Common.Abstractions;
using Witcher3StringEditor.Dialogs.Models;
using Witcher3StringEditor.Serializers.Abstractions;

namespace Witcher3StringEditor.Dialogs.ViewModels;

public partial class SaveDialogViewModel
    : ObservableObject, IModalDialogViewModel, ICloseable
{
    private readonly IW3Serializer _serializer;

    [ObservableProperty] private IW3Job _w3Job;

    public SaveDialogViewModel(IAppSettings appSettings, IW3Serializer serializer,
        IReadOnlyList<IW3Item> w3Items, string path)
    {
        _serializer = serializer;
        W3Job = new W3JobModel
        {
            Path = path,
            W3Items = w3Items,
            IdSpace = FindIdSpace(w3Items[0]),
            Language = appSettings.PreferredLanguage,
            W3FileType = appSettings.PreferredW3FileType
        };
    }

    public event EventHandler? RequestClose;

    public bool? DialogResult { get; private set; }

    [RelayCommand]
    private async Task Save()
    {
        Log.Information("Target filetype: {FileType}.", W3Job.W3FileType);
        Log.Information("Target language: {Language}.", W3Job.Language);
        var saveResult = await _serializer.Serialize(W3Job);
        Log.Information("Sve result: {Result}.", saveResult);
        WeakReferenceMessenger.Default.Send(new ValueChangedMessage<bool>(saveResult), "Save");
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
        return int.Parse(foundIdSpace, CultureInfo.InvariantCulture);
    }

    [GeneratedRegex(@"^211(\d{4})\d{3}$")]
    private static partial Regex IdSpaceRegex();
}