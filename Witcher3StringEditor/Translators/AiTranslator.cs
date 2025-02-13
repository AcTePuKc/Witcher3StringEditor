using System.IO;
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
        if (string.IsNullOrEmpty(text)) throw new ArgumentException("Text cannot be null.");
        if (string.IsNullOrWhiteSpace(promots)) throw new InvalidOperationException("Prompts not configured.");
        var sourceLanguage = Language.GetLanguage(fromLanguage ?? "en");
        var targetLanguage = Language.GetLanguage(toLanguage);
        var context = BrowsingContext.New(Configuration.Default);
        var document = await context.OpenAsync(req => req.Content(text));
        var nodes = (document.Body?.Descendants<IText>().ToArray()) ?? throw new InvalidDataException("No text found.");
        var history = new ChatHistory();
        history.AddSystemMessage(string.Format(promots, toLanguage));
        history.AddUserMessage(ExtractTextContent(nodes));
        var translationResponse = (await chatCompletionService.GetChatMessageContentAsync(history, new OpenAIPromptExecutionSettings
        {
            Temperature = 1.3
        })).ToString();
        if (string.IsNullOrWhiteSpace(translationResponse))
            throw new InvalidDataException("Translation content cannot be null or empty.");
        var lines = nodes.Length > 1 ? translationResponse.Split(["\r\n", "\r", "\n"], StringSplitOptions.TrimEntries) : [translationResponse];
        if (lines.Length != nodes.Length)
            throw new InvalidOperationException($"The number of translated lines ({{lines.Length}}) does not match the number of original nodes ({{nodes.Length}}).");
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

    private static string ExtractTextContent(IText[] nodes)
    {
        string result;
        if (nodes.Length == 1)
        {
            result = nodes[0].Text;
        }
        else
        {
            var isFirst = true;
            var stringBuilder = new StringBuilder();
            foreach (var node in nodes)
            {
                if (!isFirst)
                    stringBuilder.AppendLine();
                stringBuilder.Append(node.Text);
                isFirst = false;
            }
            result = stringBuilder.ToString();
        }

        return result;
    }
}