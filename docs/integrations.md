# Integrations Plan (TM, Ollama, Terminology, Profiles)

## Architecture overview
- **Provider contracts (future-facing, not yet wired into translation flow)**
  - `ITranslationProvider` defines `ListModelsAsync` + `TranslateAsync` for provider implementations.
  - Integration-facing DTOs live in `Witcher3StringEditor/Integrations/Providers/TranslationProviderModels.cs`:
    - `TranslationRequest` (input text, language info, model id, metadata).
    - `TranslationResult` (output text, model id, metadata).
    - `TranslationProviderModel` (model id, display name, metadata).
  - An in-memory registry (`ITranslationProviderRegistry` + `InMemoryTranslationProviderRegistry`) exists as a
    stub; it is not registered in DI and is not used by production code paths yet.
  - Current production routing still uses the legacy contracts in `Witcher3StringEditor.Common/Translation/`.
  - These integration contracts are intentionally inert and should not be referenced by the current translation flow
    until routing is explicitly enabled.
- **Settings persistence**
  - Settings are stored in `AppSettings.Json` via `ConfigService` and loaded in `App.xaml.cs` at startup.
  - `AppSettings` implements `IAppSettings` and is the binding target for the Settings dialog.
- **Translation entrypoints**
  - `MainWindowViewModel.ShowTranslateDialog` constructs `TranslationDialogViewModel` and passes routing services.
  - `TranslationRouter` orchestrates provider routing and falls back to `LegacyTranslationRouter` when needed.
  - `TranslationPipelineContextBuilder` merges app settings with optional translation profiles.
- **Local integrations**
  - Translation memory storage is local (SQLite/JSON) in `Witcher3StringEditor.Data`.
  - Terminology/style loading is local via `ITerminologyLoader` / `IStyleGuideLoader`.
  - Profiles are local JSON and resolved through catalog + resolver interfaces.

## Planned work as GitHub issues

### Issue 0: Integration provider registry scaffolding
**Description**
Add a minimal interface and in-memory registry for integration providers without wiring it into DI or runtime flows.

**Acceptance Criteria**
- Registry interface and in-memory implementation compile.
- No DI registration or runtime usage is added.
- Provider scaffolding remains inert and future-facing.

**Files to touch**
- `Witcher3StringEditor/Integrations/Providers/ITranslationProviderRegistry.cs`
- `Witcher3StringEditor/Integrations/Providers/InMemoryTranslationProviderRegistry.cs`
- `docs/integrations.md`

**QA checklist**
- Build: `dotnet build`.
- Manual: confirm no new settings/UI changes.
- No regressions: translation flow remains unchanged.

---

### Issue 1: Inventory pass for integration entrypoints
**Description**
Document where settings persistence, translation routing, and integration hooks live so future work can wire in safely.

**Acceptance Criteria**
- Inventory report lists settings persistence, translation entrypoints, and integration namespaces.
- Key classes include file paths and responsibilities.
- Risks/constraints (startup ordering, DI lifetimes, settings binding) are captured.

**Files to touch**
- `docs/inspections/inventory.md`
- `docs/integrations.md`

**QA checklist**
- Build: `dotnet build` (or confirm existing build workflow).
- Manual: open the inventory report and verify each section has file paths.
- No regressions: documentation-only change.

---

### Issue 2: Database-backed translation memory bootstrap
**Description**
Wire translation memory initialization to local storage (SQLite/JSON) while keeping it disabled by default.

**Acceptance Criteria**
- TM initialization is explicit and only runs when TM is enabled.
- TM storage remains local (SQLite/JSON); no external services.
- Default experience is unchanged when TM is disabled.

**Files to touch**
- `Witcher3StringEditor.Data/TranslationMemory/SqliteTranslationMemoryDatabaseInitializer.cs`
- `Witcher3StringEditor.Data/TranslationMemory/SqliteTranslationMemoryStore.cs`
- `Witcher3StringEditor/Services/TranslationMemorySettingsProvider.cs`
- `Witcher3StringEditor/App.xaml.cs`

**QA checklist**
- Build: `dotnet build`.
- Manual: toggle TM in Settings and ensure the app still starts.
- No regressions: translation flows unchanged with TM off.

---

### Issue 3: Ollama provider + model selection scaffolding
**Description**
Keep Ollama local with model listing and model selection stubs. No new workflow beyond model selection.

**Acceptance Criteria**
- Ollama provider is registered and model list retrieval is wired (async, no UI blocking).
- Settings store BaseUrl, model name, and timeout locally.
- No changes to translation behavior until provider routing is explicitly enabled.

**Files to touch**
- `Witcher3StringEditor.Integrations.Ollama/OllamaTranslationProvider.cs`
- `Witcher3StringEditor.Integrations.Ollama/OllamaSettings.cs`
- `Witcher3StringEditor.Dialogs/ViewModels/SettingsDialogViewModel.cs`
- `Witcher3StringEditor/Models/AppSettings.cs`
- `Witcher3StringEditor/App.xaml.cs`

**QA checklist**
- Build: `dotnet build`.
- Manual: open Settings, select Ollama, and refresh models (no crash).
- No regressions: existing translators still function.

---

### Issue 4: Terminology + style guide loading pipeline
**Description**
Continue local terminology/style loading with validation hooks and no workflow changes.

**Acceptance Criteria**
- Terminology/style files load through existing loaders.
- Validation hooks exist but remain no-op by default.
- Settings UI only shows minimal placeholders/status text.

**Files to touch**
- `Witcher3StringEditor/Services/TerminologyLoader.cs`
- `Witcher3StringEditor/Services/NoopTerminologyValidationService.cs`
- `Witcher3StringEditor.Dialogs/ViewModels/SettingsDialogViewModel.cs`

**QA checklist**
- Build: `dotnet build`.
- Manual: load sample terminology/style files and confirm status updates.
- No regressions: translations behave the same with terminology disabled.

---

### Issue 5: Translation profiles storage + resolver wiring
**Description**
Keep translation profiles local and optional. Provide list + resolver wiring without changing default behavior.

**Acceptance Criteria**
- Profiles load from local JSON storage and list in Settings.
- Selected profile resolves to a concrete `TranslationProfile` for pipeline use.
- No profile selected = default settings behavior.

**Files to touch**
- `Witcher3StringEditor.Data/Profiles/JsonTranslationProfileStore.cs`
- `Witcher3StringEditor.Data/Profiles/TranslationProfileCatalog.cs`
- `Witcher3StringEditor/Services/NoopTranslationProfileResolver.cs`
- `Witcher3StringEditor.Dialogs/ViewModels/SettingsDialogViewModel.cs`
- `Witcher3StringEditor/App.xaml.cs`

**QA checklist**
- Build: `dotnet build`.
- Manual: load a profile file and confirm it appears in Settings.
- No regressions: leaving profile empty should not change translation results.
