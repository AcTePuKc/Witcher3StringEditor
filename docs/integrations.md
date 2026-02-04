# Integrations Architecture (Short Spec)

## Goals
- Keep all new integrations **local-only** (SQLite or JSON).
- Add safe, inert scaffolding for future features without changing current workflows.
- Prefer small interfaces and TODO hooks over real implementations.

## Architecture Overview
### Translation Providers + Model Selection
- **Interfaces** live in `Witcher3StringEditor.Common/Translation/`.
- **Registry** (future) resolves provider names to `ITranslationProvider` instances.
- **Ollama stub** lives in `Witcher3StringEditor.Integrations.Ollama/` with settings + model listing placeholder.
- **Settings bridge** lives in `IAppSettings` (`TranslationProviderName`, `TranslationModelName`, `TranslationBaseUrl`, `CachedTranslationModels`).

### Provider Selection Behavior
- If a provider is selected **and** provider routing is implemented, the provider wins.
- Otherwise, the app falls back to the legacy translator selection.

### Translation Memory (Database-Backed)
- **Interfaces/models** live in `Witcher3StringEditor.Common/TranslationMemory/`.
- **Settings stub** lives in `Witcher3StringEditor.Common/TranslationMemory/TranslationMemorySettings.cs`.
- **SQLite storage** lives in `Witcher3StringEditor.Data/TranslationMemory/`.
- Data is stored under AppData via `Witcher3StringEditor.Data/Storage/` helpers.

### Terminology + Style Packs
- **Models + loader interface** live in `Witcher3StringEditor.Common/Terminology/`.
- **Loader implementation** lives in `Witcher3StringEditor/Services/TerminologyLoader.cs`.
- **Sample fixtures** live under `docs/samples/` for TSV/CSV and Markdown style guides.

### Translation Profiles
- **Profile models** live in `Witcher3StringEditor.Common/Profiles/`.
- **Profile store** lives in `Witcher3StringEditor.Data/Profiles/` (JSON-backed, AppData).
- **Resolver stub** lives in `Witcher3StringEditor/Services/` to merge profiles with settings later.

## Wiring Map (Current → Future)
- **Settings dialog** should remain the single source of truth for provider/model/profile selection.
- **Main translation flow** (`MainWindowViewModel`) should eventually call the provider registry when
  `TranslationProviderName` is configured; otherwise it should keep using the existing translator list.
- **Translation memory** should be queried right before provider/translator execution and saved after a
  successful result. (TODO: hook into the translation command path.)
- **Terminology/style** should be loaded on demand (path in settings or profile) and injected into the
  provider request metadata, with validation hooks after translation. (TODO: inject + validate.)

## Planned Tasks (Issue Breakdown Summary)
1. Inventory pass to confirm settings persistence, translation entry points, and UI hooks.
2. Wire provider registry + model discovery (Ollama first) with no behavior changes.
3. Keep translation memory + QA stores inert until explicitly enabled.
4. Extend terminology/style loading with prompt injection + validation hooks (TODOs only).
5. Add profile selection wiring (store + resolver) without altering existing translator flow.
6. Add minimal settings stubs for translation memory enablement + local database path.

## Constraints
- No external services.
- No UI redesigns; only minimal placeholders in settings if needed.
- Every change must compile and remain inert by default.
