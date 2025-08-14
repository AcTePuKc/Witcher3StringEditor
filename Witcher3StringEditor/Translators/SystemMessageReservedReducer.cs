using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Witcher3StringEditor.Translators;

internal class SystemMessageReservedReducer : IChatHistoryReducer
{
    private readonly int maxNonSystemMessages;
        
    public SystemMessageReservedReducer(int maxNonSystemMessages)
    {
        if (maxNonSystemMessages < 0)
            throw new ArgumentException(@"The maximum number of preserved messages cannot be negative", nameof(maxNonSystemMessages));
        this.maxNonSystemMessages = maxNonSystemMessages;
    }

    Task<IEnumerable<ChatMessageContent>?> IChatHistoryReducer.ReduceAsync(IReadOnlyList<ChatMessageContent> chatHistory, CancellationToken cancellationToken)
    {
        var nonSystemMessages = chatHistory
            .Where(m => m.Role != AuthorRole.System)
            .ToList();
        
        if (nonSystemMessages.Count <= maxNonSystemMessages)
        {
            return Task.FromResult<IEnumerable<ChatMessageContent>?>(chatHistory);
        }

        var skipCount = nonSystemMessages.Count - maxNonSystemMessages;
        var reducedHistory = new List<ChatMessageContent>();
        reducedHistory.AddRange([.. chatHistory.Where(m => m.Role == AuthorRole.System)]);
        reducedHistory.AddRange([.. nonSystemMessages.Skip(skipCount)]);
        return Task.FromResult<IEnumerable<ChatMessageContent>?>(reducedHistory);
    }
}