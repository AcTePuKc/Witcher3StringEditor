# Integration Backlog (GitHub Issue Drafts)

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
- Manual: open Settings dialog and confirm no UI regressions
- No regressions: translation dialog still opens

---

## Issue 2: Translation memory + QA stores (SQLite scaffolding)
**Description**
Ensure local translation memory and QA stores use a minimal SQLite schema and AppData-based storage paths. Keep operations to upsert + exact-match lookup only.

**Acceptance Criteria**
- SQLite bootstrap and schemas exist for translation memory + QA.
- Store interfaces remain local-only and inert by default.
- Storage uses AppData paths only.

**Files to Touch**
- `Witcher3StringEditor.Common/TranslationMemory/*`
- `Witcher3StringEditor.Common/QualityAssurance/*`
- `Witcher3StringEditor.Data/Storage/*`
- `Witcher3StringEditor.Data/TranslationMemory/*`
- `Witcher3StringEditor.Data/QualityAssurance/*`
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
- Profile model includes optional terminology/style paths and translation memory enablement.
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

**QA Checklist**
- Build: `dotnet build`
- Manual: launch app and open Translation dialog
- No regressions: translation still uses existing translator selection

**Current Status / Partial Completion**
- A context builder exists, but it is not wired into translation routing yet.

---

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
- No regressions: existing translators still listed

**Current Status / Partial Completion**
- Placeholder UI does not exist yet; only issue drafts cover it.

---

## Issue 9: Ollama integration settings + model list stub
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
