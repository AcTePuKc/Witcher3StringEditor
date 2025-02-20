using AngleSharp;
using AngleSharp.Dom;
using GTranslate;
using GTranslate.Results;
using GTranslate.Translators;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using Witcher3StringEditor.Interfaces;

namespace Witcher3StringEditor.Translators;

internal class AiTranslator : ITranslator
{
    private Language? destinationLanguage;
    private readonly HttpClient httpClient;
    private readonly ChatHistory chatHistory;
    private readonly IModelSettings modelSettings;
    private readonly IChatCompletionService chatCompletionService;
#pragma warning disable SKEXP0001 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
    private readonly ChatHistoryTruncationReducer? chatHistoryReducer;
#pragma warning restore SKEXP0001 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。

    public AiTranslator(IModelSettings settings)
    {
        chatHistory = [];
        modelSettings = settings;
        httpClient = new HttpClient
        {
            BaseAddress = new Uri(settings.EndPoint)
        };
#pragma warning disable SKEXP0001 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
        if (modelSettings.ContextLength > 0)
            chatHistoryReducer = new ChatHistoryTruncationReducer(modelSettings.ContextLength);
#pragma warning restore SKEXP0001 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
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
        if (string.IsNullOrWhiteSpace(modelSettings.Prompts)) throw new InvalidOperationException("Prompts not configured.");
        var sourceLanguage = Language.GetLanguage(fromLanguage ?? "en");
        var targetLanguage = Language.GetLanguage(toLanguage);
        if (destinationLanguage?.Equals(targetLanguage) != true)
        {
            chatHistory.Clear();
            destinationLanguage = targetLanguage;
        }
        var context = BrowsingContext.New(Configuration.Default);
        var document = await context.OpenAsync(req => req.Content(text));
        var nodes = document.Body?.Descendants<IText>().ToArray() ?? throw new InvalidDataException("No text found.");
        if (chatHistory.Count == 0)
            chatHistory.AddSystemMessage(string.Format(modelSettings.Prompts, toLanguage));
        if (modelSettings.ContextLength == 0 && chatHistory.Count > 1)
            chatHistory.RemoveRange(1, chatHistory.Count - 1);
        _ = await chatHistory.ReduceInPlaceAsync(chatHistoryReducer, CancellationToken.None);
        chatHistory.AddUserMessage(ExtractTextContent(nodes));
        var promptExecutionSettings = new OpenAIPromptExecutionSettings();
        if (modelSettings.Temperature >= 0)
            promptExecutionSettings.Temperature = modelSettings.Temperature;
        if (modelSettings.TopP >= 0)
            promptExecutionSettings.TopP = modelSettings.TopP;
        var translationResponse = (await chatCompletionService.GetChatMessageContentAsync(chatHistory, promptExecutionSettings)).ToString();
        if (string.IsNullOrWhiteSpace(translationResponse))
            throw new InvalidDataException("Translation content cannot be null or empty.");
        chatHistory.AddAssistantMessage(translationResponse);
        var lines = nodes.Length > 1 ? translationResponse.Split(["\r\n", "\r", "\n"], StringSplitOptions.TrimEntries) : [translationResponse];
        if (lines.Length != nodes.Length)
            throw new InvalidOperationException($"The number of translated lines ({lines.Length}) does not match the number of original nodes ({nodes.Length}).");
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