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

        var providerError = result.Errors.FirstOrDefault(IsProviderRelatedError);

        return providerError?.Message;
    }


    public static TranslationProviderFailureDto? GetProviderFailure(this IResultBase result)
    {
        if (result is null)
        {
            return null;
        }

        var providerError = result.Errors.FirstOrDefault(IsProviderRelatedError);
        if (providerError is null)
        {
            return null;
        }

        var providerName = providerError.Metadata.TryGetValue(TranslationFailureMetadata.ProviderNameKey, out var providerNameValue)
            ? providerNameValue?.ToString()
            : null;
        var providerFailureKind = providerError.Metadata.TryGetValue(TranslationFailureMetadata.ProviderFailureReasonKey, out var providerFailureReasonValue)
            && providerFailureReasonValue is TranslationProviderFailureKind providerFailureReason
            ? providerFailureReason
            : TranslationProviderFailureKind.Unknown;

        return new TranslationProviderFailureDto(
            string.IsNullOrWhiteSpace(providerName) ? string.Empty : providerName,
            providerFailureKind,
            providerError.Message);
    }

    private static bool IsProviderRelatedError(IError error)
    {
        if (error is null)
        {
            return false;
        }

        if (!error.Metadata.TryGetValue(TranslationFailureMetadata.FailureKindKey, out var failureKindValue))
        {
            return false;
        }

        var failureKind = failureKindValue?.ToString();
        return string.Equals(failureKind, TranslationFailureMetadata.ProviderFailureKind, StringComparison.Ordinal)
               || string.Equals(failureKind, TranslationFailureMetadata.RequestValidationFailureKind, StringComparison.Ordinal);
    }

    public static string? GetStatusMessage(this IResultBase result)
    {
        if (result is null)
        {
            return null;
        }

        var statusReason = result.Reasons
            .FirstOrDefault(reason =>
                reason.Metadata.TryGetValue(TranslationStatusMetadata.StatusMessageKey, out _));

        return statusReason?.Metadata[TranslationStatusMetadata.StatusMessageKey]?.ToString();
    }
}
