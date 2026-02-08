using Witcher3StringEditor.Common.Translation;

namespace Witcher3StringEditor.Services;

internal sealed class NoopTranslationPostProcessor : ITranslationPostProcessor
{
    public string Process(string input, TranslationContext ctx)
    {
        return input;
    }
}
