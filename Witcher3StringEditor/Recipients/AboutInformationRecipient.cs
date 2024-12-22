using CommunityToolkit.Mvvm.Messaging;

namespace Witcher3StringEditor.Recipients;

internal class AboutInformationRecipient : IRecipient<AboutInformationMessage>
{
    public void Receive(AboutInformationMessage aboutInformation)
    {
    }
}