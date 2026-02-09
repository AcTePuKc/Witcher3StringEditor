# Integrations Architecture & Plan

## Goal
Prepare **local-only**, compile-safe scaffolding for:
- Database-backed translation memory (SQLite/JSON).
- Ollama integration with model selection.
- Terminology and style guide loading.
- Translation profiles.

## Architecture overview (stub-first)
- **Contracts live in `Witcher3StringEditor.Common`**
  - Providers, model catalogs, terminology loaders, profile stores, translation memory interfaces, and request DTOs
    for glossary/style/profile references.
  - In-memory provider registry stub (`InMemoryTranslationProviderRegistry`) is available for future wiring.
- **Local storage lives in `Witcher3StringEditor.Data`**
  - SQLite/JSON implementations and database bootstrap logic.
- **Integration stubs live in `Witcher3StringEditor.Integrations.*`**
  - Ollama provider + model catalog, terminology loaders, profile store stubs.
- **Runtime wiring lives in `Witcher3StringEditor/Services`**
  - No-op implementations remain the default until explicitly wired.
  - Factory stubs (for example, translation memory store creation) live here to keep wiring inert.
- **Pipeline context lives in `Witcher3StringEditor.Common/Translation`**
  - `TranslationPipelineContext` captures provider/model/profile identifiers and terminology paths for future routing.
  - Context data is a read-only snapshot that should remain unused in runtime flows until explicitly wired.

**Constraints**
- No external services; storage is local only.
- No large refactors.
- No behavior changes unless explicitly flagged as placeholders.

## Phase 0: Compile-Safe Scaffolding
Phase 0 focuses on **compile-safe scaffolding only** (interfaces, stubs, TODO markers) with zero runtime behavior
changes. It exists to prepare the codebase for future integrations without altering routing or UI behavior.

**Do**
- Add compile-safe interfaces, DTOs, and stub implementations.
- Keep all new integrations inert by default (no runtime wiring).
- Document follow-up wiring steps explicitly and defer them to later issues.

**Don’t**
- Do not change translation routing behavior.
- Do not change UI behavior (placeholders only, no workflow changes).
- Do not introduce external services or network dependencies (local-only storage).

For detailed implementation notes, see `docs/implementation.md`.

## Planned tasks (issue drafts)
See `docs/integration-issues.md` for full issue drafts and QA checklists. The current focus is:
1. **Provider contracts + registry stubs (interfaces + DTOs)**
2. **Translation memory (local DB) scaffolding**
3. **Ollama model selection scaffolding**
4. **Terminology/style loading scaffolding**
5. **Translation profile scaffolding**
6. **Local loader stubs for profiles and translation memory**
7. **Sample TSV/MD assets for terminology/style testing**
8. **Settings placeholders for provider/model/profile selection**

## Integration handoff notes
- **Settings**: `App.xaml.cs` and `Models/AppSettings.cs` are the persistence entry points.
- **Routing**: `Services/TranslationRouter.cs` and `Services/TranslationPipelineContextBuilder.cs` remain the safe
  injection points for future wiring.
- **UI**: Settings dialog and translation dialog placeholders should remain minimal and inert by default.
- **Samples**: test assets live under `docs/samples/` (TSV terminology packs, Markdown style guides).
