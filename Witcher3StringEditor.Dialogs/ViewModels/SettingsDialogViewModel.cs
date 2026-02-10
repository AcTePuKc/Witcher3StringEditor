using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using Serilog;
using Witcher3StringEditor.Common.Abstractions;
using Witcher3StringEditor.Common.Profiles;
using Witcher3StringEditor.Common.Terminology;
using Witcher3StringEditor.Common.Translation;
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
    private readonly ITranslationProfileCatalog profileCatalog;
    private readonly ITranslationProfileSettingsResolver profileSettingsResolver;
    private readonly ITerminologyLoader terminologyLoader;
    private readonly IStyleGuideLoader styleGuideLoader;
    private readonly ITerminologyValidationService terminologyValidationService;
    private readonly ITranslationProviderHealthCheck providerHealthCheck;
    /// <summary>
    ///     Initializes a new instance of the SettingsDialogViewModel class
    /// </summary>
    /// <param name="appSettings">Application settings service</param>
    /// <param name="dialogService">Dialog service for showing file dialogs</param>
    /// <param name="translators">Collection of available translators</param>
    /// <param name="supportedCultures">Collection of supported cultures for localization</param>
    public SettingsDialogViewModel(
        IAppSettings appSettings,
        IDialogService dialogService,
        ITranslationProfileCatalog profileCatalog,
        ITranslationProfileSettingsResolver profileSettingsResolver,
        ITerminologyLoader terminologyLoader,
        IStyleGuideLoader styleGuideLoader,
        ITerminologyValidationService terminologyValidationService,
        ITranslationProviderHealthCheck providerHealthCheck,
        IEnumerable<string> translators,
        IEnumerable<CultureInfo> supportedCultures)
    {
        AppSettings = appSettings;
        this.dialogService = dialogService;
        this.profileCatalog = profileCatalog;
        this.profileSettingsResolver = profileSettingsResolver;
        this.terminologyLoader = terminologyLoader;
        this.styleGuideLoader = styleGuideLoader;
        this.terminologyValidationService = terminologyValidationService;
        this.providerHealthCheck = providerHealthCheck;
        Translators = translators;
        SupportedCultures = supportedCultures;
        if (appSettings is INotifyPropertyChanged notifyPropertyChanged)
        {
            notifyPropertyChanged.PropertyChanged += OnAppSettingsPropertyChanged;
        }

        ModelOptions = new ObservableCollection<string>(DefaultModelOptions);
        ModelStatusText = "Not loaded yet.";
        ProfileStatusText = "Not loaded yet.";
        SelectedProfilePreview = "Not loaded yet.";
        TerminologyStatusText = "Not loaded yet.";
        StyleGuideStatusText = "Not loaded yet.";
        ProviderConnectionStatusText = "Not loaded yet.";
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
    ///     Gets the list of available translation profiles (stubbed)
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<TranslationProfileSummary> translationProfiles = [];

    /// <summary>
    ///     Gets the model refresh status text
    /// </summary>
    [ObservableProperty] private string modelStatusText = string.Empty;

    /// <summary>
    ///     Gets the translation profile status text
    /// </summary>
    [ObservableProperty] private string profileStatusText = string.Empty;

    /// <summary>
    ///     Gets the translation profile preview text
    /// </summary>
    [ObservableProperty] private string selectedProfilePreview = string.Empty;

    /// <summary>
    ///     Gets the terminology load status text
    /// </summary>
    [ObservableProperty] private string terminologyStatusText = string.Empty;

    /// <summary>
    ///     Gets the style guide load status text
    /// </summary>
    [ObservableProperty] private string styleGuideStatusText = string.Empty;

    /// <summary>
    ///     Gets the provider connection status text
    /// </summary>
    [ObservableProperty] private string providerConnectionStatusText = string.Empty;

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

    [RelayCommand]
    private async Task RefreshTranslationProfiles()
    {
        await LoadTranslationProfilesAsync();
        await UpdateSelectedProfilePreviewAsync();
    }

    [RelayCommand]
    private async Task RefreshTerminologyStatus()
    {
        await UpdateTerminologyStatusAsync();
    }

    [RelayCommand]
    private async Task RefreshStyleGuideStatus()
    {
        await UpdateStyleGuideStatusAsync();
    }

    /// <summary>
    ///     Loads cached model options from local settings without performing network requests.
    /// </summary>
    [RelayCommand]
    private void LoadCachedModels()
    {
        try
        {
            ModelOptions = new ObservableCollection<string>(InitializeModelOptions(AppSettings));
            ModelStatusText = ModelOptions.Count == 0
                ? "No cached model entries found."
                : "Loaded model options from local cache.";
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Settings model cache load failed.");
            ModelOptions = new ObservableCollection<string>(DefaultModelOptions);
            ModelStatusText = "Model cache could not be loaded from settings.";
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
        var refreshedModels = new List<string>();

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
                        refreshedModels.Add(modelName);
                    }
                }
            }

            ModelOptions.Clear();
            foreach (var modelName in refreshedModels.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                ModelOptions.Add(modelName);
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

    /// <summary>
    ///     Tests the selected provider connection by listing available models.
    /// </summary>
    [RelayCommand]
    private async Task TestProviderConnection()
    {
        ProviderConnectionStatusText = string.Empty;
        var providerName = AppSettings.TranslationProviderName;
        if (string.IsNullOrWhiteSpace(providerName))
        {
            ProviderConnectionStatusText = "Select a provider before testing the connection.";
            return;
        }

        try
        {
            var result = await providerHealthCheck.CheckAsync(providerName);
            ProviderConnectionStatusText = result.Message;
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Provider connection test failed for {ProviderName}.", providerName);
            ProviderConnectionStatusText = "Connection test failed.";
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

    private async Task LoadTranslationProfilesAsync()
    {
        try
        {
            var profiles = await profileCatalog.ListSummariesAsync();
            var orderedProfiles = profiles
                .OrderBy(profile => profile.DisplayName, StringComparer.OrdinalIgnoreCase)
                .ToList();

            TranslationProfiles = new ObservableCollection<TranslationProfileSummary>(orderedProfiles);
            ProfileStatusText = TranslationProfiles.Count == 0
                ? "No translation profiles available yet."
                : "Translation profile routing is not enabled yet.";
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to load translation profile summaries.");
            TranslationProfiles = [];
            ProfileStatusText = "Failed to load translation profiles.";
        }
    }

    private void OnAppSettingsPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IAppSettings.TranslationProviderName))
        {
            ProviderConnectionStatusText = string.Empty;
        }
    }

    private async Task UpdateSelectedProfilePreviewAsync()
    {
        var profileId = AppSettings.TranslationProfileId;
        if (string.IsNullOrWhiteSpace(profileId))
        {
            SelectedProfilePreview = "Select a translation profile to preview its settings.";
            return;
        }

        try
        {
            var previewProfile = await profileSettingsResolver.ResolveAsync(AppSettings);
            SelectedProfilePreview = previewProfile is null
                ? "Translation profile details are not available yet."
                : BuildProfilePreview(previewProfile);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to load translation profile {ProfileId}.", profileId);
            SelectedProfilePreview = "Failed to load translation profile details.";
        }
    }

    private static string BuildProfilePreview(TranslationProfile profile)
    {
        var builder = new StringBuilder();

        AppendPreviewLine(builder, "Name", profile.Name);
        AppendPreviewLine(builder, "Provider", profile.ProviderName);
        AppendPreviewLine(builder, "Model", profile.ModelName);
        AppendPreviewLine(builder, "Base URL", profile.BaseUrl);
        AppendPreviewLine(builder, "Terminology path",
            profile.TerminologyFilePath ?? profile.TerminologyPath);
        AppendPreviewLine(builder, "Style guide path",
            profile.StyleGuideFilePath ?? profile.StyleGuidePath);

        if (profile.UseTerminologyPack.HasValue)
        {
            AppendPreviewLine(builder, "Terminology",
                profile.UseTerminologyPack.Value ? "Enabled" : "Disabled");
        }

        if (profile.UseStyleGuide.HasValue)
        {
            AppendPreviewLine(builder, "Style guide",
                profile.UseStyleGuide.Value ? "Enabled" : "Disabled");
        }

        if (profile.UseTranslationMemory.HasValue)
        {
            AppendPreviewLine(builder, "Translation memory",
                profile.UseTranslationMemory.Value ? "Enabled" : "Disabled");
        }

        AppendPreviewLine(builder, "Notes", profile.Notes);

        if (builder.Length == 0)
        {
            return "Translation profile does not specify any settings.";
        }

        return builder.ToString().TrimEnd();
    }

    private static void AppendPreviewLine(StringBuilder builder, string label, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        builder.Append(label);
        builder.Append(": ");
        builder.AppendLine(value.Trim());
    }

    private async Task UpdateTerminologyStatusAsync()
    {
        try
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

            if (!File.Exists(AppSettings.TerminologyFilePath))
            {
                TerminologyStatusText = "Terminology file not found.";
                return;
            }

            await terminologyLoader.LoadAsync(AppSettings.TerminologyFilePath);
            var validationResult =
                await terminologyValidationService.ValidateTerminologyAsync(AppSettings.TerminologyFilePath);
            TerminologyStatusText = BuildValidationStatus("Terminology loaded successfully.", validationResult);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to load terminology pack from {Path}.", AppSettings.TerminologyFilePath);
            TerminologyStatusText = "Failed to load terminology file.";
        }
    }

    private async Task UpdateStyleGuideStatusAsync()
    {
        try
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

            if (!File.Exists(AppSettings.StyleGuideFilePath))
            {
                StyleGuideStatusText = "Style guide file not found.";
                return;
            }

            await styleGuideLoader.LoadStyleGuideAsync(AppSettings.StyleGuideFilePath);
            var validationResult =
                await terminologyValidationService.ValidateStyleGuideAsync(AppSettings.StyleGuideFilePath);
            StyleGuideStatusText = BuildValidationStatus("Style guide loaded successfully.", validationResult);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to load style guide from {Path}.", AppSettings.StyleGuideFilePath);
            StyleGuideStatusText = "Failed to load style guide file.";
        }
    }

    private static string BuildValidationStatus(string status, TerminologyValidationResult validationResult)
    {
        if (string.IsNullOrWhiteSpace(validationResult.Message))
        {
            return status;
        }

        return $"{status} Validation: {validationResult.Message}";
    }
}
