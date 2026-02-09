# Integration Backlog (GitHub Issue Drafts)

## Scope Overview
These issue drafts cover the planned **database-backed translation memory**, **Ollama model selection**, **terminology/style
loading**, and **translation profile** scaffolding. Each issue stays inert by default and focuses on stubs/interfaces only.


## Phase 0: compile-safe scaffolding
Phase 0 issues are limited to compile-safe scaffolding and documentation updates only.

**Phase 0 constraints**
- No full feature implementation.
- No large refactors.
- No behavior changes unless explicitly marked as placeholders.
- No external services; local-only storage (e.g., SQLite/JSON) when applicable.
- UI/UX changes must be minimal placeholders for settings panels.
- Changes must compile (or be explicitly deferred to a follow-up issue).

## Issue 00: Translation provider contracts (interfaces + DTOs)
**Description**
Add the provider contract interfaces and DTOs that future integrations will implement. Keep them unused by the existing
translation flow so the current legacy translator path remains the default.

**Acceptance Criteria**
- `ITranslationProvider` includes `ListModelsAsync` and `TranslateAsync` signatures.
- Minimal DTOs exist for request/result/model metadata (`TranslationRequest`, `TranslationResult`, `ModelInfo`).
- No new call sites are introduced in the existing translation flow.

**Files to Touch**
- `Witcher3StringEditor.Common/Translation/ITranslationProvider.cs`
- `Witcher3StringEditor.Common/Translation/TranslationModels.cs`
- `docs/integrations.md`


**Behavior Impact**
- Default: no runtime behavior change; scaffolding only.
- UI: placeholders only when explicitly called out (no workflow changes).

**Build Commands**
- `dotnet build`

**Reminder (Phase 0)**
- Keep changes compile-safe scaffolding only (stubs/interfaces/TODOs).
- No full feature implementation, no external services, local-only storage.

**QA Checklist**
- Build: `dotnet build`
- Manual: confirm the translation dialog still defaults to legacy translators
- No regressions: provider routing remains opt-in/inert

---

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


**Behavior Impact**
- Default: no runtime behavior change; scaffolding only.
- UI: placeholders only when explicitly called out (no workflow changes).

**Build Commands**
- `dotnet build`

**Reminder (Phase 0)**
- Keep changes compile-safe scaffolding only (stubs/interfaces/TODOs).
- No full feature implementation, no external services, local-only storage.

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


**Behavior Impact**
- Default: no runtime behavior change; scaffolding only.
- UI: placeholders only when explicitly called out (no workflow changes).

**Build Commands**
- `dotnet build`

**Reminder (Phase 0)**
- Keep changes compile-safe scaffolding only (stubs/interfaces/TODOs).
- No full feature implementation, no external services, local-only storage.

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
- Add translation memory workflow stubs (`ITranslationMemoryService`) with a no-op implementation.
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
- `Witcher3StringEditor/Integrations/Storage/ITranslationMemoryService.cs`
- `Witcher3StringEditor/Integrations/Storage/StubTranslationMemoryService.cs`
- `docs/integrations.md`


**Behavior Impact**
- Default: no runtime behavior change; scaffolding only.
- UI: placeholders only when explicitly called out (no workflow changes).

**Build Commands**
- `dotnet build`

**Reminder (Phase 0)**
- Keep changes compile-safe scaffolding only (stubs/interfaces/TODOs).
- No full feature implementation, no external services, local-only storage.

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
- Settings dialog includes a provider timeout setting in seconds (no runtime enforcement yet).
- Model discovery uses provider `ListModelsAsync` (stub behavior allowed).
- Settings dialog includes a Test Connection button that reports provider health (success/failure).
- Cached model list persists in settings for offline UX.
- Document which registry type is DI-backed vs. legacy/local, with a TODO to consolidate later.

**Files to Touch**
- `Witcher3StringEditor.Common/Translation/*`
- `Witcher3StringEditor.Common/Abstractions/IAppSettings.cs`
- `Witcher3StringEditor/Services/NoopTranslationModelCatalog.cs`
- `Witcher3StringEditor/Services/*TranslationProviderRegistry*.cs`
- `Witcher3StringEditor/Services/TranslationProviderHealthCheck.cs`
- `Witcher3StringEditor/App.xaml.cs`
- `Witcher3StringEditor.Integrations.Ollama/*`
- `Witcher3StringEditor.Dialogs/ViewModels/SettingsDialogViewModel.cs`
- `Witcher3StringEditor.Dialogs/Views/SettingsDialog.xaml`
- `Witcher3StringEditor/Models/AppSettings.cs`
- `docs/integrations.md`


**Behavior Impact**
- Default: no runtime behavior change; scaffolding only.
- UI: placeholders only when explicitly called out (no workflow changes).

**Build Commands**
- `dotnet build`

**Reminder (Phase 0)**
- Keep changes compile-safe scaffolding only (stubs/interfaces/TODOs).
- No full feature implementation, no external services, local-only storage.

**QA Checklist**
- Build: `dotnet build`
- Manual: open Settings dialog and confirm provider/model placeholders render
- No regressions: translation dialog still loads existing translator list

---

## Issue 4: Terminology + style pack loading hooks
**Description**
Keep terminology/style loaders local-only, add TODO hooks for prompt injection and post-translation validation, and validate
parsing against sample files. The Integrations layer includes minimal parsing stubs (TSV/CSV + Markdown) without UI wiring.

**Acceptance Criteria**
- Loader supports TSV/CSV terminology packs and Markdown style guides with minimal parsing.
- Prompt builder interface exists with a no-op implementation for later prompt injection.
- TODO markers exist for future prompt injection/validation.
- Samples under `docs/samples/` parse without errors.
- No UI/translation flow wiring is introduced.

**Files to Touch**
- `Witcher3StringEditor.Common/Terminology/*`
- `Witcher3StringEditor/Integrations/Terminology/*`
- `Witcher3StringEditor/Services/TerminologyLoader.cs`
- `Witcher3StringEditor/Services/NoopTerminologyPromptBuilder.cs`
- `docs/samples/*`
- `docs/integrations.md`


**Behavior Impact**
- Default: no runtime behavior change; scaffolding only.
- UI: placeholders only when explicitly called out (no workflow changes).

**Build Commands**
- `dotnet build`

**Reminder (Phase 0)**
- Keep changes compile-safe scaffolding only (stubs/interfaces/TODOs).
- No full feature implementation, no external services, local-only storage.

**QA Checklist**
- Build: `dotnet build`
- Manual: load a sample pack and confirm no crash
- No regressions: translation still works without a terminology pack

---

## Issue 5: Translation profile model + JSON store
**Description**
Define a minimal translation profile model and persist profiles locally (JSON), leaving resolver wiring for a follow-up
task once selection is hooked up.

**Acceptance Criteria**
- Translation profile model includes core identifiers, provider/model, and optional terminology/style/tm fields.
- JSON-backed profile store returns empty list when file is missing.
- No UI wiring or behavior changes to existing translator selection.

**Files to Touch**
- `Witcher3StringEditor/Integrations/Profiles/TranslationProfile.cs`
- `Witcher3StringEditor/Integrations/Profiles/JsonTranslationProfileStore.cs`
- `docs/integrations.md`


**Behavior Impact**
- Default: no runtime behavior change; scaffolding only.
- UI: placeholders only when explicitly called out (no workflow changes).

**Build Commands**
- `dotnet build`

**Reminder (Phase 0)**
- Keep changes compile-safe scaffolding only (stubs/interfaces/TODOs).
- No full feature implementation, no external services, local-only storage.

**QA Checklist**
- Build: `dotnet build`
- Manual: inspect the JSON store to confirm missing-file behavior
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


**Behavior Impact**
- Default: no runtime behavior change; scaffolding only.
- UI: placeholders only when explicitly called out (no workflow changes).

**Build Commands**
- `dotnet build`

**Reminder (Phase 0)**
- Keep changes compile-safe scaffolding only (stubs/interfaces/TODOs).
- No full feature implementation, no external services, local-only storage.

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


**Behavior Impact**
- Default: no runtime behavior change; scaffolding only.
- UI: placeholders only when explicitly called out (no workflow changes).

**Build Commands**
- `dotnet build`

**Reminder (Phase 0)**
- Keep changes compile-safe scaffolding only (stubs/interfaces/TODOs).
- No full feature implementation, no external services, local-only storage.

**Dependencies**
- Issue 3 (Provider registry + model discovery wiring)
- Issue 4 (Terminology + style pack loading hooks)
- Issue 5 (Translation profile storage + resolver)

**QA Checklist**
- Build: `dotnet build`
- Manual: verify the context builder returns defaults without throwing
- No regressions: translation dialog still opens and legacy translator list is intact

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


**Behavior Impact**
- Default: no runtime behavior change; scaffolding only.
- UI: placeholders only when explicitly called out (no workflow changes).

**Build Commands**
- `dotnet build`

**Reminder (Phase 0)**
- Keep changes compile-safe scaffolding only (stubs/interfaces/TODOs).
- No full feature implementation, no external services, local-only storage.

**Dependencies**
- Issue 3 (Provider registry + model discovery wiring)
- Issue 4 (Terminology + style pack loading hooks)
- Issue 5 (Translation profile storage + resolver)

**QA Checklist**
- Build: `dotnet build`
- Manual: open Settings dialog and verify placeholders render
- No regressions: translation dialog still opens and legacy translator list is intact

---

## Issue 9: Translation profile selection service (stub)
**Description**
Introduce a minimal selection service abstraction so future UI/profile flows can set or clear the active profile without
directly binding to settings. Keep behavior inert by default.

**Acceptance Criteria**
- `ITranslationProfileSelectionService` provides get/set methods for the selected profile id.
- No-op implementation returns null and ignores set requests.
- No wiring changes to existing translation or settings flows.

**Files to Touch**
- `Witcher3StringEditor.Common/Profiles/ITranslationProfileSelectionService.cs`
- `Witcher3StringEditor/Services/NoopTranslationProfileSelectionService.cs`
- `docs/integrations.md`


**Behavior Impact**
- Default: no runtime behavior change; scaffolding only.
- UI: placeholders only when explicitly called out (no workflow changes).

**Build Commands**
- `dotnet build`

**Reminder (Phase 0)**
- Keep changes compile-safe scaffolding only (stubs/interfaces/TODOs).
- No full feature implementation, no external services, local-only storage.

**QA Checklist**
- Build: `dotnet build`
- Manual: app startup without profile selection side effects
- No regressions: translation dialog still opens and legacy translator list is intact

---

## Issue 12: TranslationProviderRegistry ownership consolidation
**Description**
Confirm that the DI-backed registry in `Witcher3StringEditor.Services` is the only active registry at runtime, then
retire the legacy `Witcher3StringEditor.Common.Translation.TranslationProviderRegistry` class (or keep a short-lived
adapter if a legacy call site still exists).

**Acceptance Criteria**
- A scan of the repo confirms all runtime usage goes through `ITranslationProviderRegistry`.
- Any remaining legacy registry call sites are migrated to the Services registry or routed through an adapter.
- The legacy registry class is removed or explicitly marked for removal once usage is eliminated.
- `docs/integrations.md` reflects the ownership decision and migration steps.

**Files to Touch**
- `Witcher3StringEditor.Common/Translation/TranslationProviderRegistry.cs`
- `Witcher3StringEditor/Services/*TranslationProviderRegistry*.cs`
- `docs/integrations.md`


**Behavior Impact**
- Default: no runtime behavior change; scaffolding only.
- UI: placeholders only when explicitly called out (no workflow changes).

**Build Commands**
- `dotnet build`

**Reminder (Phase 0)**
- Keep changes compile-safe scaffolding only (stubs/interfaces/TODOs).
- No full feature implementation, no external services, local-only storage.

**QA Checklist**
- Build: `dotnet build`
- Manual: open a translation dialog and confirm provider fallback still logs as expected
- No regressions: legacy translator selection remains intact

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


**Behavior Impact**
- Default: no runtime behavior change; scaffolding only.
- UI: placeholders only when explicitly called out (no workflow changes).

**Build Commands**
- `dotnet build`

**Reminder (Phase 0)**
- Keep changes compile-safe scaffolding only (stubs/interfaces/TODOs).
- No full feature implementation, no external services, local-only storage.

**QA Checklist**
- Build: `dotnet build`
- Manual: open Settings dialog, toggle terminology/style, and confirm status text updates
- No regressions: translation dialog still opens
- No regressions: existing translators still listed

---

## Issue 50: Terminology/style validation service stub
**Description**
Introduce an inert terminology/style validation service that can be called from the Settings dialog after files are
selected. The default implementation should always return a "Not validated" status so UI text can be wired without
changing translation output.

**Acceptance Criteria**
- Add `ITerminologyValidationService` + `TerminologyValidationResult` to `Common/Terminology`.
- Register a no-op implementation in DI that always returns a "Not validated" result.
- Settings dialog calls the validation service after successfully loading terminology and style guide files.
- Status text reflects the validation result without enforcing translation behavior.

**Files to Touch**
- `Witcher3StringEditor.Common/Terminology/ITerminologyValidationService.cs`
- `Witcher3StringEditor.Common/Terminology/TerminologyValidationResult.cs`
- `Witcher3StringEditor/Services/NoopTerminologyValidationService.cs`
- `Witcher3StringEditor/App.xaml.cs`
- `Witcher3StringEditor.Dialogs/ViewModels/SettingsDialogViewModel.cs`
- `docs/integrations.md`


**Behavior Impact**
- Default: no runtime behavior change; scaffolding only.
- UI: placeholders only when explicitly called out (no workflow changes).

**Build Commands**
- `dotnet build`

**Reminder (Phase 0)**
- Keep changes compile-safe scaffolding only (stubs/interfaces/TODOs).
- No full feature implementation, no external services, local-only storage.

**QA Checklist**
- Build: `dotnet build`
- Manual: select terminology/style files in Settings and confirm validation status shows "Not validated."
- No regressions: translation dialogs still open

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


**Behavior Impact**
- Default: no runtime behavior change; scaffolding only.
- UI: placeholders only when explicitly called out (no workflow changes).

**Build Commands**
- `dotnet build`

**Reminder (Phase 0)**
- Keep changes compile-safe scaffolding only (stubs/interfaces/TODOs).
- No full feature implementation, no external services, local-only storage.

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


**Behavior Impact**
- Default: no runtime behavior change; scaffolding only.
- UI: placeholders only when explicitly called out (no workflow changes).

**Build Commands**
- `dotnet build`

**Reminder (Phase 0)**
- Keep changes compile-safe scaffolding only (stubs/interfaces/TODOs).
- No full feature implementation, no external services, local-only storage.

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


**Behavior Impact**
- Default: no runtime behavior change; scaffolding only.
- UI: placeholders only when explicitly called out (no workflow changes).

**Build Commands**
- `dotnet build`

**Reminder (Phase 0)**
- Keep changes compile-safe scaffolding only (stubs/interfaces/TODOs).
- No full feature implementation, no external services, local-only storage.

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

## Issue 15: Translation dialog provider routing toggle
**Description**
Add a translation dialog toggle that captures whether provider routing should be used and plumb that intent into
translation router requests without changing behavior unless the router supports it.

**Acceptance Criteria**
- Translation view model state exposes a `UseProviderForTranslation` boolean (default false).
- Translation dialog UI binds a toggle to `UseProviderForTranslation`.
- Translation router requests carry the toggle value without changing legacy routing behavior by default.
- Switching between single/batch translation preserves the toggle state.

**Files to Touch**
- `Witcher3StringEditor.Common/Translation/ITranslationRouter.cs`
- `Witcher3StringEditor.Dialogs/ViewModels/TranslationViewModelBase.cs`
- `Witcher3StringEditor.Dialogs/ViewModels/SingleItemTranslationViewModel.cs`
- `Witcher3StringEditor.Dialogs/ViewModels/BatchItemsTranslationViewModel.cs`
- `Witcher3StringEditor.Dialogs/ViewModels/TranslationDialogViewModel.cs`
- `Witcher3StringEditor.Dialogs/Views/TranslationDialog.xaml`
- `docs/integrations.md`


**Behavior Impact**
- Default: no runtime behavior change; scaffolding only.
- UI: placeholders only when explicitly called out (no workflow changes).

**Build Commands**
- `dotnet build`

**Reminder (Phase 0)**
- Keep changes compile-safe scaffolding only (stubs/interfaces/TODOs).
- No full feature implementation, no external services, local-only storage.

**QA Checklist**
- Build: `dotnet build`
- Manual: open Translation dialog, toggle provider routing on/off, and confirm no translation behavior changes
- No regressions: legacy translator selection remains the default path

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


**Behavior Impact**
- Default: no runtime behavior change; scaffolding only.
- UI: placeholders only when explicitly called out (no workflow changes).

**Build Commands**
- `dotnet build`

**Reminder (Phase 0)**
- Keep changes compile-safe scaffolding only (stubs/interfaces/TODOs).
- No full feature implementation, no external services, local-only storage.

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


**Behavior Impact**
- Default: no runtime behavior change; scaffolding only.
- UI: placeholders only when explicitly called out (no workflow changes).

**Build Commands**
- `dotnet build`

**Reminder (Phase 0)**
- Keep changes compile-safe scaffolding only (stubs/interfaces/TODOs).
- No full feature implementation, no external services, local-only storage.

**QA Checklist**
- Build: `dotnet build`
- Manual: open the translation dialog and run a single translation to ensure no errors
- No regressions: legacy translator selection remains the default

---

## Issue 43: Translation memory storage path + bootstrap health check
**Description**
Add a lightweight health check that validates the translation memory database path and bootstrap status. Keep the check
local-only and no-op by default, surfacing warnings in logs or status text only when explicitly invoked.

**Acceptance Criteria**
- Health check returns a status enum (Ok/Warning/Error) with a localized message.
- No new database file is created unless the check is explicitly run.
- The check runs against AppData paths only and never attempts network access.

**Files to Touch**
- `Witcher3StringEditor.Common/TranslationMemory/TranslationMemoryHealthStatus.cs`
- `Witcher3StringEditor.Common/TranslationMemory/ITranslationMemoryHealthCheck.cs`
- `Witcher3StringEditor/Services/NoopTranslationMemoryHealthCheck.cs`
- `docs/integrations.md`


**Behavior Impact**
- Default: no runtime behavior change; scaffolding only.
- UI: placeholders only when explicitly called out (no workflow changes).

**Build Commands**
- `dotnet build`

**Reminder (Phase 0)**
- Keep changes compile-safe scaffolding only (stubs/interfaces/TODOs).
- No full feature implementation, no external services, local-only storage.

**QA Checklist**
- Build: `dotnet build`
- Manual: run the health check via a debug harness (or temporary command) and confirm it returns `Warning` when no DB exists
- No regressions: translation dialog behavior unchanged

---

## Issue 44: Translation memory settings mapping in pipeline context (no-op)
**Description**
Extend the translation pipeline context builder to include translation memory enablement flags and resolved database
paths, without changing routing behavior. The resulting context is read-only and unused by default.

**Acceptance Criteria**
- `TranslationPipelineContext` includes `UseTranslationMemory` and `TranslationMemoryPath`.
- Context builder pulls values from settings/profile, with profile overriding settings when set.
- No translation routing changes; context is not consumed yet.

**Files to Touch**
- `Witcher3StringEditor.Common/Translation/TranslationPipelineContext.cs`
- `Witcher3StringEditor/Services/TranslationPipelineContextBuilder.cs`
- `docs/integrations.md`


**Behavior Impact**
- Default: no runtime behavior change; scaffolding only.
- UI: placeholders only when explicitly called out (no workflow changes).

**Build Commands**
- `dotnet build`

**Reminder (Phase 0)**
- Keep changes compile-safe scaffolding only (stubs/interfaces/TODOs).
- No full feature implementation, no external services, local-only storage.

**QA Checklist**
- Build: `dotnet build`
- Manual: verify the context builder returns defaults when settings are empty
- No regressions: legacy translator flow unchanged

---

## Issue 45: Ollama model refresh metadata + cache policy
**Description**
Add metadata fields to store the last model refresh time and cache policy (manual vs timed refresh). Keep the refresh
logic inert by default and only invoked on explicit user action in settings.

**Acceptance Criteria**
- App settings include `TranslationModelCacheUpdatedUtc` and `TranslationModelCachePolicy`.
- The settings dialog shows read-only metadata (timestamp + policy).
- Refresh logic remains a stub and is only called by an explicit command.

**Files to Touch**
- `Witcher3StringEditor.Common/Abstractions/IAppSettings.cs`
- `Witcher3StringEditor/Models/AppSettings.cs`
- `Witcher3StringEditor.Dialogs/ViewModels/SettingsDialogViewModel.cs`
- `Witcher3StringEditor.Dialogs/Views/SettingsDialog.xaml`
- `docs/integrations.md`


**Behavior Impact**
- Default: no runtime behavior change; scaffolding only.
- UI: placeholders only when explicitly called out (no workflow changes).

**Build Commands**
- `dotnet build`

**Reminder (Phase 0)**
- Keep changes compile-safe scaffolding only (stubs/interfaces/TODOs).
- No full feature implementation, no external services, local-only storage.

**QA Checklist**
- Build: `dotnet build`
- Manual: open Settings dialog and confirm metadata placeholders render
- No regressions: translation dialog still loads existing translators

---

## Issue 46: Provider request options mapping (Ollama settings → provider request)
**Description**
Define a minimal mapping from provider-specific settings (Ollama base URL, model, parameters) into a provider request
options object. The mapping must be purely structural and not perform any HTTP calls.

**Acceptance Criteria**
- A `TranslationProviderRequestOptions` model exists with provider name, model, and parameter bag.
- A mapper converts settings/profile data into options for the provider call.
- No network calls; options are unused until routing is wired.

**Files to Touch**
- `Witcher3StringEditor.Common/Translation/TranslationProviderRequestOptions.cs`
- `Witcher3StringEditor.Integrations.Ollama/OllamaSettings.cs`
- `Witcher3StringEditor/Services/TranslationProviderRequestOptionsMapper.cs`
- `docs/integrations.md`


**Behavior Impact**
- Default: no runtime behavior change; scaffolding only.
- UI: placeholders only when explicitly called out (no workflow changes).

**Build Commands**
- `dotnet build`

**Reminder (Phase 0)**
- Keep changes compile-safe scaffolding only (stubs/interfaces/TODOs).
- No full feature implementation, no external services, local-only storage.

**QA Checklist**
- Build: `dotnet build`
- Manual: run a small debug mapping call and verify values are copied
- No regressions: translation flow unchanged

---

## Issue 47: Terminology/style injection context stub
**Description**
Add a small context object that bundles terminology pack and style guide data for future prompt injection. Keep it
unused by default and only created when enablement flags are true.

**Acceptance Criteria**
- `TerminologyInjectionContext` includes terminology entries + style guide content.
- Builder returns `null` when both enablement flags are disabled.
- No prompt injection or post-processing logic is added.

**Files to Touch**
- `Witcher3StringEditor.Common/Terminology/TerminologyInjectionContext.cs`
- `Witcher3StringEditor/Services/TerminologyInjectionContextBuilder.cs`
- `docs/integrations.md`


**Behavior Impact**
- Default: no runtime behavior change; scaffolding only.
- UI: placeholders only when explicitly called out (no workflow changes).

**Build Commands**
- `dotnet build`

**Reminder (Phase 0)**
- Keep changes compile-safe scaffolding only (stubs/interfaces/TODOs).
- No full feature implementation, no external services, local-only storage.

**QA Checklist**
- Build: `dotnet build`
- Manual: toggle terminology/style flags and confirm builder returns null when disabled
- No regressions: translation flow unchanged

---

## Issue 48: Translation profile effective settings preview
**Description**
Add a read-only preview panel in the Settings dialog that shows the effective provider/model/terminology/tm values
after merging the selected profile with app settings. The preview is informational only and does not change behavior.

**Acceptance Criteria**
- Preview fields show the resolved provider/model/base URL and terminology/style paths.
- Preview indicates when profile overrides are absent (falls back to settings).
- No routing or settings changes; view-only.

**Files to Touch**
- `Witcher3StringEditor.Dialogs/Views/SettingsDialog.xaml`
- `Witcher3StringEditor.Dialogs/ViewModels/SettingsDialogViewModel.cs`
- `Witcher3StringEditor/Services/TranslationProfileResolver.cs`
- `docs/integrations.md`


**Behavior Impact**
- Default: no runtime behavior change; scaffolding only.
- UI: placeholders only when explicitly called out (no workflow changes).

**Build Commands**
- `dotnet build`

**Reminder (Phase 0)**
- Keep changes compile-safe scaffolding only (stubs/interfaces/TODOs).
- No full feature implementation, no external services, local-only storage.

**QA Checklist**
- Build: `dotnet build`
- Manual: open Settings dialog and verify preview fields render
- No regressions: translation dialog still opens and translators list intact

---

## Issue 49: Profile-aware provider/model override in translation router (no-op)
**Description**
Allow the translation router to read provider/model overrides from the pipeline context (profile-aware) while keeping
behavior unchanged unless a profile is explicitly selected. The router should still fall back to legacy translators by
default.

**Acceptance Criteria**
- Router checks context-derived provider/model overrides after request-level overrides.
- If no profile is selected, router behavior is unchanged.
- Default settings still use legacy translator path.

**Files to Touch**
- `Witcher3StringEditor.Common/Translation/ITranslationRouter.cs`
- `Witcher3StringEditor/Services/TranslationRouter.cs`
- `Witcher3StringEditor/Services/TranslationPipelineContextBuilder.cs`
- `docs/integrations.md`


**Behavior Impact**
- Default: no runtime behavior change; scaffolding only.
- UI: placeholders only when explicitly called out (no workflow changes).

**Build Commands**
- `dotnet build`

**Reminder (Phase 0)**
- Keep changes compile-safe scaffolding only (stubs/interfaces/TODOs).
- No full feature implementation, no external services, local-only storage.

**QA Checklist**
- Build: `dotnet build`
- Manual: verify no behavior change when no profile is selected
- No regressions: translation dialog still opens and translators list intact

---

## Issue 56: Translation memory SQLite store (exact-match implementation)
**Description**
Implement the translation memory store against the local SQLite schema, supporting exact-match lookups and inserts only.
Keep the service opt-in via the existing enablement flag so default behavior remains unchanged.

**Acceptance Criteria**
- SQLite-backed store implements exact-match lookup + save operations (no fuzzy matching).
- Store uses AppData paths and does not create files unless the feature is enabled.
- The translation memory service can delegate to the SQLite store when enabled.
- No changes to translation behavior when translation memory is disabled.

**Files to Touch**
- `Witcher3StringEditor.Common/TranslationMemory/*`
- `Witcher3StringEditor.Data/TranslationMemory/*`
- `Witcher3StringEditor/Services/TranslationMemoryService.cs`
- `Witcher3StringEditor/App.xaml.cs`
- `docs/integrations.md`


**Behavior Impact**
- Default: no runtime behavior change; scaffolding only.
- UI: placeholders only when explicitly called out (no workflow changes).

**Build Commands**
- `dotnet build`

**Reminder (Phase 0)**
- Keep changes compile-safe scaffolding only (stubs/interfaces/TODOs).
- No full feature implementation, no external services, local-only storage.

**QA Checklist**
- Build: `dotnet build`
- Manual: enable translation memory and run a single translation; verify a database file is created locally
- Manual: re-run the same translation and confirm it pulls from cache
- No regressions: translation dialog still opens and legacy translators list intact

---

## Issue 57: Ollama translation provider execution (model selection)
**Description**
Implement the Ollama provider execution path with model selection and request mapping. Keep network calls behind explicit
user action and ensure the legacy translator path remains default.

**Acceptance Criteria**
- Ollama provider accepts model selection from settings or request overrides.
- Request options include base URL, model, and parameters.
- Translation router can route to Ollama when explicitly selected.
- No behavioral changes when Ollama is not selected.

**Files to Touch**
- `Witcher3StringEditor.Integrations.Ollama/*`
- `Witcher3StringEditor.Common/Translation/*`
- `Witcher3StringEditor/Services/TranslationProviderRequestOptionsMapper.cs`
- `Witcher3StringEditor/Services/TranslationRouter.cs`
- `docs/integrations.md`


**Behavior Impact**
- Default: no runtime behavior change; scaffolding only.
- UI: placeholders only when explicitly called out (no workflow changes).

**Build Commands**
- `dotnet build`

**Reminder (Phase 0)**
- Keep changes compile-safe scaffolding only (stubs/interfaces/TODOs).
- No full feature implementation, no external services, local-only storage.

**QA Checklist**
- Build: `dotnet build`
- Manual: select Ollama in Settings (if available) and confirm model selection persists
- Manual: run a translation and confirm it routes to Ollama only when explicitly selected
- No regressions: legacy translator selection remains the default

---

## Issue 58: Terminology/style injection and validation wiring (opt-in)
**Description**
Wire terminology and style guide data into the prompt builder and validation pipeline when explicitly enabled. Keep
behavior unchanged when enablement flags are off.

**Acceptance Criteria**
- Prompt builder consumes `TerminologyInjectionContext` when enabled.
- Validation service runs after loading terminology/style inputs and surfaces status in the UI.
- No translation output changes when terminology/style is disabled.

**Files to Touch**
- `Witcher3StringEditor.Common/Terminology/*`
- `Witcher3StringEditor/Services/TerminologyInjectionContextBuilder.cs`
- `Witcher3StringEditor/Services/TerminologyPromptBuilder.cs`
- `Witcher3StringEditor/Services/TerminologyValidationService.cs`
- `Witcher3StringEditor.Dialogs/ViewModels/SettingsDialogViewModel.cs`
- `docs/integrations.md`


**Behavior Impact**
- Default: no runtime behavior change; scaffolding only.
- UI: placeholders only when explicitly called out (no workflow changes).

**Build Commands**
- `dotnet build`

**Reminder (Phase 0)**
- Keep changes compile-safe scaffolding only (stubs/interfaces/TODOs).
- No full feature implementation, no external services, local-only storage.

**QA Checklist**
- Build: `dotnet build`
- Manual: enable terminology/style and load sample files; confirm status displays and no crash
- Manual: disable terminology/style and confirm no prompt injection occurs
- No regressions: translation dialog still opens and translators list intact

---

## Issue 59: Translation profile management UI + persistence wiring
**Description**
Add UI and persistence wiring to create, edit, and delete translation profiles stored locally. Keep profile usage
opt-in and ensure the selected profile can be cleared to restore default settings.

**Acceptance Criteria**
- Profiles can be created, edited, and deleted in Settings.
- Changes persist to the local JSON profile store.
- Selecting a profile updates the effective settings preview (read-only).
- Clearing the selected profile reverts to app settings without behavior changes.

**Files to Touch**
- `Witcher3StringEditor.Common/Profiles/*`
- `Witcher3StringEditor.Data/Profiles/*`
- `Witcher3StringEditor/Services/TranslationProfileResolver.cs`
- `Witcher3StringEditor.Dialogs/ViewModels/SettingsDialogViewModel.cs`
- `Witcher3StringEditor.Dialogs/Views/SettingsDialog.xaml`
- `docs/integrations.md`


**Behavior Impact**
- Default: no runtime behavior change; scaffolding only.
- UI: placeholders only when explicitly called out (no workflow changes).

**Build Commands**
- `dotnet build`

**Reminder (Phase 0)**
- Keep changes compile-safe scaffolding only (stubs/interfaces/TODOs).
- No full feature implementation, no external services, local-only storage.

**QA Checklist**
- Build: `dotnet build`
- Manual: create/edit/delete a profile and confirm the JSON store updates
- Manual: select/clear a profile and confirm the effective settings preview updates
- No regressions: translation dialog still opens and legacy translators list intact

---

## Issue 61: Provider routing opt-in gating (toggle enforcement)
**Description**
Update the translation router to honor the `UseProviderForTranslation` toggle so provider routing only occurs when the
user explicitly opts in. Keep the legacy translator path as the default.

**Acceptance Criteria**
- `TranslationRouter` checks `UseProviderForTranslation` before attempting provider resolution.
- When the toggle is off, the router always uses the legacy translator path even if provider settings are present.
- No behavior changes for users who do not enable the toggle.

**Files to Touch**
- `Witcher3StringEditor.Common/Translation/ITranslationRouter.cs`
- `Witcher3StringEditor/Services/TranslationRouter.cs`
- `Witcher3StringEditor.Dialogs/ViewModels/TranslationViewModelBase.cs` (if request shaping needs to be adjusted)
- `docs/integrations.md`


**Behavior Impact**
- Default: no runtime behavior change; scaffolding only.
- UI: placeholders only when explicitly called out (no workflow changes).

**Build Commands**
- `dotnet build`

**Reminder (Phase 0)**
- Keep changes compile-safe scaffolding only (stubs/interfaces/TODOs).
- No full feature implementation, no external services, local-only storage.

**QA Checklist**
- Build: `dotnet build`
- Manual: toggle provider routing on/off in the translation dialog and confirm behavior remains unchanged when off
- No regressions: legacy translator selection remains the default path

---

## Issue 62: Provider selection validation step (pre-flight)
**Description**
Add a pre-flight validation step that checks provider routing inputs (provider name, model, base URL, and cached model
list) before invoking the provider. The validation should be inert by default and only gate provider routing when the
opt-in toggle is enabled.

**Acceptance Criteria**
- A validation result model exists with status + message fields for provider routing readiness.
- The router (or a validation helper) performs validation before provider execution when opt-in routing is enabled.
- Validation failures surface a clear warning and fall back to legacy translation without throwing.
- No network calls or behavioral changes when routing is disabled.

**Files to Touch**
- `Witcher3StringEditor.Common/Translation/TranslationProviderValidationResult.cs`
- `Witcher3StringEditor.Common/Translation/ITranslationProviderValidator.cs`
- `Witcher3StringEditor/Services/NoopTranslationProviderValidator.cs`
- `Witcher3StringEditor/Services/TranslationRouter.cs`
- `docs/integrations.md`


**Behavior Impact**
- Default: no runtime behavior change; scaffolding only.
- UI: placeholders only when explicitly called out (no workflow changes).

**Build Commands**
- `dotnet build`

**Reminder (Phase 0)**
- Keep changes compile-safe scaffolding only (stubs/interfaces/TODOs).
- No full feature implementation, no external services, local-only storage.

**QA Checklist**
- Build: `dotnet build`
- Manual: enable provider routing and confirm validation warnings appear when provider/model is missing
- No regressions: legacy translator selection remains the default path

---

## Issue 63: Translation dialog provider validation status line
**Description**
Expose provider validation status in the translation dialog so users can see readiness before routing. This should be
read-only and should not change behavior unless routing is explicitly enabled.

**Acceptance Criteria**
- Translation dialog shows a status line for provider validation results (ok/warning/error).
- Status updates when provider/model/base URL settings change.
- No routing behavior changes; the status is informational only.

**Files to Touch**
- `Witcher3StringEditor.Dialogs/ViewModels/TranslationDialogViewModel.cs`
- `Witcher3StringEditor.Dialogs/Views/TranslationDialog.xaml`
- `docs/integrations.md`


**Behavior Impact**
- Default: no runtime behavior change; scaffolding only.
- UI: placeholders only when explicitly called out (no workflow changes).

**Build Commands**
- `dotnet build`

**Reminder (Phase 0)**
- Keep changes compile-safe scaffolding only (stubs/interfaces/TODOs).
- No full feature implementation, no external services, local-only storage.

**QA Checklist**
- Build: `dotnet build`
- Manual: open translation dialog and confirm validation status updates as settings change
- No regressions: legacy translator selection remains the default path

---

## Issue 64: Provider routing toggle + validation QA checklist
**Description**
Define a QA checklist that explicitly covers opt-in routing and validation behaviors, including fallbacks and error
states, so future implementations do not regress the legacy translator flow.

**Acceptance Criteria**
- A QA checklist file documents build + manual checks for routing toggle and validation behavior.
- Checklist covers on/off toggle states, missing provider/model scenarios, and fallback to legacy translators.
- Checklist calls out that routing must remain opt-in and inert by default.

**Files to Touch**
- `docs/qa/provider-routing-validation.md`
- `docs/integrations.md`


**Behavior Impact**
- Default: no runtime behavior change; scaffolding only.
- UI: placeholders only when explicitly called out (no workflow changes).

**Build Commands**
- `dotnet build`

**Reminder (Phase 0)**
- Keep changes compile-safe scaffolding only (stubs/interfaces/TODOs).
- No full feature implementation, no external services, local-only storage.

**QA Checklist**
- Build: `dotnet build`
- Manual: review checklist for completeness and clarity
- No regressions: documentation-only change

---

## Issue 66: Translation mode selector (legacy vs provider)
**Description**
Expose a read-only translation mode selector in the translation dialog so users can see whether the flow is using the
legacy translator or provider routing. This selector reflects the opt-in routing toggle but does not change behavior.

**Acceptance Criteria**
- Translation dialog shows a mode selector with **Legacy** as the default state.
- The selector updates to **Provider** only when opt-in routing is enabled.
- Selector is informational and does not change routing behavior.

**Files to Touch**
- `Witcher3StringEditor.Dialogs/Views/TranslationDialog.xaml`
- `Witcher3StringEditor.Dialogs/ViewModels/TranslationDialogViewModel.cs`
- `Witcher3StringEditor.Dialogs/ViewModels/TranslationViewModelBase.cs`
- `docs/integrations.md`


**Behavior Impact**
- Default: no runtime behavior change; scaffolding only.
- UI: placeholders only when explicitly called out (no workflow changes).

**Build Commands**
- `dotnet build`

**Reminder (Phase 0)**
- Keep changes compile-safe scaffolding only (stubs/interfaces/TODOs).
- No full feature implementation, no external services, local-only storage.

**QA Checklist**
- Build: `dotnet build`
- Manual: open Translation dialog and confirm Legacy/Provider indicator updates with the routing toggle
- No regressions: legacy translator selection remains the default path

---

## Issue 67: Provider readiness banner (status summary)
**Description**
Add a read-only readiness banner in the translation dialog that summarizes provider configuration (provider name,
model, base URL, validation status). This banner should be informational only and must not alter routing.

**Acceptance Criteria**
- Banner displays provider name/model/base URL summary from settings.
- Banner shows validation status (ok/warning/error) without enabling routing.
- Banner updates when provider settings change.

**Files to Touch**
- `Witcher3StringEditor.Dialogs/Views/TranslationDialog.xaml`
- `Witcher3StringEditor.Dialogs/ViewModels/TranslationDialogViewModel.cs`
- `Witcher3StringEditor/Services/*TranslationProviderValidator*.cs`
- `docs/integrations.md`


**Behavior Impact**
- Default: no runtime behavior change; scaffolding only.
- UI: placeholders only when explicitly called out (no workflow changes).

**Build Commands**
- `dotnet build`

**Reminder (Phase 0)**
- Keep changes compile-safe scaffolding only (stubs/interfaces/TODOs).
- No full feature implementation, no external services, local-only storage.

**QA Checklist**
- Build: `dotnet build`
- Manual: update provider settings and confirm banner text updates
- No regressions: translation dialog still defaults to legacy translators

---

## Issue 68: Provider fallback status messaging
**Description**
Surface a non-blocking status message in the translation dialog when provider routing falls back to the legacy
translator. The message should include provider/model and a short reason for the fallback.

**Acceptance Criteria**
- Status line appears when a provider failure or validation warning triggers a fallback.
- Message includes provider/model and failure reason, without blocking translation results.
- No routing behavior changes when provider routing is disabled.

**Files to Touch**
- `Witcher3StringEditor.Dialogs/ViewModels/TranslationViewModelBase.cs`
- `Witcher3StringEditor.Dialogs/ViewModels/TranslationDialogViewModel.cs`
- `Witcher3StringEditor.Dialogs/Views/TranslationDialog.xaml`
- `docs/integrations.md`


**Behavior Impact**
- Default: no runtime behavior change; scaffolding only.
- UI: placeholders only when explicitly called out (no workflow changes).

**Build Commands**
- `dotnet build`

**Reminder (Phase 0)**
- Keep changes compile-safe scaffolding only (stubs/interfaces/TODOs).
- No full feature implementation, no external services, local-only storage.

**QA Checklist**
- Build: `dotnet build`
- Manual: simulate a provider validation warning and confirm fallback message displays
- No regressions: legacy translator selection remains default when routing is disabled

---

## Issue 69: Legacy default routing guardrails (documentation + QA)
**Description**
Add a dedicated QA checklist that verifies the legacy translator path remains the default, even when provider settings
are configured. This should document checks for translation mode selector state, readiness banner state, and fallback
behavior messaging.

**Acceptance Criteria**
- New QA checklist documents build + manual steps for legacy-default guardrails.
- Checklist calls out translation mode selector, readiness banner, and fallback status messaging.
- No runtime behavior changes.

**Files to Touch**
- `docs/qa/legacy-default-routing.md`
- `docs/integrations.md`


**Behavior Impact**
- Default: no runtime behavior change; scaffolding only.
- UI: placeholders only when explicitly called out (no workflow changes).

**Build Commands**
- `dotnet build`

**Reminder (Phase 0)**
- Keep changes compile-safe scaffolding only (stubs/interfaces/TODOs).
- No full feature implementation, no external services, local-only storage.

**QA Checklist**
- Build: `dotnet build`
- Manual: review checklist for completeness and clarity
- No regressions: documentation-only change

---

## Issue 71: Translation memory import/export stubs (local-only)
**Description**
Add inert-by-default interfaces for translation memory import/export so future tooling can move data between local
SQLite and portable files without changing runtime behavior.

**Acceptance Criteria**
- `ITranslationMemoryImportService` and `ITranslationMemoryExportService` interfaces exist with no-op implementations.
- Import/export operations are local-only (no network access) and remain inert by default.
- A placeholder DTO for import/export metadata exists (file path, row counts, timestamps).
- No changes to translation flow or settings behavior.

**Files to Touch**
- `Witcher3StringEditor.Common/TranslationMemory/*`
- `Witcher3StringEditor/Services/*TranslationMemoryImportService*.cs`
- `Witcher3StringEditor/Services/*TranslationMemoryExportService*.cs`
- `docs/integrations.md`


**Behavior Impact**
- Default: no runtime behavior change; scaffolding only.
- UI: placeholders only when explicitly called out (no workflow changes).

**Build Commands**
- `dotnet build`

**Reminder (Phase 0)**
- Keep changes compile-safe scaffolding only (stubs/interfaces/TODOs).
- No full feature implementation, no external services, local-only storage.

**QA Checklist**
- Build: `dotnet build`
- Manual: confirm no new UI or runtime behavior changes are visible
- No regressions: translation dialog still opens and legacy translators remain default

---

## Issue 72: Ollama model selection source-of-truth mapping (inert by default)
**Description**
Define a single, inert-by-default mapping between app settings, profile overrides, and the effective Ollama model
selection so future routing can read it without changing current behavior.

**Acceptance Criteria**
- A `TranslationModelSelection` read-only model exists with provider name, model, and base URL fields.
- A resolver returns the effective selection from app settings and selected profile overrides (profile wins).
- No routing changes; existing translator selection remains the default.
- A TODO note documents where routing will consume the selection later.

**Files to Touch**
- `Witcher3StringEditor.Common/Translation/*`
- `Witcher3StringEditor/Services/*TranslationModelSelectionResolver*.cs`
- `docs/integrations.md`


**Behavior Impact**
- Default: no runtime behavior change; scaffolding only.
- UI: placeholders only when explicitly called out (no workflow changes).

**Build Commands**
- `dotnet build`

**Reminder (Phase 0)**
- Keep changes compile-safe scaffolding only (stubs/interfaces/TODOs).
- No full feature implementation, no external services, local-only storage.

**QA Checklist**
- Build: `dotnet build`
- Manual: verify resolver returns null or defaults when no profile is selected
- No regressions: translation dialog still opens and legacy translators remain default

---

## Issue 73: Terminology/style references in translation profiles (settings-only)
**Description**
Extend translation profile models with terminology/style references and enablement toggles, keeping behavior inert by
default and unused until wired into translation routing.

**Acceptance Criteria**
- Profile model includes optional terminology pack path, style guide path, and enablement flags.
- Profile store serializes/deserializes the new fields without breaking existing profiles.
- No changes to translation output or routing (inert by default).

**Files to Touch**
- `Witcher3StringEditor.Common/Profiles/*`
- `Witcher3StringEditor.Data/Profiles/*`
- `docs/integrations.md`


**Behavior Impact**
- Default: no runtime behavior change; scaffolding only.
- UI: placeholders only when explicitly called out (no workflow changes).

**Build Commands**
- `dotnet build`

**Reminder (Phase 0)**
- Keep changes compile-safe scaffolding only (stubs/interfaces/TODOs).
- No full feature implementation, no external services, local-only storage.

**QA Checklist**
- Build: `dotnet build`
- Manual: load existing profile JSON and confirm no crash
- No regressions: translation dialog still opens and legacy translators remain default

---

## Issue 74: Translation profile activation guardrails (inert by default)
**Description**
Add a no-op guardrail service that validates profile activation state (selected vs. inactive) and surfaces a warning
message without changing behavior unless a future UI explicitly consumes it.

**Acceptance Criteria**
- `ITranslationProfileActivationGuard` exists and returns a warning when a profile is selected but missing required fields.
- Default implementation is inert by default and only returns informational messages.
- No routing, translation, or settings behavior changes.

**Files to Touch**
- `Witcher3StringEditor.Common/Profiles/*`
- `Witcher3StringEditor/Services/*TranslationProfileActivationGuard*.cs`
- `docs/integrations.md`


**Behavior Impact**
- Default: no runtime behavior change; scaffolding only.
- UI: placeholders only when explicitly called out (no workflow changes).

**Build Commands**
- `dotnet build`

**Reminder (Phase 0)**
- Keep changes compile-safe scaffolding only (stubs/interfaces/TODOs).
- No full feature implementation, no external services, local-only storage.

**QA Checklist**
- Build: `dotnet build`
- Manual: confirm no new UI or behavior changes are visible
- No regressions: translation dialog still opens and legacy translators remain default
