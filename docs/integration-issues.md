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
- Build: `dotnet build`
- Manual: open settings dialog to confirm no UI regressions
- No regressions: verify existing translation dialog still opens

---

## Issue 2: Translation memory + QA stores (SQLite scaffolding)
**Description**
Introduce local translation memory and QA stores with a minimal SQLite schema and bootstrap logic. Use AppData-based storage paths and keep operations to upsert + exact-match lookup.

**Acceptance Criteria**
- `ITranslationMemoryStore` and `IQAStore` implemented with SQLite-backed stubs.
- Schema includes source text, target text, language/issue type, timestamps.
- Storage uses AppData-based file location strategy.
- No UI changes and no runtime behavior change by default.

**Files to Touch**
- `Witcher3StringEditor.Common/TranslationMemory/*`
- `Witcher3StringEditor.Common/QualityAssurance/*`
- `Witcher3StringEditor.Data/*`
- `docs/integrations.md`

**QA Checklist**
- Build: `dotnet build`
- Manual: app startup and open translation dialog
- No regressions: confirm existing translation flow behaves the same

---

## Issue 3: Provider registry + model discovery wiring (Ollama first)
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
- `docs/integrations.md`

**QA Checklist**
- Build: `dotnet build`
- Manual: open settings dialog and click refresh button
- No regressions: confirm translation dialog still loads existing translator list

---

## Issue 4: Terminology + style pack loading
**Description**
Implement loaders for TSV/CSV/Markdown terminology packs and add hooks for future prompt injection and post-translation validation. Validate parsing against the sample TSV and Markdown style guide.

**Acceptance Criteria**
- `TerminologyPack` includes term, translation, notes, and mode metadata.
- `TerminologyLoader` supports CSV/TSV and Markdown parsing with simple rules.
- TODO markers exist for prompt injection and post-translation validation.
- Loader can parse `docs/samples/terminology.sample.tsv` and `docs/samples/style-guide.sample.md` without errors.

**Files to Touch**
- `Witcher3StringEditor.Common/Terminology/*`
- `Witcher3StringEditor/Services/TerminologyLoader.cs`
- `Witcher3StringEditor.Dialogs/ViewModels/*TranslationViewModel*.cs`
- `docs/samples/*`
- `docs/integrations.md`

**QA Checklist**
- Build: `dotnet build`
- Manual: load a sample pack and confirm no crash
- No regressions: confirm translation still works without a terminology pack

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
- No regressions: confirm existing translator selection still functions
