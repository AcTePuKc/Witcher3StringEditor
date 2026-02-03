# Integrations Architecture (Stub Plan)

## Goals
- Provide safe, local-only integration points for translation memory, provider/model selection, terminology/style packs, and translation profiles.
- Keep all storage local (JSON or SQLite) and leave integrations inert until enabled.

## Current Scaffolding (Already in Repo)
- Provider abstraction and registry: `Witcher3StringEditor.Common/Translation/`.
- Ollama provider stub and settings model: `Witcher3StringEditor.Integrations.Ollama/`.
- Translation memory + QA store interfaces: `Witcher3StringEditor.Common/TranslationMemory/` and `Witcher3StringEditor.Common/QualityAssurance/`.
- SQLite-backed store stubs: `Witcher3StringEditor.Data/TranslationMemory/` and `Witcher3StringEditor.Data/QualityAssurance/`.
- Terminology models + loader interface: `Witcher3StringEditor.Common/Terminology/`.
- Translation profile model + store interface: `Witcher3StringEditor.Common/Profiles/`.

## Proposed Architecture

### Settings Surface
- Extend `IAppSettings` and `AppSettings` with provider/model/base URL/profile id settings.
- Treat `BaseUrl` as an alias for the persisted translation base URL so Ollama endpoints can use it directly.
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

## Testing Strategy (Initial Integrations)
### Current Test Infrastructure
- No test projects are present in the solution or repo tree.
- No xUnit/NUnit/MSTest package references are present.

### Decision
Use **manual QA checklists** (e.g., as part of pull request templates or a dedicated wiki page) for the initial integration scaffolding. The goal of this phase is to keep changes inert and compile-safe without introducing a test project until the integration boundaries stabilize.

### First Automated Test Targets (Follow-Up)
When we introduce xUnit stubs later, start with small, deterministic targets:
1. `TerminologyLoader` parsing for CSV/TSV/Markdown samples (fixture-driven, no UI).
2. `JsonTranslationProfileStore` behavior when the profiles file is missing/empty.
3. `SqliteTranslationMemoryStore` and `SqliteQAStore` init + basic upsert/lookup using a temp file.
4. Ollama provider settings validation (pure model tests, no HTTP).

## Notes
- All integrations should be inert by default.
- New services should be registered behind feature flags or no-op implementations until ready.

## Configuration Storage Review
### Current State
- `ConfigService` serializes settings as JSON via Newtonsoft and reads/writes a single config file path provided at startup.【F:Witcher3StringEditor/Services/ConfigService.cs†L1-L38】
- `AppSettings` is the primary persisted settings model, with observable properties for settings like translator choice, provider/model/base URL fields, and the optional translation profile id.【F:Witcher3StringEditor/Models/AppSettings.cs†L1-L122】
- The settings file lives under `%AppData%/Witcher3StringEditor(_Debug)/AppSettings.Json`, built in `App.xaml.cs` and injected into `ConfigService` at startup.【F:Witcher3StringEditor/App.xaml.cs†L120-L177】【F:Witcher3StringEditor/App.xaml.cs†L267-L279】

### Option A: Extend the Existing JSON Config (`AppSettings.Json`)
**Pros**
- Single file to back up and migrate.
- No new storage path logic or file lifecycle concerns.
- Fits the current `ConfigService` serialization flow.

**Cons**
- Profiles become bloated inside `AppSettings` as arrays grow (harder to manage/merge).
- Sharing profiles across machines becomes coupled to user-specific settings.
- Harder to version or validate profile-specific data separately.

### Option B: Add a Dedicated Profiles File (e.g., `translation-profiles.json`)
**Pros**
- Profiles can be versioned/validated independently from base app settings.
- Easier to share/export/import a single profiles file.
- Keeps `AppSettings` focused on per-user defaults and last-used state.

**Cons**
- Additional file to manage and migrate.
- Requires a separate loader/store and path conventions.

### Decision
Use a **dedicated profiles file** for translation profiles and keep `AppSettings` limited to the active profile id plus current provider/model/base URL defaults. This preserves the existing settings flow while enabling profile management without inflating the base config file.

### Next Steps
1. Add a local `ITranslationProfileStore` implementation that reads `translation-profiles.json` from AppData.
2. Keep `AppSettings.TranslationProfileId` as the selected profile pointer.
3. Add UI placeholders to select a profile (no behavior changes).
4. Add import/export tasks for profiles as a future enhancement.

## Issue Breakdown (GitHub-Style)
### Issue 1: Inventory pass for settings + translation entry points
**Description**
Document config storage, translation provider resolution, and UI entry points for new integrations.

**Acceptance Criteria**
- `docs/integrations.md` lists config storage details and translation entry points.
- Risks/constraints are captured with file references.

**Files to Touch**
- `docs/integrations.md`

**QA Checklist**
- Build succeeds.
- Settings dialog opens and translator selection is unchanged.

### Issue 2: Local JSON-backed translation profiles store (stub)
**Description**
Add a JSON-backed implementation of `ITranslationProfileStore` that reads profiles from AppData.

**Acceptance Criteria**
- New store compiles and returns empty list if the file is missing.
- File path follows AppData conventions.
- No UI wiring or behavior changes yet.

**Files to Touch**
- `Witcher3StringEditor.Data/Profiles/JsonTranslationProfileStore.cs`
- `docs/integrations.md`

**QA Checklist**
- Build succeeds.
- App starts and settings persist as before.

### Issue 3: Ollama integration scaffolding + model list stub
**Description**
Create settings and provider stubs for Ollama with model selection placeholders.

**Acceptance Criteria**
- Ollama settings model (BaseUrl, Model) exists.
- Provider stub compiles and exposes a `ListModelsAsync` placeholder.
- Settings model refresh resolves `BaseUrl` (defaulting to `http://localhost:11434`) and populates model names from `/api/tags`.
- Error states show a friendly status message and keep the model list empty.

**Files to Touch**
- `Witcher3StringEditor.Integrations.Ollama/...`
- `Witcher3StringEditor.Common/Translation/...` (if registry updates are needed)
- `docs/integrations.md`

**QA Checklist**
- Build succeeds.
- Translation dialog remains functional with existing providers.

### Issue 4: Terminology & style pack loading (local-only stubs)
**Description**
Add loaders for CSV/TSV/Markdown terminology and style guides, plus sample fixtures.

**Acceptance Criteria**
- Loader interfaces/stubs compile.
- Sample files are added under `docs/samples/`.
- No enforcement logic beyond basic parsing.

**Files to Touch**
- `Witcher3StringEditor.Common/Terminology/...`
- `Witcher3StringEditor/...` (loader implementation)
- `docs/samples/...`
- `docs/integrations.md`

**QA Checklist**
- Build succeeds.
- Loading samples does not alter translation workflows.

### Issue 5: Translation memory + QA stores (SQLite bootstrap)
**Description**
Ensure SQLite stores are wired for translation memory and QA metadata (inert by default).

**Acceptance Criteria**
- Store interfaces + bootstrap exist.
- AppData-based database path is used.
- No automatic background writes yet.

**Files to Touch**
- `Witcher3StringEditor.Common/TranslationMemory/...`
- `Witcher3StringEditor.Common/QualityAssurance/...`
- `Witcher3StringEditor.Data/TranslationMemory/...`
- `Witcher3StringEditor.Data/QualityAssurance/...`
- `docs/integrations.md`

**QA Checklist**
- Build succeeds.
- Database file is created only when stores are called.

## Decision Log / Inventory
### Configuration Persistence (ConfigService / AppSettings)
- `ConfigService` serializes settings to JSON and returns a new settings instance when the file is missing.【F:Witcher3StringEditor/Services/ConfigService.cs†L1-L38】
- `AppSettings` houses persisted user settings such as translator choice and translation provider defaults (including profile id).【F:Witcher3StringEditor/Models/AppSettings.cs†L1-L122】
- `App.xaml.cs` constructs the settings file path under AppData and registers `ConfigService` and `AppSettings` with DI using that path.【F:Witcher3StringEditor/App.xaml.cs†L120-L177】【F:Witcher3StringEditor/App.xaml.cs†L267-L279】

### Translation Entry Points (ITranslator / TranslateAsync / TranslationDialogViewModel)
- Service registration for translators lives in `App.xaml.cs`, where `ITranslator` implementations (Microsoft/Google/Yandex) are registered with DI. This is the root entry point for resolving translators used across the UI.【F:Witcher3StringEditor/App.xaml.cs†L288-L313】
- Translator selection is exposed in the settings dialog: `SettingDialogViewModel` surfaces the available translator names and binds them to `AppSettings.Translator` in the settings UI. This is the configuration entry point for picking the active translator.【F:Witcher3StringEditor.Dialogs/ViewModels/SettingDialogViewModel.cs†L16-L73】【F:Witcher3StringEditor.Dialogs/Views/SettingsDialog.xaml†L201-L209】
- The translation dialog flow is launched from `MainWindowViewModel.ShowTranslateDialog`, which resolves the configured `ITranslator` from DI and instantiates `TranslationDialogViewModel`. This is the main UI entry point for translation work.【F:Witcher3StringEditor/ViewModels/MainWindowViewModel.cs†L624-L645】
- `TranslationDialogViewModel` owns the dialog-level flow and toggles between single-item and batch translation view models. It is the orchestrator for translation operations inside the modal dialog.【F:Witcher3StringEditor.Dialogs/ViewModels/TranslationDialogViewModel.cs†L21-L121】
- Actual translation calls (`TranslateAsync`) are invoked in:
  - `SingleItemTranslationViewModel.ExecuteTranslationTask`, which calls `Translator.TranslateAsync` for the currently selected item and handles cancellation via a token source.【F:Witcher3StringEditor.Dialogs/ViewModels/SingleItemTranslationViewModel.cs†L159-L208】
  - `BatchItemsTranslationViewModel.TranslateItem`, which calls `translator.TranslateAsync` while iterating a selected item range in the batch workflow.【F:Witcher3StringEditor.Dialogs/ViewModels/BatchItemsTranslationViewModel.cs†L251-L264】
- Shared translation plumbing is in `TranslationViewModelBase`, which stores the `ITranslator`, initializes language lists, and sets language defaults used by both single and batch flows.【F:Witcher3StringEditor.Dialogs/ViewModels/TranslationViewModelBase.cs†L15-L74】

### Background/Batch Translators Beyond Dialog Flow
- There are no background or batch translation services outside the translation dialog flow. The only batch translation logic lives in `BatchItemsTranslationViewModel`, which is created and controlled by `TranslationDialogViewModel` within the modal dialog.【F:Witcher3StringEditor.Dialogs/ViewModels/BatchItemsTranslationViewModel.cs†L18-L180】【F:Witcher3StringEditor.Dialogs/ViewModels/TranslationDialogViewModel.cs†L82-L121】
