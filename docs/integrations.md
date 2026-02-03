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
- Add TODO hooks in the translation flow for prompt injection and post-translation validation.

### Translation Memory (Local Only)
- Introduce a local translation memory store interface in `Witcher3StringEditor.Common.TranslationMemory`.
- Planned storage: SQLite with a minimal schema for source/target/language/time.
- Storage lives in `Witcher3StringEditor.Data` with AppData-based file paths.

### QA Metadata (Local Only)
- Define an `IQAStore` interface in `Witcher3StringEditor.Common.QualityAssurance`.
- Store QA issue entries in SQLite with minimal fields (source/target/issue type/created).
- Keep lookups exact-match only and upserts minimal.

### Terminology & Style Packs
- Define `TerminologyPack` entries with term, translation, notes, and mode.
- `TerminologyLoader` parses CSV/TSV terminology files and Markdown style guides with simple rules.
- Markdown parsing produces a `StyleGuide` model and maps required/forbidden terms into a terminology pack.
- Sample files in `docs/samples/` serve as validation fixtures for loaders.
- `TerminologyLoader.ValidateSamplesAsync` loads the sample TSV/Markdown files for quick validation.

### Translation Profiles
- Add a profile model and store interface in `Witcher3StringEditor.Common.Profiles`.
- Profiles bind provider/model/base URL/notes into a reusable configuration.

## Planned Tasks (High Level)
1. Inventory pass to locate settings persistence, translation entrypoints, and UI integration points.
2. Implement SQLite-backed translation memory + QA stores with AppData storage.
3. Wire provider registry to settings and model discovery (Ollama first).
4. Implement terminology/style pack loading + validation hooks.
5. Implement translation profile storage (JSON first, SQLite later).

## Notes
- All integrations should be inert by default.
- New services should be registered behind feature flags or no-op implementations until ready.
