using System;
using System.Collections.Generic;

namespace Witcher3StringEditor.Integrations.Tasks;

public sealed record TranslationTaskGroup(
    string Id,
    string DisplayName,
    string? ProfileId,
    IReadOnlyList<string> EntryIds,
    DateTimeOffset CreatedAt,
    string? Notes = null);
