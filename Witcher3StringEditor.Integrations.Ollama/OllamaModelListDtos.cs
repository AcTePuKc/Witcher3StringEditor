using System.Collections.Generic;

namespace Witcher3StringEditor.Integrations.Ollama;

public sealed record OllamaModelListRequest(string? BaseUrl);

public sealed record OllamaModelDescriptor(string Name, string? Digest, long? Size);

public sealed record OllamaModelListResponse(IReadOnlyList<OllamaModelDescriptor> Models);
