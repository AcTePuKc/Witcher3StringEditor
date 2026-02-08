# Integration Backlog (GitHub Issue Drafts)

## Scope Overview
These issue drafts cover the planned **database-backed translation memory**, **Ollama model selection**, **terminology/style
loading**, and **translation profile** scaffolding. Each issue stays inert by default and focuses on stubs/interfaces only.

## Issue 0: Translation flow + fallback investigation
**Description**
Trace the current single-item and batch translation flows, including how the router picks providers vs legacy translators,
and document all fallback/default/error handling behavior.

**Acceptance Criteria**
- A Markdown report documents the single-item and batch translation flows.
- File paths, methods, and trigger conditions are listed for each flow step.
- Fallback/default selection logic and provider error handling are enumerated.

**Note: suspected single-row translation iteration path (current)**
- `Witcher3StringEditor.Dialogs/ViewModels/SingleItemTranslationViewModel.cs`
  - `OnCurrentItemIndexChanged` updates the in-memory row model for the selected index.
  - `Navigate`, `Previous`, and `Next` drive per-row iteration and auto-save checks.
  - `Translate` triggers the per-row translation execution path.

**Files to Touch**
- `docs/fallback-investigation.md`

**QA Checklist**
- Build: `dotnet build`
- Manual: confirm referenced files still exist and paths are accurate
- No regressions: translation dialog still opens and legacy translator list is intact

---

## Issue 1: Inventory pass for integration entrypoints
**Description**
Confirm where settings are persisted, where translation requests flow, which dialogs/view models will host provider/model/terminology/profile configuration, and which integration namespaces should be the long-term source of truth.

**Acceptance Criteria**
- Document settings persistence flow and config path.
- Identify translation entrypoints and extension points for provider/model selection.
- Identify UI surfaces for settings placeholders.
- Confirm whether `Witcher3StringEditor.Common/*` or `Witcher3StringEditor/Integrations/*` should own integration abstractions.
- Summarize risks or constraints.

**Files to Touch**
- `docs/inventory-report.md`

**QA Checklist**
- Build: `dotnet build`
- Manual: open Settings dialog and confirm the app still launches
- No regressions: legacy translator-based translation still works

---

## Issue 2: Translation memory + QA stores (SQLite scaffolding)
**Description**
Ensure local translation memory and QA stores use a minimal SQLite schema and AppData-based storage paths. Keep operations to upsert + exact-match lookup only.

**Acceptance Criteria**
- SQLite bootstrap and schemas exist for translation memory + QA.
- Database initialization is abstracted behind an `ITranslationMemoryDatabaseInitializer` stub for future migrations.
- Store interfaces remain local-only and inert by default.
- Add a translation memory workflow stub (`ITranslationMemoryService`) with a no-op implementation.
- Storage uses AppData paths only.

**Files to Touch**
- `Witcher3StringEditor.Common/TranslationMemory/*`
- `Witcher3StringEditor.Common/TranslationMemory/ITranslationMemoryService.cs`
- `Witcher3StringEditor.Common/QualityAssurance/*`
- `Witcher3StringEditor.Data/Storage/*`
- `Witcher3StringEditor.Data/TranslationMemory/*`
- `Witcher3StringEditor.Data/QualityAssurance/*`
- `Witcher3StringEditor.Data/TranslationMemory/NoopTranslationMemoryDatabaseInitializer.cs`
- `Witcher3StringEditor/Services/NoopTranslationMemoryService.cs`
- `docs/integrations.md`

**QA Checklist**
- Build: `dotnet build`
- Manual: app startup without database file side effects
- No regressions: translation dialog behaves as before

---

## Issue 3: Provider registry + model discovery wiring (Ollama first)
**Description**
Add a provider registry abstraction that maps provider names to implementations and supports model discovery (Ollama first). Keep existing translator selection intact.

**Acceptance Criteria**
- Provider registry returns known provider descriptors and resolves providers by name.
- Settings dialog has placeholders for provider/model/base URL without changing translator behavior.
- Model discovery uses provider `ListModelsAsync` (stub behavior allowed).
- Cached model list persists in settings for offline UX.

**Files to Touch**
- `Witcher3StringEditor.Common/Translation/*`
- `Witcher3StringEditor/Services/NoopTranslationModelCatalog.cs`
- `Witcher3StringEditor/Services/*TranslationProviderRegistry*.cs`
- `Witcher3StringEditor.Integrations.Ollama/*`
- `Witcher3StringEditor.Dialogs/ViewModels/SettingsDialogViewModel.cs`
- `Witcher3StringEditor.Dialogs/Views/SettingsDialog.xaml`
- `docs/integrations.md`

**QA Checklist**
- Build: `dotnet build`
- Manual: open Settings dialog and confirm provider/model placeholders render
- No regressions: translation dialog still loads existing translator list

---

## Issue 4: Terminology + style pack loading hooks
**Description**
Keep terminology/style loaders local-only, add TODO hooks for prompt injection and post-translation validation, and validate parsing against sample files.

**Acceptance Criteria**
- Loader supports TSV/CSV terminology packs and Markdown style guides.
- Prompt builder interface exists with a no-op implementation for later prompt injection.
- TODO markers exist for future prompt injection/validation.
- Samples under `docs/samples/` parse without errors.

**Files to Touch**
- `Witcher3StringEditor.Common/Terminology/*`
- `Witcher3StringEditor/Services/TerminologyLoader.cs`
- `Witcher3StringEditor/Services/NoopTerminologyPromptBuilder.cs`
- `docs/samples/*`
- `docs/integrations.md`

**QA Checklist**
- Build: `dotnet build`
- Manual: load a sample pack and confirm no crash
- No regressions: translation still works without a terminology pack

---

## Issue 5: Translation profile storage + resolver
**Description**
Persist profiles locally (JSON) and add a resolver stub that can merge a selected profile into current settings later.

**Acceptance Criteria**
- JSON-backed profile store returns empty list when file is missing.
- Resolver stub returns null when no profile is selected.
- Profile model includes optional terminology/style paths, file path aliases, enablement toggles, and translation memory enablement.
- No UI wiring or behavior changes to existing translator selection.

**Files to Touch**
- `Witcher3StringEditor.Common/Profiles/*`
- `Witcher3StringEditor.Data/Profiles/*`
- `Witcher3StringEditor/Services/*TranslationProfileResolver*.cs`
- `docs/integrations.md`

**QA Checklist**
- Build: `dotnet build`
- Manual: app startup and Settings dialog open
- No regressions: existing translation flow unchanged

---

## Issue 6: Integration architecture spec + wiring map
**Description**
Update and expand `docs/integrations.md` with a high-level architecture sketch, planned tasks, and explicit extension points for providers, terminology, and profiles. Include a minimal wiring map so future agents know where to hook in without guessing.

**Acceptance Criteria**
- `docs/integrations.md` documents provider registry, translation memory, terminology, and profile layers.
- Explicit TODO list of integration tasks with file/path hints.
- Notes identify any uncertain locations requiring inventory confirmation.

**Files to Touch**
- `docs/integrations.md`

**Dependencies**
- Issue 1 (Inventory pass for integration entrypoints)

**QA Checklist**
- Build: `dotnet build`
- Manual: open `docs/integrations.md` and verify links/paths are accurate
- No regressions: confirm no runtime-facing changes were introduced

**Current Status / Partial Completion**
- An initial draft of the integrations spec exists at `docs/integrations.md`, but it needs to be expanded with more detail.

---

## Issue 7: Translation pipeline context builder (settings + profiles + terminology)
**Description**
Introduce a small pipeline context builder that combines settings, profile selection, and terminology/style paths into a
single read-only context object for future translation routing. Keep it unused by default.

**Acceptance Criteria**
- A context builder returns a `TranslationPipelineContext` populated from settings and the selected profile.
- No translation routing changes; the context is not consumed yet.
- TODO markers indicate where translation memory and terminology injection will later occur.

**Files to Touch**
- `Witcher3StringEditor.Common/Translation/TranslationPipelineContext.cs`
- `Witcher3StringEditor/Services/*TranslationPipelineContext*.cs`
- `docs/integrations.md`

**Dependencies**
- Issue 3 (Provider registry + model discovery wiring)
- Issue 4 (Terminology + style pack loading hooks)
- Issue 5 (Translation profile storage + resolver)

## Issue 8: Settings UI placeholders for integrations
**Description**
Add minimal settings UI placeholders for provider selection, model selection, terminology packs, and translation profile selection. No behavioral changes; values can be stored but not used.

**Acceptance Criteria**
- Settings dialog shows placeholders for provider/model selection.
- Terminology pack and profile pickers exist (non-functional allowed).
- Persisted settings are inert by default.

**Files to Touch**
- `Witcher3StringEditor.Dialogs/Views/SettingsDialog.xaml`
- `Witcher3StringEditor.Dialogs/ViewModels/SettingsDialogViewModel.cs`
- `Witcher3StringEditor.Common/Settings/*` (if required)
- `docs/integrations.md`

**Dependencies**
- Issue 3 (Provider registry + model discovery wiring)
- Issue 4 (Terminology + style pack loading hooks)
- Issue 5 (Translation profile storage + resolver)

**QA Checklist**
- Build: `dotnet build`
- Manual: open Settings dialog and verify placeholders render
- No regressions: translation dialog still opens and legacy translator list is intact

---

## Tracking Note: GoogleTranslator disposal lifecycle
**Observation**
`GoogleTranslator` instances (from `GTranslate.Translators`) are created via DI as `ITranslator` and disposed in two spots:
- `Witcher3StringEditor/ViewModels/MainWindowViewModel.cs` in `ShowTranslateDialog` after the translation dialog closes.
- `Witcher3StringEditor/ViewModels/MainWindowViewModel.cs` in `ShowSettingsDialog` after enumerating translator names.

**Suspected Lifecycle Issue**
If `GoogleTranslator` maintains internal HTTP clients or caches that expect a longer lifetime (e.g., across multiple dialog opens), disposing immediately after each dialog might cause avoidable reinitialization or premature teardown during dialog usage if instances are shared or cached. Follow-up should confirm intended lifetimes in the DI container and whether translators should be scoped for the full app session instead of per-dialog usage.

---

## Issue 9: Terminology/style enablement toggles + load status
**Description**
Add inert enablement toggles for terminology and style guide preview loading. When toggled on, the Settings dialog
should attempt to load the selected file and surface a status line (loaded/failed) without enforcing behavior.

**Acceptance Criteria**
- `IAppSettings` includes `UseTerminologyPack` and `UseStyleGuide` flags.
- Settings dialog shows enablement toggles and load status text for terminology and style guide.
- Loader is only invoked when the corresponding toggle is enabled.
- No translation flow behavior changes.

**Files to Touch**
- `Witcher3StringEditor.Common/Abstractions/IAppSettings.cs`
- `Witcher3StringEditor/Models/AppSettings.cs`
- `Witcher3StringEditor.Dialogs/ViewModels/SettingsDialogViewModel.cs`
- `Witcher3StringEditor.Dialogs/Views/SettingsDialog.xaml`
- `docs/integrations.md`

**QA Checklist**
- Build: `dotnet build`
- Manual: open Settings dialog, toggle terminology/style, and confirm status text updates
- No regressions: translation dialog still opens
- No regressions: existing translators still listed

---

## Issue 10: Translation memory settings toggle + path persistence
**Description**
Add inert settings for translation memory enablement and local database path selection. Keep the toggle and path display
in the Settings dialog without wiring translation flow logic.

**Acceptance Criteria**
- `IAppSettings` includes `UseTranslationMemory` and `TranslationMemoryPath`.
- Settings dialog shows a translation memory card with a toggle and path display field.
- Provide a settings provider stub that maps app settings to `TranslationMemorySettings`.
- No translation flow behavior changes.

**Files to Touch**
- `Witcher3StringEditor.Common/Abstractions/IAppSettings.cs`
- `Witcher3StringEditor/Models/AppSettings.cs`
- `Witcher3StringEditor.Common/TranslationMemory/ITranslationMemorySettingsProvider.cs`
- `Witcher3StringEditor/Services/TranslationMemorySettingsProvider.cs`
- `Witcher3StringEditor.Dialogs/Views/SettingsDialog.xaml`
- `docs/integrations.md`

**QA Checklist**
- Build: `dotnet build`
- Manual: open Settings dialog and confirm the translation memory card renders
- No regressions: legacy translator selection remains unchanged

---

## Issue 11: Translation router request metadata expansion
**Description**
Extend the translation router request DTO to carry optional provider and model names so future call sites can override
app settings. Add a noop router implementation for scenarios where routing should be disabled without side effects.

**Acceptance Criteria**
- `TranslationRouterRequest` includes provider + model name fields.
- Router resolves provider/model names from the request first, falling back to app settings.
- Noop router returns a non-success result without performing work.

**Files to Touch**
- `Witcher3StringEditor.Common/Translation/ITranslationRouter.cs`
- `Witcher3StringEditor/Services/LegacyTranslationRouter.cs`
- `Witcher3StringEditor/Services/TranslationRouter.cs`
- `docs/integrations.md`

**QA Checklist**
- Build: `dotnet build`
- Manual: open Translation dialog and confirm no behavior change without provider selection
- No regressions: existing translators still listed

**Current Status / Partial Completion**
- Placeholder UI does not exist yet; only issue drafts cover it.

---

## Issue 12: Ollama integration settings + model list stub
**Description**
Create an Ollama settings model (BaseUrl, Model, parameters), add a stubbed client, and provide a placeholder `ListModelsAsync` method.

**Acceptance Criteria**
- Settings model lives under `Witcher3StringEditor.Integrations.Ollama`.
- Client stub compiles and returns a placeholder model list.
- No network calls executed by default.

**Files to Touch**
- `Witcher3StringEditor.Integrations.Ollama/*`
- `Witcher3StringEditor.Common/Translation/*` (if shared models needed)
- `docs/integrations.md`

**Dependencies**
- Issue 3 (Provider registry + model discovery wiring)

**QA Checklist**
- Build: `dotnet build`
- Manual: open Settings dialog and confirm no runtime errors
- No regressions: translation flow unchanged

**Current Status / Partial Completion**
- Integration project already includes an Ollama settings model and provider stub; wiring into the
  provider registry and settings UI remains.

---

## Issue 13: Output post-processing rules (opt-in)
**Description**
Introduce a minimal post-processing pipeline for translation output cleanup (e.g., stripping polite prefixes). Keep
rules opt-in and default to no-op behavior so translation output remains unchanged unless users enable it.

**Acceptance Criteria**
- `ITranslationPostProcessor` interface exists with a `Process(string, TranslationContext)` signature.
- A no-op post-processor returns the original text unchanged.
- Settings include an opt-in toggle (or profile flag) for post-processing rules (placeholder acceptable).
- No behavior changes unless the toggle is enabled.

**Files to Touch**
- `Witcher3StringEditor.Common/Translation/ITranslationPostProcessor.cs`
- `Witcher3StringEditor.Common/Translation/TranslationContext.cs`
- `Witcher3StringEditor/Services/NoopTranslationPostProcessor.cs`
- `Witcher3StringEditor.Common/Abstractions/IAppSettings.cs` (if needed)
- `Witcher3StringEditor/Models/AppSettings.cs` (if needed)
- `docs/integrations.md`
- `docs/ai-output-notes.md`

**QA Checklist**
- Build: `dotnet build`
- Manual: open Settings dialog and confirm no regressions
- No regressions: translation output unchanged when post-processing is disabled

---

## Issue 14: Translation memory lookup/save wiring in translation dialogs
**Description**
Wire translation memory lookups into the single-item and batch translation flows so the dialog can reuse cached
translations and store new results after successful translations. Keep the default behavior inert via the no-op
translation memory service.

**Acceptance Criteria**
- Translation dialog view models call `ITranslationMemoryService.LookupAsync` before provider/legacy translation.
- Successful translations are saved via `ITranslationMemoryService.SaveAsync`.
- The no-op translation memory service remains the default registration.
- No UI/UX changes and no behavior changes when translation memory is disabled.

**Files to Touch**
- `Witcher3StringEditor.Dialogs/ViewModels/TranslationViewModelBase.cs`
- `Witcher3StringEditor.Dialogs/ViewModels/SingleItemTranslationViewModel.cs`
- `Witcher3StringEditor.Dialogs/ViewModels/BatchItemsTranslationViewModel.cs`
- `Witcher3StringEditor.Dialogs/ViewModels/TranslationDialogViewModel.cs`
- `Witcher3StringEditor/ViewModels/MainWindowViewModel.cs`
- `Witcher3StringEditor/App.xaml.cs`
- `Witcher3StringEditor.Common/TranslationMemory/ITranslationMemoryService.cs`
- `Witcher3StringEditor/Services/NoopTranslationMemoryService.cs`
- `docs/integrations.md`

**QA Checklist**
- Build: `dotnet build`
- Manual: open the translation dialog and run a single translation to ensure no errors
- No regressions: legacy translator selection remains the default
