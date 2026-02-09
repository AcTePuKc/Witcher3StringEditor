using System.Collections.Generic;

namespace Witcher3StringEditor.Common.TranslationMemory;

public sealed record TranslationMemoryMigrationStep(int Version, string Description);

public interface ITranslationMemoryMigrationPlanner
{
    IReadOnlyList<TranslationMemoryMigrationStep> GetPlan(TranslationMemorySettings settings);
}
