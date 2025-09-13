using System.Globalization;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using HanumanInstitute.MvvmDialogs;
using Serilog;
using Witcher3StringEditor.Common;
using Witcher3StringEditor.Common.Abstractions;
using Witcher3StringEditor.Serializers;
using Witcher3StringEditor.Serializers.Abstractions;

namespace Witcher3StringEditor.Dialogs.ViewModels;

public partial class SaveDialogViewModel
    : ObservableObject, IModalDialogViewModel, ICloseable
{
    private readonly IW3Serializer serializer;

    private readonly IReadOnlyList<IW3StringItem> w3StringItems;

    [ObservableProperty] private W3FileType targetFileType;

    [ObservableProperty] private int idSpace;

    [ObservableProperty] private bool isIgnoreIdSpaceCheck;

    [ObservableProperty] private W3Language targetLanguage;

    [ObservableProperty] private string savePath;

    public SaveDialogViewModel(IAppSettings appSettings, IW3Serializer serializer,
        IReadOnlyList<IW3StringItem> w3StringItems, string savePath)
    {
        SavePath = savePath;
        this.w3StringItems = w3StringItems;
        this.serializer = serializer;
        IdSpace = FindIdSpace(w3StringItems[0]);
        TargetLanguage = appSettings.PreferredLanguage;
        TargetFileType = appSettings.PreferredW3FileType;
    }

    public event EventHandler? RequestClose;

    public bool? DialogResult { get; private set; }

    [RelayCommand]
    private async Task Save()
    {
        Log.Information("Target filetype: {FileType}.", TargetFileType);
        Log.Information("Target language: {Language}.", TargetLanguage);
        var saveResult = await serializer.Serialize(w3StringItems, new W3SerializationContext
        {
            OutputDirectory = SavePath,
            ExpectedIdSpace = IdSpace,
            TargetFileType = TargetFileType,
            TargetLanguage = TargetLanguage,
            IgnoreIdSpaceCheck = IsIgnoreIdSpaceCheck
        });
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

    private static int FindIdSpace(IW3StringItem iw3StringItem)
    {
        var match = IdSpaceRegex().Match(iw3StringItem.StrId);
        if (!match.Success) return -1;
        var foundIdSpace = match.Groups[1].Value;
        return int.Parse(foundIdSpace, CultureInfo.InvariantCulture);
    }

    [GeneratedRegex(@"^211(\d{4})\d{3}$")]
    private static partial Regex IdSpaceRegex();
}