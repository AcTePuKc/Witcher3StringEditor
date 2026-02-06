using System;
using System.Linq;
using FluentResults;

namespace Witcher3StringEditor.Common.Translation;

public static class ResultExtensions
{
    public static bool IsFailure(this IResultBase result)
    {
        return result is not null && result.IsFailed;
    }

    public static string? GetProviderError(this IResultBase result)
    {
        if (result is null)
        {
            return null;
        }

        var providerError = result.Errors.FirstOrDefault(error =>
            error.Metadata.TryGetValue(TranslationFailureMetadata.FailureKindKey, out var kind) &&
            string.Equals(kind?.ToString(), TranslationFailureMetadata.ProviderFailureKind,
                StringComparison.Ordinal));

        return providerError?.Message;
    }
}
