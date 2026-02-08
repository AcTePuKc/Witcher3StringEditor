using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Witcher3StringEditor.Common.Translation;

public interface ITranslationProviderRegistry
{
    IReadOnlyList<TranslationProviderDescriptor> GetProviders();

    ITranslationProvider? Resolve(string providerName);

    bool TryGet(string providerName, out ITranslationProvider provider);
}

public interface ITranslationProviderHealthCheck
{
    Task<TranslationProviderHealthCheckResult> CheckAsync(
        string providerName,
        CancellationToken cancellationToken = default);
}

public sealed record TranslationProviderHealthCheckResult(
    bool IsSuccess,
    string Message,
    int? ModelCount = null);

public sealed class TranslationProviderDescriptor
{
    public string Name { get; init; } = string.Empty;

    public string? DisplayName { get; init; }

    public bool SupportsModelListing { get; init; }

    public string? Notes { get; init; }
}
