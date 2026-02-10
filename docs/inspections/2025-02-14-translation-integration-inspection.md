# Translation Integration Scaffolding Inspection

## Scope
- Reviewed the existing scaffolding for database-backed translation memory, Ollama provider/model selection, terminology +
  style loading, and translation profiles.
- Focused on local-only storage and compile-safe stubs without altering runtime behavior.
- Out of scope: full feature wiring, UI workflows, and runtime behavior changes.

## Findings
- Translation provider routing already supports opt-in providers via `TranslationRouter` and falls back to legacy
  translation when a provider is missing or invalid.
- Translation memory has a local SQLite implementation in the data layer, but the app currently defaults to no-op
  services and does not wire initialization by default.
- Ollama integration includes a provider stub and model catalog stub; models are returned only when a model name is set.
- Terminology and style guide loaders already support local CSV/TSV/Markdown parsing stubs with no runtime wiring.
- Translation profile models and JSON storage exist, but profile selection and application remain no-op by default.
- There is duplication between `Witcher3StringEditor.Common/*` and `Witcher3StringEditor/Integrations/*` namespaces for
  terminology and profiles; future work should confirm the long-term source of truth before wiring.

## Files
- `Witcher3StringEditor/App.xaml.cs`
- `Witcher3StringEditor/Models/AppSettings.cs`
- `Witcher3StringEditor/Services/TranslationRouter.cs`
- `Witcher3StringEditor/Services/TranslationPipelineContextBuilder.cs`
- `Witcher3StringEditor.Common/TranslationMemory/*`
- `Witcher3StringEditor.Data/TranslationMemory/*`
- `Witcher3StringEditor.Integrations.Ollama/*`
- `Witcher3StringEditor/Services/TerminologyLoader.cs`
- `Witcher3StringEditor.Common/Profiles/*`
- `Witcher3StringEditor.Data/Profiles/*`

## Next steps
- Use the issue drafts in `docs/archive/integration-issues.md` to drive the next scaffolding pass (Issue A-D cover translation
  memory, Ollama model selection, terminology/style loading, and translation profiles).
- Confirm whether `Common` or `Integrations` namespaces should be the canonical home for terminology and profile models
  before wiring to settings or UI.
- Keep new work compile-safe and inert by default (no runtime wiring until a dedicated wiring issue is approved).
