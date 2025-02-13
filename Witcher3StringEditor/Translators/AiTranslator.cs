using AngleSharp;
using AngleSharp.Dom;
using GTranslate;
using GTranslate.Results;
using GTranslate.Translators;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using Witcher3StringEditor.Interfaces;

namespace Witcher3StringEditor.Translators;

internal class AiTranslator : ITranslator
{
    private readonly string promots;
    private readonly HttpClient httpClient;
    private readonly IChatCompletionService chatCompletionService;

    public AiTranslator(IModelSettings settings)
    {
        promots = settings.Prompts;
        httpClient = new HttpClient
        {
            BaseAddress = new Uri(settings.EndPoint)
        };
        chatCompletionService = new OpenAIChatCompletionService(settings.ModelId, apiKey: Unprotect(settings.ApiKey), httpClient: httpClient);
    }

    private static string Unprotect(string encryptedKey)
    {
        var encryptedData = Convert.FromBase64String(encryptedKey);
        var data = ProtectedData.Unprotect(encryptedData, null, DataProtectionScope.CurrentUser);
        return Encoding.UTF8.GetString(data);
    }

    ~AiTranslator()
    {
        httpClient.Dispose();
    }

    public string Name => "AiTranslator";

    public Task<ILanguage> DetectLanguageAsync(string text) => throw new NotImplementedException();

    public bool IsLanguageSupported(string language) => throw new NotImplementedException();

    public bool IsLanguageSupported(ILanguage language) => throw new NotImplementedException();

    public async Task<ITranslationResult> TranslateAsync(string text, string toLanguage, string? fromLanguage = null)
    {
        var sourceLanguage = Language.GetLanguage(fromLanguage ?? "en");
        var targetLanguage = Language.GetLanguage(toLanguage);
        var config = Configuration.Default;
        var context = BrowsingContext.New(config);
        var document = await context.OpenAsync(req => req.Content(text));
        var nodes = document.Body?.Descendants<IText>().ToArray();
        if (nodes == null)
            return new AiTranslationResult
            {
                Source = text,
                Translation = string.Empty,
                SourceLanguage = sourceLanguage,
                TargetLanguage = targetLanguage
            };
        var stringBuilder = new StringBuilder();
        foreach (var node in nodes)
            stringBuilder.AppendLine(node.Text);
        var history = new ChatHistory();
        history.AddSystemMessage(string.Format(promots, toLanguage));
        history.AddUserMessage(stringBuilder.ToString());
        var translationResponse = (await chatCompletionService.GetChatMessageContentAsync(history, new OpenAIPromptExecutionSettings
        {
            Temperature = 1.3
        })).ToString();
        var lines = translationResponse.Split(["\r\n", "\r", "\n"], StringSplitOptions.TrimEntries);
        if (lines.Length != nodes.Length)
            return new AiTranslationResult
            {
                Source = text,
                Translation = string.Empty,
                SourceLanguage = sourceLanguage,
                TargetLanguage = targetLanguage
            };
        for (var i = 0; i < nodes.Length; i++)
            nodes[i].TextContent = lines[i];
        return new AiTranslationResult
        {
            Source = text,
            Translation = document.Body?.InnerHtml ?? string.Empty,
            SourceLanguage = sourceLanguage,
            TargetLanguage = targetLanguage
        };
    }

    public async Task<ITranslationResult> TranslateAsync(string text, ILanguage toLanguage, ILanguage? fromLanguage = null)
        => await TranslateAsync(text, toLanguage.Name, fromLanguage?.Name);

    public Task<ITransliterationResult> TransliterateAsync(string text, string toLanguage, string? fromLanguage = null)
        => throw new NotImplementedException();

    public Task<ITransliterationResult> TransliterateAsync(string text, ILanguage toLanguage, ILanguage? fromLanguage = null)
        => throw new NotImplementedException();
}