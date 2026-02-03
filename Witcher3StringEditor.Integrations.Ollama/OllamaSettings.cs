using System;

namespace Witcher3StringEditor.Integrations.Ollama;

public sealed class OllamaSettings
{
    public string BaseUrl { get; init; } = "http://localhost:11434";

    public string ModelName { get; init; } = "";

    public double Temperature { get; init; } = 0.2;

    public int NumCtx { get; init; } = 2048;

    public TimeSpan? Timeout { get; init; }
}
