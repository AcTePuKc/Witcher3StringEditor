using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Witcher3StringEditor.Translators;

internal class SystemMessagePreservingReducer : IChatHistoryReducer
{
    private readonly int maxNonSystemMessages;
        
    public SystemMessagePreservingReducer(int maxNonSystemMessages)
    {
        if (maxNonSystemMessages < 0)
            throw new ArgumentException(@"The maximum number of preserved messages cannot be negative", nameof(maxNonSystemMessages));
        this.maxNonSystemMessages = maxNonSystemMessages;
    }

    Task<IEnumerable<ChatMessageContent>?> IChatHistoryReducer.ReduceAsync(IReadOnlyList<ChatMessageContent> chatHistory, CancellationToken cancellationToken)
    {
        var systemMessages = chatHistory
            .Where(m => m.Role == AuthorRole.System)
            .ToList();

        var nonSystemMessages = chatHistory
            .Where(m => m.Role != AuthorRole.System)
            .ToList();

        List<ChatMessageContent> truncatedNonSystemMessages;
        if (nonSystemMessages.Count <= maxNonSystemMessages)
        {
            truncatedNonSystemMessages = nonSystemMessages;
        }
        else
        {
            var skipCount = nonSystemMessages.Count - maxNonSystemMessages;
            truncatedNonSystemMessages = [.. nonSystemMessages.Skip(skipCount)];
        }

        var reducedHistory = new List<ChatMessageContent>();
        reducedHistory.AddRange(systemMessages);
        reducedHistory.AddRange(truncatedNonSystemMessages);

        return Task.FromResult<IEnumerable<ChatMessageContent>?>(reducedHistory);
    }
}