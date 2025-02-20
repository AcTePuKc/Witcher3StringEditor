using CommunityToolkit.Mvvm.ComponentModel;
using Witcher3StringEditor.Interfaces;

namespace Witcher3StringEditor.Models;

public partial class ModelSettings : ObservableObject, IModelSettings
{
    [ObservableProperty]
    private string endPoint = string.Empty;

    [ObservableProperty]
    private string modelId = string.Empty;

    [ObservableProperty]
    private string apiKey = string.Empty;

    [ObservableProperty]
    private string prompts = string.Empty;

    [ObservableProperty]
    private int contextLength;

    [ObservableProperty]
    private double temperature = -1;

    [ObservableProperty]
    private double topP = -1;
}