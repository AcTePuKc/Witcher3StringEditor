using GTranslate;
using GTranslate.Results;
using GTranslate.Translators;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using Witcher3StringEditor.Interfaces;

namespace Witcher3StringEditor.Translators
{
    internal class AiTranslator : ITranslator
    {
        private readonly string promots;
        private readonly HttpClient httpClient;
        private readonly IChatCompletionService chatCompletionService;

        public AiTranslator(IModelSettings settings)
        {
            promots = settings.Prompts;
            httpClient = new HttpClient()
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
            var history = new ChatHistory();
            history.AddSystemMessage(string.Format(promots, toLanguage));
            history.AddUserMessage(text);
            return new AiTranslationResult()
            {
                Source = text,
                Translation = (await chatCompletionService.GetChatMessageContentAsync(history, new OpenAIPromptExecutionSettings
                {
                    Temperature = 1.3
                })).ToString(),
                SourceLanguage = Language.GetLanguage(fromLanguage ?? "en"),
                TargetLanguage = Language.GetLanguage(toLanguage)
            };
        }

        public async Task<ITranslationResult> TranslateAsync(string text, ILanguage toLanguage, ILanguage? fromLanguage = null)
            => await TranslateAsync(text, toLanguage.Name, fromLanguage?.Name);

        public Task<ITransliterationResult> TransliterateAsync(string text, string toLanguage, string? fromLanguage = null)
            => throw new NotImplementedException();

        public Task<ITransliterationResult> TransliterateAsync(string text, ILanguage toLanguage, ILanguage? fromLanguage = null)
            => throw new NotImplementedException();
    }
}