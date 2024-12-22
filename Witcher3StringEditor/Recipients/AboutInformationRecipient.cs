using CommunityToolkit.Mvvm.Messaging;

namespace Witcher3StringEditor.Recipients;

internal class AboutInformationRecipient : IRecipient<AboutInformationMessage>
{
    public string? Message { get; private set; }

    public void Receive(AboutInformationMessage aboutInformation)
        => Message = aboutInformation.Message;
}