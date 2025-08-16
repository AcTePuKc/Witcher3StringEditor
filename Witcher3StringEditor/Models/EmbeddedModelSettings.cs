using CommunityToolkit.Mvvm.ComponentModel;
using Witcher3StringEditor.Interfaces;

namespace Witcher3StringEditor.Models
{
    internal partial class EmbeddedModelSettings : ObservableObject, IEmbeddedModelSettings
    {
        [ObservableProperty] private string apiKey = string.Empty;

        [ObservableProperty] private string endPoint = string.Empty;

        [ObservableProperty] private string modelId = string.Empty;

        [ObservableProperty] private int dimensions = 1536;
    }
}
