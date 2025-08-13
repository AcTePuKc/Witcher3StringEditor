using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using AngleSharp;
using AngleSharp.Dom;
using CommunityToolkit.Diagnostics;
using GTranslate;
using GTranslate.Results;
using GTranslate.Translators;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Serilog;
using Witcher3StringEditor.Interfaces;

namespace Witcher3StringEditor.Translators;

internal sealed class AiTranslator : ITranslator, IDisposable
{
    private readonly IBrowsingContext browsingContext;
    private readonly ChatHistory chatHistory;
    private readonly IChatHistoryReducer? chatHistoryReducer;
    private readonly HttpClient httpClient;
    private readonly Kernel kernel;
    private readonly IModelSettings modelSettings;
    private readonly PromptExecutionSettings promptExecutionSettings;
    private bool disposedValue;
    private ILanguage? selectedLanguage;

    public AiTranslator(IModelSettings settings)
    {
        chatHistory = [];
        modelSettings = settings;
        httpClient = new HttpClient
        {
            BaseAddress = new Uri(settings.EndPoint)
        };
        browsingContext = BrowsingContext.New(Configuration.Default);
        promptExecutionSettings = CreatePromptExecutionSettings();
        if (modelSettings.ContextLength > 0)
            chatHistoryReducer = new ChatHistoryTruncationReducer(modelSettings.ContextLength);
        kernel = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion(settings.ModelId, new Uri(settings.EndPoint), Unprotect(settings.ApiKey)).Build();
        Log.Information("AiTranslator initialized");
        Log.Information("EndPoint: {EndPoint}", settings.EndPoint);
        Log.Information("ModelId: {ModelId}", settings.ModelId);
        Log.Information("Prompts: {Prompts}", settings.Prompts);
        Log.Information("Temperature: {Temperature}", settings.Temperature);
        Log.Information("TopP: {TopP}", settings.TopP);
        Log.Information("ContextLength: {ContextLength}", settings.ContextLength);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public string Name => "AiTranslator";

    public Task<ILanguage> DetectLanguageAsync(string text)
    {
        throw new NotImplementedException();
    }

    public bool IsLanguageSupported(string language)
    {
        throw new NotImplementedException();
    }

    public bool IsLanguageSupported(ILanguage language)
    {
        throw new NotImplementedException();
    }

    public async Task<ITranslationResult> TranslateAsync(string text, string toLanguage, string? fromLanguage = null)
    {
        Guard.IsTrue(Language.TryGetLanguage(fromLanguage ?? "en", out var sourceLanguage));
        Guard.IsTrue(Language.TryGetLanguage(toLanguage, out var targetLanguage));
        return await TranslateAsync(text, targetLanguage, sourceLanguage);
    }

    public async Task<ITranslationResult> TranslateAsync(string text, ILanguage toLanguage,
        ILanguage? fromLanguage = null)
    {
        Guard.IsNotNullOrWhiteSpace(text);
        await PrepareChatHistory(toLanguage);
        var (document, nodes) = await ProcessDocumentAndExtractNodes(text);
        Guard.IsNotNull(nodes);
        Guard.IsNotEmpty(nodes);
        var translationResponse = await FetchTranslationResponse(nodes);
        Guard.IsNotNullOrWhiteSpace(translationResponse);
        UpdateNodeTextContent(nodes, translationResponse);
        var translation = document.Body?.InnerHtml;
        Guard.IsNotNullOrWhiteSpace(translation);
        var translationResult = new AiTranslationResult
        {
            Source = text,
            Translation = translation,
            SourceLanguage = fromLanguage ?? Language.GetLanguage("en"),
            TargetLanguage = toLanguage
        };
        document.Dispose();
        return translationResult;
    }

    public Task<ITransliterationResult> TransliterateAsync(string text, string toLanguage, string? fromLanguage = null)
    {
        throw new NotImplementedException();
    }

    public Task<ITransliterationResult> TransliterateAsync(string text, ILanguage toLanguage,
        ILanguage? fromLanguage = null)
    {
        throw new NotImplementedException();
    }

    private OpenAIPromptExecutionSettings CreatePromptExecutionSettings()
    {
        var defaultPromptSettings = new OpenAIPromptExecutionSettings();
        if (modelSettings.Temperature > 0)
            defaultPromptSettings.Temperature = modelSettings.Temperature;
        if (modelSettings.TopP > 0)
            defaultPromptSettings.TopP = modelSettings.TopP;
        return defaultPromptSettings;
    }

    private static string Unprotect(string encryptedKey)
    {
        var encryptedData = Convert.FromBase64String(encryptedKey);
        var data = ProtectedData.Unprotect(encryptedData, null, DataProtectionScope.CurrentUser);
        return Encoding.UTF8.GetString(data);
    }

    private async Task PrepareChatHistory(ILanguage toLanguage)
    {
        if (selectedLanguage?.Equals(toLanguage) != true)
        {
            chatHistory.Clear();
            selectedLanguage = toLanguage;
        }

        if (chatHistory.Count == 0)
            chatHistory.AddSystemMessage(string.Format(modelSettings.Prompts, toLanguage.Name));
        if (modelSettings.ContextLength == 0 && chatHistory.Count > 1)
            chatHistory.RemoveRange(1, chatHistory.Count - 1);
        _ = await chatHistory.ReduceInPlaceAsync(chatHistoryReducer, CancellationToken.None);
    }

    private async Task<(IDocument document, IText[]? nodes)> ProcessDocumentAndExtractNodes(string text)
    {
        var document = await browsingContext.OpenAsync(req => req.Content(text));
        var nodes = document.Body?.Descendants<IText>().ToArray();
        return (document, nodes);
    }

    private async Task<string> FetchTranslationResponse(IText[] nodes)
    {
        chatHistory.AddUserMessage(ExtractTextContent(nodes));
        var translation = (await kernel.GetRequiredService<IChatCompletionService>()
                .GetChatMessageContentAsync(chatHistory, promptExecutionSettings))
            .ToString();
        chatHistory.AddAssistantMessage(translation);
        return translation;
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
        for (var i = 0; i < nodes.Length; i++)
            nodes[i].TextContent = lines[i];
    }

    private void Dispose(bool disposing)
    {
        if (disposedValue) return;
        if (disposing)
        {
            httpClient.Dispose();
            browsingContext.Dispose();
        }

        disposedValue = true;
        Log.Information("AiTranslator disposed");
    }

    ~AiTranslator()
    {
        Dispose(false);
    }
}