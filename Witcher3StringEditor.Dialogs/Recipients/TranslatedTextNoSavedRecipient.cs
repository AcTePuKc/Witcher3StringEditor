using CommunityToolkit.Mvvm.Messaging;

namespace Witcher3StringEditor.Dialogs.Recipients;

internal class TranslatedTextNoSavedRecipient : IRecipient<TranslatedTextNoSavedMessage>
{
    public void Receive(TranslatedTextNoSavedMessage message)
    {
    }
}