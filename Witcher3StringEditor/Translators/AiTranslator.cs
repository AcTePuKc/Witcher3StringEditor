using AngleSharp;
using AngleSharp.Dom;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.DependencyInjection;
using GTranslate;
using GTranslate.Results;
using GTranslate.Translators;
using Microsoft.Extensions.Logging;
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
    private readonly IBrowsingContext browsingContext;
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
        browsingContext = BrowsingContext.New(Configuration.Default);
#pragma warning disable SKEXP0001 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
        if (modelSettings.ContextLength > 0)
            chatHistoryReducer = new ChatHistoryTruncationReducer(modelSettings.ContextLength);
#pragma warning restore SKEXP0001 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
        chatCompletionService = new OpenAIChatCompletionService(settings.ModelId,
                                                                apiKey: Unprotect(settings.ApiKey),
                                                                httpClient: httpClient,
                                                                loggerFactory: Ioc.Default.GetRequiredService<ILoggerFactory>());
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
        browsingContext.Dispose();
    }

    public string Name => "AiTranslator";

    public Task<ILanguage> DetectLanguageAsync(string text) => throw new NotImplementedException();

    public bool IsLanguageSupported(string language) => throw new NotImplementedException();

    public bool IsLanguageSupported(ILanguage language) => throw new NotImplementedException();

    public async Task<ITranslationResult> TranslateAsync(string text, string toLanguage, string? fromLanguage = null)
    {
        Guard.IsNotNullOrWhiteSpace(text);
        Guard.IsNotNullOrWhiteSpace(modelSettings.Prompts);
        var sourceLanguage = Language.GetLanguage(fromLanguage ?? "en");
        var targetLanguage = Language.GetLanguage(toLanguage);
        await PrepareChatHistory(targetLanguage);
        var (document, nodes) = await ProcessDocumentAndExtractNodes(text);
        var translationResponse = await GetTranslationResponse(nodes);
        UpdateNodeTextContent(nodes, translationResponse);
        var translation = BuildTranslationResult(text, document.Body?.InnerHtml ?? string.Empty, sourceLanguage, targetLanguage);
        document.Dispose();
        return translation;
    }

    public async Task<ITranslationResult> TranslateAsync(string text, ILanguage toLanguage, ILanguage? fromLanguage = null)
        => await TranslateAsync(text, toLanguage.Name, fromLanguage?.Name);

    public Task<ITransliterationResult> TransliterateAsync(string text, string toLanguage, string? fromLanguage = null)
        => throw new NotImplementedException();

    public Task<ITransliterationResult> TransliterateAsync(string text, ILanguage toLanguage, ILanguage? fromLanguage = null)
        => throw new NotImplementedException();

    private async Task PrepareChatHistory(Language targetLanguage)
    {
        if (destinationLanguage?.Equals(targetLanguage) != true)
        {
            chatHistory.Clear();
            destinationLanguage = targetLanguage;
        }
        if (chatHistory.Count == 0)
            chatHistory.AddSystemMessage(string.Format(modelSettings.Prompts, targetLanguage.ISO6393));
        if (modelSettings.ContextLength == 0 && chatHistory.Count > 1)
            chatHistory.RemoveRange(1, chatHistory.Count - 1);
        _ = await chatHistory.ReduceInPlaceAsync(chatHistoryReducer, CancellationToken.None);
    }

    private async Task<(IDocument document, IText[] nodes)> ProcessDocumentAndExtractNodes(string text)
    {
        var document = await browsingContext.OpenAsync(req => req.Content(text));
        var nodes = document.Body?.Descendants<IText>().ToArray() ?? throw new InvalidDataException("No text found.");
        return (document, nodes);
    }

    private async Task<string> GetTranslationResponse(IText[] nodes)
    {
        chatHistory.AddUserMessage(ExtractTextContent(nodes));
        var promptExecutionSettings = new OpenAIPromptExecutionSettings();
        if (modelSettings.Temperature >= 0)
            promptExecutionSettings.Temperature = modelSettings.Temperature;
        if (modelSettings.TopP >= 0)
            promptExecutionSettings.TopP = modelSettings.TopP;
        var translationResponse = (await chatCompletionService.GetChatMessageContentAsync(chatHistory, promptExecutionSettings)).ToString();
        Guard.IsNotNullOrWhiteSpace(translationResponse);
        chatHistory.AddAssistantMessage(translationResponse);
        return translationResponse;
    }

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

    private static void UpdateNodeTextContent(IText[] nodes, string translation)
    {
        var lines = nodes.Length > 1
            ? translation.Split(["\r\n", "\r", "\n"], StringSplitOptions.TrimEntries)
            : [translation];
        Guard.HasSizeEqualTo(lines, nodes.Length);
        for (var i = 0; i < nodes.Length; i++)
            nodes[i].TextContent = lines[i];
    }

    private static AiTranslationResult BuildTranslationResult(string source, string translation, Language formLanguage, Language toLanguage)
    {
        return new AiTranslationResult
        {
            Source = source,
            Translation = translation,
            SourceLanguage = formLanguage,
            TargetLanguage = toLanguage
        };
    }
}