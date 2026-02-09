# Integrations Architecture & Plan

## Goal
Prepare **local-only**, compile-safe scaffolding for:
- Database-backed translation memory (SQLite/JSON).
- Ollama integration with model selection.
- Terminology and style guide loading.
- Translation profiles.

## Architecture overview (stub-first)
- **Contracts live in `Witcher3StringEditor.Common`**
  - Providers, model catalogs, terminology loaders, profile stores, and translation memory interfaces.
- **Local storage lives in `Witcher3StringEditor.Data`**
  - SQLite/JSON implementations and database bootstrap logic.
- **Integration stubs live in `Witcher3StringEditor.Integrations.*`**
  - Ollama provider + model catalog, terminology loaders, profile store stubs.
- **Runtime wiring lives in `Witcher3StringEditor/Services`**
  - No-op implementations remain the default until explicitly wired.

**Constraints**
- No external services; storage is local only.
- No large refactors.
- No behavior changes unless explicitly flagged as placeholders.

## Planned tasks (issue drafts)
See `docs/integration-issues.md` for full issue drafts and QA checklists. The current focus is:
1. **Translation memory (local DB) scaffolding**
2. **Ollama model selection scaffolding**
3. **Terminology/style loading scaffolding**
4. **Translation profile scaffolding**

## Integration handoff notes
- **Settings**: `App.xaml.cs` and `Models/AppSettings.cs` are the persistence entry points.
- **Routing**: `Services/TranslationRouter.cs` and `Services/TranslationPipelineContextBuilder.cs` remain the safe
  injection points for future wiring.
- **UI**: Settings dialog and translation dialog placeholders should remain minimal and inert by default.
