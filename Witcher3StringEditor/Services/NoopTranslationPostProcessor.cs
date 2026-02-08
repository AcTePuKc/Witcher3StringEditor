using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Witcher3StringEditor.Common.Translation;

namespace Witcher3StringEditor.Services;

internal sealed class NoopTranslationPostProcessor : ITranslationPostProcessor
{
    public string Name => "No-op";

    public Task<TranslationPostProcessingResult> ProcessAsync(
        TranslationPostProcessingRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = new TranslationPostProcessingResult(
            request.Text,
            AppliedRules: Array.Empty<string>(),
            Metadata: new Dictionary<string, string>
            {
                ["todo"] = "Post-processing rules are not implemented yet."
            });

        return Task.FromResult(result);
    }
}
