# Integration Backlog (GitHub Issue Drafts)

## Issue 1: Inventory pass for integration entrypoints
**Description**
Confirm where settings are persisted, where translation requests flow, and which dialogs/view models will host provider/model/terminology/profile configuration.

**Acceptance Criteria**
- Document settings persistence flow and config path.
- Identify translation entrypoints and extension points for provider/model selection.
- Identify UI surfaces for settings placeholders.
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

**Files to Touch**
- `Witcher3StringEditor.Common/Translation/*`
- `Witcher3StringEditor/Services/*TranslationProviderRegistry*.cs`
- `Witcher3StringEditor.Integrations.Ollama/*`
- `Witcher3StringEditor.Dialogs/ViewModels/SettingDialogViewModel.cs`
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
- TODO markers exist for future prompt injection/validation.
- Samples under `docs/samples/` parse without errors.

**Files to Touch**
- `Witcher3StringEditor.Common/Terminology/*`
- `Witcher3StringEditor/Services/TerminologyLoader.cs`
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

**QA Checklist**
- Docs: verify links/paths are accurate
- Manual: ensure no missing references in the doc

**Current Status / Partial Completion**
- An initial draft of the integrations spec exists at `docs/integrations.md`, but it needs to be expanded with more detail.

---

## Issue 7: Settings UI placeholders for integrations
**Description**
Add minimal settings UI placeholders for provider selection, model selection, terminology packs, and translation profile selection. No behavioral changes; values can be stored but not used.

**Acceptance Criteria**
- Settings dialog shows placeholders for provider/model selection.
- Terminology pack and profile pickers exist (non-functional allowed).
- Persisted settings are inert by default.

**Files to Touch**
- `Witcher3StringEditor.Dialogs/Views/SettingsDialog.xaml`
- `Witcher3StringEditor.Dialogs/ViewModels/SettingDialogViewModel.cs`
- `Witcher3StringEditor.Common/Settings/*` (if required)
- `docs/integrations.md`

**QA Checklist**
- Build: `dotnet build`
- Manual: open Settings dialog and verify placeholders render
- No regressions: existing translators still listed

**Current Status / Partial Completion**
- Placeholder UI does not exist yet; only issue drafts cover it.

---

## Issue 8: Ollama integration settings + model list stub
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

**QA Checklist**
- Build: `dotnet build`
- Manual: open Settings dialog and confirm no runtime errors
- No regressions: translation flow unchanged

**Current Status / Partial Completion**
- Integration project exists, but no settings model or client stub added yet.

---

## Issue 9: Local translation memory interfaces + SQLite bootstrap
**Description**
Define translation memory interfaces, provide a minimal SQLite schema, and implement a stub repository that performs no lookups unless explicitly enabled.

**Acceptance Criteria**
- SQLite schema defined with source/target/lang fields.
- TM interfaces + stub store compile.
- Storage path uses local AppData.

**Files to Touch**
- `Witcher3StringEditor.Common/TranslationMemory/*`
- `Witcher3StringEditor.Data/TranslationMemory/*`
- `Witcher3StringEditor.Data/Storage/*`
- `docs/integrations.md`

**QA Checklist**
- Build: `dotnet build`
- Manual: app startup without DB side effects
- No regressions: translation dialog behaves as before

**Current Status / Partial Completion**
- No schema or interfaces exist in repo yet.

---

## Issue 10: Terminology + style loading stubs + samples
**Description**
Add terminology/style models, minimal loaders for TSV/CSV/Markdown, and sample files for validation. Keep behavior read-only and inert.

**Acceptance Criteria**
- Loaders can parse TSV/CSV terminology and Markdown style guides.
- Samples added under `docs/samples/`.
- TODO hooks exist for prompt injection/validation.

**Files to Touch**
- `Witcher3StringEditor.Common/Terminology/*`
- `Witcher3StringEditor/Services/TerminologyLoader.cs`
- `docs/samples/*`
- `docs/integrations.md`

**QA Checklist**
- Build: `dotnet build`
- Manual: load a sample pack and confirm no crash
- No regressions: translation still works without a terminology pack

**Current Status / Partial Completion**
- No loaders or sample files exist yet.
