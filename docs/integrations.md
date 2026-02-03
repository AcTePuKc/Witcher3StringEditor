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

## Decision Log / Inventory
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
