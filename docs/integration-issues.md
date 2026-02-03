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
