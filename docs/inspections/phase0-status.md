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

## Checklist Status (Phase 0)
- ✅ Database-backed translation memory scaffolding: interfaces + SQLite store + settings stubs are present and unused.
- ✅ Ollama model selection scaffolding: model DTOs + catalog stub are present and unused.
- ✅ Terminology/style loading scaffolding: models + loaders + prompt/validation stubs are present and unused.
- ✅ Translation profiles scaffolding: profile models + catalog/preview/selection stubs are present and unused.
- ✅ SafeFireAndForget placeholder added with diagnostics-only usage notes (no runtime wiring).

## Remaining Blockers
- None. Model ownership is confirmed in `docs/inspections/model-ownership.md`, with Common as the authoritative contract
  location for profiles, terminology, providers, and translation memory.

## Ownership Decision
- ✅ **Complete.** The authoritative location for translation-related contracts is confirmed in
  [`docs/inspections/model-ownership.md`](docs/inspections/model-ownership.md).

## Readiness Memo (Go/No-Go)
- **Decision: GO.** Phase 0 scaffolding is complete, ownership is confirmed, and there are no remaining blockers.
- **Checklist:** Re-verify against the scaffold checklist before any runtime wiring:
  [`docs/inspections/scaffold-progress.md`](docs/inspections/scaffold-progress.md).
- **Ownership reference:** Keep the model ownership map as the single source of truth:
  [`docs/inspections/model-ownership.md`](docs/inspections/model-ownership.md).
