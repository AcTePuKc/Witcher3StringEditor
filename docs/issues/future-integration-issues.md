# Future Integrations - GitHub Issue Drafts

## Issue 1: Inventory pass for integration attachment points
**Description**
Run a read-only inventory pass to confirm exact classes/files where translation memory, provider selection/model selection, terminology/style loading, and translation profile behavior should attach. This issue exists to prevent guessing and avoid misplaced wiring.

**Acceptance Criteria**
- A markdown report lists concrete file paths and class/method entrypoints.
- The report highlights settings persistence paths and dialog/view-model surfaces.
- No production behavior changes are included.

**Files to touch**
- `docs/inventory/integration-entrypoints.md` (new)

**QA Checklist**
- Build: optional (docs-only)
- Manual: review confirms every planned feature has at least one verified attachment point.
- Regression: no runtime changes expected.

---

## Issue 2: Translation memory local database bootstrap (SQLite)
**Description**
Add inert, compile-safe scaffolding for a local database-backed translation memory, including schema/migration notes and store bootstrap interfaces. Keep all wiring disabled by default.

**Acceptance Criteria**
- SQLite schema document exists for TM entries and indexes.
- Store/bootstrap interfaces are present with TODO markers.
- Default runtime behavior remains unchanged (no automatic TM writes/reads).

**Files to touch**
- `docs/integrations/translation-memory-schema.md`
- `Witcher3StringEditor.Common/TranslationMemory/*`
- `Witcher3StringEditor.Data/TranslationMemory/*`

**QA Checklist**
- Build: solution compiles.
- Manual: instantiate the store/bootstrapper in isolation.
- Regression: translation flow is unchanged when TM is disabled.

---

## Issue 3: Ollama model selection integration scaffolding
**Description**
Provide model-selection contract and catalog scaffolding for Ollama, with explicit refresh actions from settings UI and no auto-refresh on dialog open.

**Acceptance Criteria**
- Provider/model DTOs and catalog abstractions compile.
- Settings can show persisted/cached model options without forcing network calls on open.
- Model refresh is user-triggered only.

**Files to touch**
- `Witcher3StringEditor.Integrations.Ollama/*`
- `Witcher3StringEditor.Dialogs/ViewModels/SettingsDialogViewModel.cs`
- `Witcher3StringEditor.Dialogs/Views/SettingsDialog.xaml`

**QA Checklist**
- Build: solution compiles.
- Manual: open settings, verify UI stays responsive and no auto model refresh happens.
- Regression: existing translator selection behavior is unchanged.

---

## Issue 4: Terminology/style loading scaffolding and validation hooks
**Description**
Keep terminology/style support local-file based with minimal loaders and validation hooks. Preserve current behavior while keeping heavier checks out of constructor/open path.

**Acceptance Criteria**
- Loader/validation interfaces remain local-file only.
- Startup/open path avoids blocking synchronous work.
- Status updates are deferred async or user-triggered.

**Files to touch**
- `Witcher3StringEditor.Common/Terminology/*`
- `Witcher3StringEditor.Data/Terminology/*`
- `Witcher3StringEditor.Dialogs/ViewModels/SettingsDialogViewModel.cs`

**QA Checklist**
- Build: solution compiles.
- Manual: open settings; terminology/style status updates complete without UI freeze.
- Regression: toggles and selected paths continue to work.

---

## Issue 5: Translation profile catalog/preview local persistence
**Description**
Keep profile listing/preview local (JSON) and inert by default. Move expensive profile scans out of constructor/open hot path if any are found.

**Acceptance Criteria**
- Local profile catalog/preview abstractions compile.
- Any profile list/preview initialization runs deferred async and non-blocking.
- No profile is auto-applied to translation runtime without explicit user choice.

**Files to touch**
- `Witcher3StringEditor.Common/Profiles/*`
- `Witcher3StringEditor.Data/Profiles/*`
- `Witcher3StringEditor.Dialogs/ViewModels/SettingsDialogViewModel.cs`

**QA Checklist**
- Build: solution compiles.
- Manual: open settings and verify profile status/preview appears without freezing.
- Regression: existing translation behavior is unchanged unless user selects a profile.
