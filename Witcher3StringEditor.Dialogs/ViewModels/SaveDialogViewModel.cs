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
using Witcher3StringEditor.Common.Constants;
using Witcher3StringEditor.Serializers;
using Witcher3StringEditor.Serializers.Abstractions;

namespace Witcher3StringEditor.Dialogs.ViewModels;

/// <summary>
///     ViewModel for the save dialog window
///     Handles saving W3 string items to a file with specified settings
///     Implements IModalDialogViewModel for dialog result handling and ICloseable for close notifications
/// </summary>
public partial class SaveDialogViewModel
    : ObservableObject, IModalDialogViewModel, ICloseable
{
    /// <summary>
    ///     The serializer used to save the W3 string items
    /// </summary>
    private readonly IW3Serializer serializer;

    /// <summary>
    ///     The collection of W3 string items to save
    /// </summary>
    private readonly IReadOnlyList<IW3StringItem> w3StringItems;

    /// <summary>
    ///     Gets or sets the ID space value for the items being saved
    /// </summary>
    [ObservableProperty] private int idSpace;

    /// <summary>
    ///     Gets or sets a value indicating whether to ignore ID space checking during save
    /// </summary>
    [ObservableProperty] private bool isIgnoreIdSpaceCheck;

    /// <summary>
    ///     Gets or sets the output directory where the file will be saved
    /// </summary>
    [ObservableProperty] private string outputDirectory;

    /// <summary>
    ///     Gets or sets the target file type for the save operation
    /// </summary>
    [ObservableProperty] private W3FileType targetFileType;

    /// <summary>
    ///     Gets or sets the target language for the save operation
    /// </summary>
    [ObservableProperty] private W3Language targetLanguage;

    /// <summary>
    ///     Initializes a new instance of the SaveDialogViewModel class
    /// </summary>
    /// <param name="appSettings">Application settings to get preferred language and file type</param>
    /// <param name="serializer">The serializer to use for saving the items</param>
    /// <param name="w3StringItems">The collection of W3 string items to save</param>
    /// <param name="outputDirectory">The initial output directory for saving</param>
    public SaveDialogViewModel(IAppSettings appSettings, IW3Serializer serializer,
        IReadOnlyList<IW3StringItem> w3StringItems, string outputDirectory)
    {
        OutputDirectory = outputDirectory;
        this.w3StringItems = w3StringItems;
        this.serializer = serializer;
        IdSpace = FindIdSpace(w3StringItems[0]);
        TargetLanguage = appSettings.PreferredLanguage;
        TargetFileType = appSettings.PreferredW3FileType;
    }

    /// <summary>
    ///     Event that is raised when the dialog requests to be closed
    /// </summary>
    public event EventHandler? RequestClose;

    /// <summary>
    ///     Gets the dialog result value
    ///     True if the save operation was successful, false if cancelled
    /// </summary>
    public bool? DialogResult { get; private set; }

    /// <summary>
    ///     Handles the save action
    ///     Serializes the W3 string items to a file with the specified settings
    /// </summary>
    [RelayCommand]
    private async Task Save()
    {
        Log.Information("Target filetype: {FileType}.", TargetFileType);
        Log.Information("Target language: {Language}.", TargetLanguage);
        var saveResult = await serializer.Serialize(w3StringItems, new W3SerializationContext
        {
            OutputDirectory = OutputDirectory,
            ExpectedIdSpace = IdSpace,
            TargetFileType = TargetFileType,
            TargetLanguage = TargetLanguage,
            IgnoreIdSpaceCheck = IsIgnoreIdSpaceCheck
        });
        Log.Information("Sve result: {Result}.", saveResult);
        WeakReferenceMessenger.Default.Send(new ValueChangedMessage<bool>(saveResult), MessageTokens.Save);
        DialogResult = true;
        RequestClose?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    ///     Handles the cancel action
    ///     Sets the dialog result to false and requests the dialog to close
    /// </summary>
    [RelayCommand]
    private void Cancel()
    {
        DialogResult = false;
        RequestClose?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    ///     Finds the ID space from a W3 string item's StrId
    /// </summary>
    /// <param name="iw3StringItem">The W3 string item to extract the ID space from</param>
    /// <returns>The ID space value, or -1 if not found</returns>
    private static int FindIdSpace(IW3StringItem iw3StringItem)
    {
        var match = IdSpaceRegex().Match(iw3StringItem.StrId);
        if (!match.Success) return -1;
        var foundIdSpace = match.Groups[1].Value;
        return int.Parse(foundIdSpace, CultureInfo.InvariantCulture);
    }

    /// <summary>
    ///     Regular expression to match and extract the ID space from a string ID
    /// </summary>
    /// <returns>A Regex object for matching ID space patterns</returns>
    [GeneratedRegex(@"^211(\d{4})\d{3}$")]
    private static partial Regex IdSpaceRegex();
}