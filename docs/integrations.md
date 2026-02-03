# Integrations Architecture (Stub Plan)

## Goals
- Provide safe, local-only integration points for translation memory, provider/model selection, terminology/style packs, and translation profiles.
- Keep all storage local (JSON or SQLite) and leave integrations inert until enabled.

## Proposed Architecture

### Settings Surface
- Extend `IAppSettings` and `AppSettings` with provider/model/base URL/profile id settings.
- Keep existing translator selection intact to avoid behavior changes.
- UI exposes provider/model/base URL as placeholders in the settings dialog.

### Provider Abstraction
- Continue using the existing `ITranslationProvider` and provider registry in `Witcher3StringEditor.Common.Translation`.
- Translation provider selection should resolve via a future registry (mapping provider name to `ITranslationProvider`).

### Translation Memory (Local Only)
- Introduce a local translation memory store interface in `Witcher3StringEditor.Common.TranslationMemory`.
- Planned storage: SQLite with a minimal schema for source/target/language/time.

### Terminology & Style Packs
- Introduce a terminology loader interface in `Witcher3StringEditor.Common.Terminology`.
- Planned loaders: TSV/CSV/Markdown, with a shared `TerminologyPack` model.

### Translation Profiles
- Add a profile model and store interface in `Witcher3StringEditor.Common.Profiles`.
- Profiles bind provider/model/base URL/notes into a reusable configuration.

## Planned Tasks (High Level)
1. Inventory pass to locate settings persistence, translation entrypoints, and UI integration points.
2. Implement SQLite-backed translation memory store + schema (local only).
3. Wire provider registry to settings and model discovery (Ollama first).
4. Implement terminology/style pack loading + validation hooks.
5. Implement translation profile storage (JSON first, SQLite later).

## Notes
- All integrations should be inert by default.
- New services should be registered behind feature flags or no-op implementations until ready.
