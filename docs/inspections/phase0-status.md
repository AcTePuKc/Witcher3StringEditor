# Phase 0 Status (Scaffolding & Inventory)

## Scope
This status report tracks minimal scaffolding for future integrations: database-backed translation memory, Ollama model selection, terminology/style loading, and translation profiles. The focus is on compile-safe interfaces/DTOs only.

## Stubs/DTOs/Interfaces Added (Unused in Runtime)
The following items were added as scaffolding only and are **unused in runtime** (no DI registrations, no call sites, and no UI wiring):

- `Witcher3StringEditor.Common/TranslationMemory/ITranslationMemoryMigrationPlanner.cs`
  - `ITranslationMemoryMigrationPlanner` interface and `TranslationMemoryMigrationStep` DTO.
- `Witcher3StringEditor.Integrations.Ollama/OllamaModelListDtos.cs`
  - `OllamaModelListRequest`, `OllamaModelDescriptor`, and `OllamaModelListResponse` DTOs.
- `Witcher3StringEditor.Common/Terminology/TerminologySourceDescriptor.cs`
  - `TerminologySourceType` enum and `TerminologySourceDescriptor` DTO.
- `Witcher3StringEditor.Common/Profiles/ITranslationProfileValidator.cs`
  - `ITranslationProfileValidator` interface and `TranslationProfileValidationResult` DTO.

## Notes
- These additions are strictly scaffolding. Runtime behavior remains unchanged.
- Follow-up tasks should define concrete implementations and then wire them into DI, UI settings, and translation flows as approved.
