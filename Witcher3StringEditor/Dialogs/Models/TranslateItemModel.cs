using CommunityToolkit.Mvvm.ComponentModel;

namespace Witcher3StringEditor.Dialogs.Models;

public partial class TranslateItemModel : ObservableObject
{

    public required Guid Id { get; init; }

    public required string Text { get; set; }

    [ObservableProperty]
    private string translatedText = string.Empty;
}