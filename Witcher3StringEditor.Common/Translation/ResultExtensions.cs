using System;
using System.Linq;
using FluentResults;

namespace Witcher3StringEditor.Common.Translation;

public static class ResultExtensions
{
    public static IError? GetProviderError(this IResultBase result)
    {
        if (result is null)
        {
            return null;
        }

        return result.Errors.FirstOrDefault(error =>
            error.Metadata.TryGetValue(TranslationFailureMetadata.FailureKindKey, out var kind) &&
            string.Equals(kind?.ToString(), TranslationFailureMetadata.ProviderFailureKind,
                StringComparison.Ordinal));
    }
}
