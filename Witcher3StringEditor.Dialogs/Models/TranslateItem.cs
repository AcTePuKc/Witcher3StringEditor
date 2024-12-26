using CommunityToolkit.Mvvm.ComponentModel;

namespace Witcher3StringEditor.Dialogs.Models;

public partial class TranslateItem : ObservableObject
{

    public required string Id { get; init; }

    public required string Text { get; set; }

    [ObservableProperty]
    private string translatedText = string.Empty;
}