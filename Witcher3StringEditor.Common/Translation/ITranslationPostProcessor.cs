namespace Witcher3StringEditor.Common.Translation;

public interface ITranslationPostProcessor
{
    string Process(string input, TranslationContext ctx);
}
