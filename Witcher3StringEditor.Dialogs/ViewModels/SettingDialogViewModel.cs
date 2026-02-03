using System.Collections.ObjectModel;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using Serilog;
using Witcher3StringEditor.Common.Abstractions;
using Witcher3StringEditor.Locales;

namespace Witcher3StringEditor.Dialogs.ViewModels;

/// <summary>
///     ViewModel for the settings dialog window
///     Manages application settings including paths, translators, and supported cultures
///     Implements IModalDialogViewModel to support dialog result handling
/// </summary>
/// <param name="appSettings">Application settings service</param>
/// <param name="dialogService">Dialog service for showing file dialogs</param>
/// <param name="translators">Collection of available translators</param>
/// <param name="supportedCultures">Collection of supported cultures for localization</param>
public partial class SettingDialogViewModel(
    IAppSettings appSettings,
    IDialogService dialogService,
    IEnumerable<string> translators,
    IEnumerable<CultureInfo> supportedCultures)
    : ObservableObject, IModalDialogViewModel
{
    private static readonly IReadOnlyList<string> DefaultModelOptions = [];

    private static readonly IReadOnlyList<string> DefaultProviderOptions =
    [
        "Ollama",
        "Custom (stub)"
    ];

    /// <summary>
    ///     Gets the application settings service
    /// </summary>
    public IAppSettings AppSettings { get; } = appSettings;

    /// <summary>
    ///     Gets the collection of available translators
    /// </summary>
    public IEnumerable<string> Translators { get; } = translators;

    /// <summary>
    ///     Gets the collection of supported cultures for localization
    /// </summary>
    public IEnumerable<CultureInfo> SupportedCultures { get; } = supportedCultures;

    /// <summary>
    ///     Gets the list of available translation providers (stubbed)
    /// </summary>
    public IReadOnlyList<string> ProviderOptions { get; } = DefaultProviderOptions;

    /// <summary>
    ///     Gets the list of available translation models (stubbed)
    /// </summary>
    [ObservableProperty] private ObservableCollection<string> modelOptions = new(DefaultModelOptions);

    /// <summary>
    ///     Gets the model refresh status text
    /// </summary>
    [ObservableProperty] private string modelStatusText = string.Empty;

    /// <summary>
    ///     Gets the dialog result value
    ///     Returns true to indicate that the dialog was closed successfully
    /// </summary>
    public bool? DialogResult => true;

    /// <summary>
    ///     Sets the path to the w3strings.exe file
    ///     Opens a file dialog to allow the user to select the w3strings.exe file
    /// </summary>
    [RelayCommand]
    private async Task SetW3StringsPath()
    {
        // Open a file dialog to allow the user to select the w3strings.exe file.
        var dialogSettings = new OpenFileDialogSettings
        {
            Filters = [new FileFilter("w3strings.exe", ".exe")], // Set the file filter.
            Title = Strings.SelectW3Strings, // Set the dialog title.
            SuggestedFileName = "w3strings" // Set the suggested file name.
        };
        using var storageFile =
            await dialogService.ShowOpenFileDialogAsync(this, dialogSettings); // Open the file dialog.
        if (storageFile is
            { Name: "w3strings.exe" }) // If the selected file is w3strings.exe, set the path to the file.
        {
            AppSettings.W3StringsPath = storageFile.LocalPath; // Set the path to the file.
            Log.Information("Encoder path set to {Path}.", storageFile.LocalPath); // Log the path.
        }
    }

    /// <summary>
    ///     Sets the path to the witcher3.exe file
    ///     Opens a file dialog to allow the user to select the witcher3.exe file
    /// </summary>
    [RelayCommand]
    private async Task SetGameExePath()
    {
        // Open a file dialog to allow the user to select the witcher3.exe file.
        var dialogSettings = new OpenFileDialogSettings
        {
            Filters = [new FileFilter("witcher3.exe", ".exe")], // Set the file filter.
            Title = Strings.SelectGameExe, // Set the dialog title.
            SuggestedFileName = "witcher3" // Set the suggested file name.
        };
        using var storageFile =
            await dialogService.ShowOpenFileDialogAsync(this, dialogSettings); // Open the file dialog.
        if (storageFile is { Name: "witcher3.exe" }) // If the selected file is witcher3.exe, set the path to the file.
        {
            AppSettings.GameExePath = storageFile.LocalPath; // Set the path to the file.
            Log.Information("Game path set to {Path}.", storageFile.LocalPath); // Log the path.
        }
    }

    /// <summary>
    ///     Refreshes the available model list (stubbed)
    /// </summary>
    [RelayCommand]
    private async Task RefreshModels()
    {
        var baseUrl = string.IsNullOrWhiteSpace(AppSettings.BaseUrl)
            ? "http://localhost:11434"
            : AppSettings.BaseUrl;

        ModelStatusText = string.Empty;
        ModelOptions.Clear();

        try
        {
            using var httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
            using var response = await httpClient.GetAsync("api/tags");
            if (!response.IsSuccessStatusCode)
            {
                ModelStatusText = $"Could not connect to Ollama at {baseUrl}";
                return;
            }

            await using var responseStream = await response.Content.ReadAsStreamAsync();
            using var jsonDocument = await JsonDocument.ParseAsync(responseStream);

            if (!jsonDocument.RootElement.TryGetProperty("models", out var modelsElement) ||
                modelsElement.ValueKind != JsonValueKind.Array)
            {
                return;
            }

            foreach (var modelElement in modelsElement.EnumerateArray())
            {
                if (modelElement.TryGetProperty("name", out var nameElement) &&
                    nameElement.ValueKind == JsonValueKind.String)
                {
                    var modelName = nameElement.GetString();
                    if (!string.IsNullOrWhiteSpace(modelName))
                    {
                        ModelOptions.Add(modelName);
                    }
                }
            }
        }
        catch (Exception)
        {
            ModelStatusText = $"Could not connect to Ollama at {baseUrl}";
        }
    }
}
