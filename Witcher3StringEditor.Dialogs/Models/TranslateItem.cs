using CommunityToolkit.Mvvm.ComponentModel;

namespace Witcher3StringEditor.Dialogs.Models;

public partial class TranslateItem : ObservableObject
{
    [ObservableProperty] private bool isSaved;

    [ObservableProperty] private string translatedText = string.Empty;

    public required Guid Id { get; init; }

    public required string Text { get; init; }
}