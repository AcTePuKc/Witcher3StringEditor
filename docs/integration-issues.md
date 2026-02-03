# Integration Backlog (GitHub Issue Drafts)

## Issue 1: Inventory pass for integration entrypoints
**Description**
Perform a repository inventory to confirm where settings are loaded/saved, where translation requests flow, and which dialogs or view models should host future integration options.

**Acceptance Criteria**
- Identify settings persistence flow and config file location.
- Identify translation entrypoints and extension points for provider calls.
- Identify UI dialogs/tabs relevant to translation settings.
- Summarize risks and constraints.

**Files to Touch**
- `docs/inventory-report.md`

**QA Checklist**
- Build: `dotnet build` (ensure no changes break compilation)
- Manual: open settings dialog to confirm no UI regressions

---

## Issue 2: Translation memory store (SQLite scaffolding)
**Description**
Introduce a local translation memory store with a minimal SQLite schema and bootstrap logic. Add no-op wiring behind a feature flag or safe default.

**Acceptance Criteria**
- `ITranslationMemoryStore` implemented with SQLite-backed stub.
- Schema includes source text, target text, language pair, timestamps.
- No UI changes and no runtime behavior change by default.

**Files to Touch**
- `Witcher3StringEditor.Common/TranslationMemory/*`
- `Witcher3StringEditor/Services/*`
- `docs/integrations.md`

**QA Checklist**
- Build: `dotnet build`
- Manual: app startup and open translation dialog

---

## Issue 3: Provider registry + model discovery wiring
**Description**
Wire provider selection and model discovery to a registry that can call provider-specific model list methods (Ollama first). Keep existing translator selection intact.

**Acceptance Criteria**
- Provider registry maps provider name to `ITranslationProvider`.
- Settings dialog refresh button calls into registry (stubbed allowed).
- No changes to existing translator selection flow.

**Files to Touch**
- `Witcher3StringEditor.Common/Translation/*`
- `Witcher3StringEditor.Dialogs/ViewModels/SettingDialogViewModel.cs`
- `Witcher3StringEditor.Dialogs/Views/SettingsDialog.xaml`

**QA Checklist**
- Build: `dotnet build`
- Manual: open settings dialog and click refresh button

---

## Issue 4: Terminology + style pack loading
**Description**
Implement loaders for TSV/CSV/Markdown terminology packs and add hooks for future prompt injection and validation.

**Acceptance Criteria**
- `ITerminologyLoader` implemented for TSV/CSV/Markdown.
- Sample files added under `docs/samples/`.
- No automatic enforcement; only reporting/stubs.

**Files to Touch**
- `Witcher3StringEditor.Common/Terminology/*`
- `Witcher3StringEditor/Services/*`
- `docs/samples/*`

**QA Checklist**
- Build: `dotnet build`
- Manual: load a sample pack and confirm no crash

---

## Issue 5: Translation profile storage
**Description**
Create a local profile store (JSON first) that persists provider/model/base URL/notes to enable profile selection later.

**Acceptance Criteria**
- `ITranslationProfileStore` implemented with JSON-backed persistence.
- Profiles can be listed and resolved by id.
- Settings can reference a profile id without changing existing translator selection.

**Files to Touch**
- `Witcher3StringEditor.Common/Profiles/*`
- `Witcher3StringEditor/Services/*`
- `docs/integrations.md`

**QA Checklist**
- Build: `dotnet build`
- Manual: create a profile file and ensure it loads
