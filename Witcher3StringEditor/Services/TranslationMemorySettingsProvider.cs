using Witcher3StringEditor.Common.Abstractions;
using Witcher3StringEditor.Common.TranslationMemory;

namespace Witcher3StringEditor.Services;

/// <summary>
///     Provides translation memory settings derived from application settings.
/// </summary>
internal sealed class TranslationMemorySettingsProvider(IAppSettings appSettings)
    : ITranslationMemorySettingsProvider
{
    /// <inheritdoc />
    public TranslationMemorySettings GetSettings()
    {
        return new TranslationMemorySettings
        {
            IsEnabled = appSettings.UseTranslationMemory,
            DatabasePath = string.IsNullOrWhiteSpace(appSettings.TranslationMemoryPath)
                ? null
                : appSettings.TranslationMemoryPath,
            ProviderName = "LocalSqlite"
        };
    }
}
