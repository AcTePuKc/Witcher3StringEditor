using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using Serilog;
using Witcher3StringEditor.Common.Abstractions;
using Witcher3StringEditor.Common.Terminology;
using Witcher3StringEditor.Locales;

namespace Witcher3StringEditor.Dialogs.ViewModels;

/// <summary>
///     ViewModel for the settings dialog window
///     Manages application settings including paths, translators, and supported cultures
///     Implements IModalDialogViewModel to support dialog result handling
/// </summary>
public partial class SettingsDialogViewModel : ObservableObject, IModalDialogViewModel
{
    private static readonly HttpClient HttpClient = new();

    private static readonly IReadOnlyList<string> DefaultModelOptions = [];

    private static readonly IReadOnlyList<string> DefaultProviderOptions =
    [
        "Ollama",
        "Custom (stub)"
    ];

    private readonly IDialogService dialogService;
    private readonly ITerminologyLoader terminologyLoader;

    /// <summary>
    ///     Initializes a new instance of the SettingsDialogViewModel class
    /// </summary>
    /// <param name="appSettings">Application settings service</param>
    /// <param name="dialogService">Dialog service for showing file dialogs</param>
    /// <param name="translators">Collection of available translators</param>
    /// <param name="supportedCultures">Collection of supported cultures for localization</param>
    /// <param name="terminologyLoader">Terminology loader for preview validation</param>
    public SettingsDialogViewModel(
        IAppSettings appSettings,
        IDialogService dialogService,
        IEnumerable<string> translators,
        IEnumerable<CultureInfo> supportedCultures,
        ITerminologyLoader terminologyLoader)
    {
        AppSettings = appSettings;
        this.dialogService = dialogService;
        Translators = translators;
        SupportedCultures = supportedCultures;
        this.terminologyLoader = terminologyLoader;
        ModelOptions = new ObservableCollection<string>(InitializeModelOptions(appSettings));

        if (appSettings is INotifyPropertyChanged notifyPropertyChanged)
        {
            notifyPropertyChanged.PropertyChanged += OnAppSettingsPropertyChanged;
        }

        _ = UpdateTerminologyStatusAsync();
        _ = UpdateStyleGuideStatusAsync();
    }

    /// <summary>
    ///     Gets the application settings service
    /// </summary>
    public IAppSettings AppSettings { get; }

    /// <summary>
    ///     Gets the collection of available translators
    /// </summary>
    public IEnumerable<string> Translators { get; }

    /// <summary>
    ///     Gets the collection of supported cultures for localization
    /// </summary>
    public IEnumerable<CultureInfo> SupportedCultures { get; }

    /// <summary>
    ///     Gets the list of available translation providers (stubbed)
    /// </summary>
    public IReadOnlyList<string> ProviderOptions { get; } = DefaultProviderOptions;

    /// <summary>
    ///     Gets the list of available translation models (stubbed)
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<string> modelOptions = [];

    /// <summary>
    ///     Gets the model refresh status text
    /// </summary>
    [ObservableProperty] private string modelStatusText = string.Empty;

    /// <summary>
    ///     Gets the terminology load status text
    /// </summary>
    [ObservableProperty] private string terminologyStatusText = string.Empty;

    /// <summary>
    ///     Gets the style guide load status text
    /// </summary>
    [ObservableProperty] private string styleGuideStatusText = string.Empty;

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
    ///     Sets the path to the terminology file
    ///     Opens a file dialog to allow the user to select a .tsv or .csv file
    /// </summary>
    [RelayCommand]
    private async Task SetTerminologyFilePath()
    {
        var dialogSettings = new OpenFileDialogSettings
        {
            Filters =
            [
                new FileFilter(Strings.FileFormatTerminology, [".tsv", ".csv"])
            ],
            Title = Strings.SelectTerminologyFile
        };

        using var storageFile = await dialogService.ShowOpenFileDialogAsync(this, dialogSettings);
        if (storageFile is not null)
        {
            AppSettings.TerminologyFilePath = storageFile.LocalPath;
            Log.Information("Terminology file path set to {Path}.", storageFile.LocalPath);
        }
    }

    /// <summary>
    ///     Sets the path to the style guide file
    ///     Opens a file dialog to allow the user to select a .md file
    /// </summary>
    [RelayCommand]
    private async Task SetStyleGuideFilePath()
    {
        var dialogSettings = new OpenFileDialogSettings
        {
            Filters =
            [
                new FileFilter(Strings.FileFormatStyleGuide, ".md")
            ],
            Title = Strings.SelectStyleGuideFile
        };

        using var storageFile = await dialogService.ShowOpenFileDialogAsync(this, dialogSettings);
        if (storageFile is not null)
        {
            AppSettings.StyleGuideFilePath = storageFile.LocalPath;
            Log.Information("Style guide file path set to {Path}.", storageFile.LocalPath);
        }
    }

    /// <summary>
    ///     Refreshes the available model list from the Ollama API.
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
            var baseUri = new Uri(baseUrl, UriKind.Absolute);
            var requestUri = new Uri(baseUri, "api/tags");
            using var response = await HttpClient.GetAsync(requestUri);
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

            EnsureSelectedModelOption();
            CacheModelOptions();
        }
        catch (UriFormatException)
        {
            ModelStatusText = $"Invalid Ollama base URL: {baseUrl}";
        }
        catch (HttpRequestException)
        {
            ModelStatusText = $"Could not connect to Ollama at {baseUrl}";
        }
        catch (TaskCanceledException)
        {
            ModelStatusText = $"Could not connect to Ollama at {baseUrl}";
        }
        catch (JsonException)
        {
            ModelStatusText = $"Received an unexpected response from Ollama at {baseUrl}";
        }
    }

    private static IEnumerable<string> InitializeModelOptions(IAppSettings appSettings)
    {
        var options = new HashSet<string>(DefaultModelOptions, StringComparer.OrdinalIgnoreCase);
        var cachedOptions = appSettings.CachedTranslationModels;

        if (cachedOptions is { Count: > 0 })
        {
            foreach (var modelName in cachedOptions)
            {
                if (!string.IsNullOrWhiteSpace(modelName))
                {
                    options.Add(modelName);
                }
            }
        }

        var selectedModel = appSettings.TranslationModelName;
        if (!string.IsNullOrWhiteSpace(selectedModel))
        {
            options.Add(selectedModel);
        }

        return options;
    }

    private void EnsureSelectedModelOption()
    {
        var selectedModel = AppSettings.TranslationModelName;
        if (!string.IsNullOrWhiteSpace(selectedModel) &&
            !ModelOptions.Contains(selectedModel, StringComparer.OrdinalIgnoreCase))
        {
            ModelOptions.Add(selectedModel);
        }
    }

    private void CacheModelOptions()
    {
        AppSettings.CachedTranslationModels =
            new ObservableCollection<string>(ModelOptions.Distinct(StringComparer.OrdinalIgnoreCase));
    }

    private void OnAppSettingsPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IAppSettings.UseTerminologyPack) ||
            e.PropertyName == nameof(IAppSettings.TerminologyFilePath))
        {
            _ = UpdateTerminologyStatusAsync();
        }

        if (e.PropertyName == nameof(IAppSettings.UseStyleGuide) ||
            e.PropertyName == nameof(IAppSettings.StyleGuideFilePath))
        {
            _ = UpdateStyleGuideStatusAsync();
        }
    }

    private async Task UpdateTerminologyStatusAsync()
    {
        if (!AppSettings.UseTerminologyPack)
        {
            TerminologyStatusText = "Terminology disabled.";
            return;
        }

        if (string.IsNullOrWhiteSpace(AppSettings.TerminologyFilePath))
        {
            TerminologyStatusText = "No terminology file selected.";
            return;
        }

        try
        {
            await terminologyLoader.LoadAsync(AppSettings.TerminologyFilePath);
            TerminologyStatusText = "Terminology loaded.";
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to load terminology pack from {Path}.", AppSettings.TerminologyFilePath);
            TerminologyStatusText = "Failed to load terminology.";
        }
    }

    private async Task UpdateStyleGuideStatusAsync()
    {
        if (!AppSettings.UseStyleGuide)
        {
            StyleGuideStatusText = "Style guide disabled.";
            return;
        }

        if (string.IsNullOrWhiteSpace(AppSettings.StyleGuideFilePath))
        {
            StyleGuideStatusText = "No style guide selected.";
            return;
        }

        try
        {
            await terminologyLoader.LoadAsync(AppSettings.StyleGuideFilePath);
            StyleGuideStatusText = "Style guide loaded.";
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to load style guide from {Path}.", AppSettings.StyleGuideFilePath);
            StyleGuideStatusText = "Failed to load style guide.";
        }
    }
}
