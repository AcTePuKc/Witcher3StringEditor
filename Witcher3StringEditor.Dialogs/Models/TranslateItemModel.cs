using CommunityToolkit.Mvvm.ComponentModel;

namespace Witcher3StringEditor.Dialogs.Models;

public partial class TranslateItemModel : ObservableObject
{
    [ObservableProperty] private bool _isSaved;

    [ObservableProperty] private string _translatedText = string.Empty;

    public required Guid Id { get; init; }

    public required string Text { get; init; }
}