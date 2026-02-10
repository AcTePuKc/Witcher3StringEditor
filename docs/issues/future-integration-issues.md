# Future Integrations - GitHub Issue Drafts

The following issue drafts intentionally keep implementation scope small and compile-safe. They focus on local storage (SQLite/JSON), interfaces, and TODO-marked scaffolding.

## Issue 0: Behavior namespace compatibility guard for Dialog XAML
**Description**
Create and maintain a lightweight compatibility check that validates XAML behavior type resolution against `Witcher3StringEditor.Dialogs.Behaviors` and ensures the main app keeps a project reference to `Witcher3StringEditor.Dialogs`.

**Acceptance Criteria**
- The four historical behavior class names remain resolvable to public types (or explicit no-op stubs if renamed later).
- `Witcher3StringEditor.csproj` keeps a direct `ProjectReference` to `Witcher3StringEditor.Dialogs`.
- No runtime logic changes are introduced.

**Files to touch**
- `docs/inspections/behavior-compatibility-check.md`
- `Witcher3StringEditor.Dialogs/Behaviors/*` (only if a compatibility stub is required)
- `Witcher3StringEditor/Views/MainWindow.xaml` (only if tags must be renamed)
- `Witcher3StringEditor/Witcher3StringEditor.csproj` (only if reference is missing)

**QA Checklist**
- Build: solution compiles and XAML loads with no behavior type lookup errors.
- Manual: open MainWindow and a dialog using grid behaviors; verify no startup parse/type errors.
- Regression: behavior changes remain inert/no-op unless explicitly implemented in a future issue.

---

## Issue 1: Inventory pass - confirm extension points for TM, Ollama, terminology/style, and profiles
**Description**
Run a read-only inventory pass to confirm the concrete attachment points before adding behavior. This avoids guessing and prevents misplaced wiring.

**Acceptance Criteria**
- A markdown report captures exact class/file entrypoints for:
  - Translation Memory read/write hooks
  - Ollama model list refresh and persisted model selection
  - Terminology/style file loading entrypoints
  - Translation profile list/preview/selection surfaces
- The report lists settings persistence paths and startup lifecycle constraints.
- No runtime behavior changes are included.

**Files to touch**
- `docs/inspections/integration-entrypoints.md` (new)

**QA Checklist**
- Build: optional (docs-only).
- Manual: reviewer can trace each planned feature to at least one verified class/method.
- Regression: no runtime changes expected.

---

## Issue 2: Translation Memory local database scaffolding (SQLite, disabled by default)
**Description**
Add/align compile-safe scaffolding for local SQLite-backed translation memory. Do not wire writes/reads into active translation flow yet.

**Acceptance Criteria**
- Local SQLite schema notes exist (table + index plan).
- `Common` contracts remain the single source of truth for TM DTOs/interfaces.
- `Data` contains inert store/bootstrap stubs with TODO markers.
- Runtime behavior remains unchanged unless TM is explicitly enabled in a future issue.

**Files to touch**
- `docs/integrations/translation-memory-schema.md`
- `Witcher3StringEditor.Common/TranslationMemory/*`
- `Witcher3StringEditor.Data/TranslationMemory/*`
- `Witcher3StringEditor/Services/*TranslationMemory*`

**QA Checklist**
- Build: solution compiles.
- Manual: instantiate TM service/store classes from DI without startup errors.
- Regression: existing translation workflow still runs with no TM dependency.

---

## Issue 3: Ollama integration with model selection scaffolding (user-triggered refresh only)
**Description**
Add minimal contracts and stubs for model selection persistence and model refresh behavior. Keep all network activity user-triggered from settings actions.

**Acceptance Criteria**
- Model catalog and selection contracts compile.
- Settings can display cached/persisted model choice without forcing network calls on dialog open.
- Refresh/load remains command-triggered; no constructor/open-path HTTP calls.

**Files to touch**
- `Witcher3StringEditor.Common/Translation/*`
- `Witcher3StringEditor.Integrations.Ollama/*`
- `Witcher3StringEditor.Dialogs/ViewModels/SettingsDialogViewModel.cs`
- `Witcher3StringEditor.Dialogs/Views/SettingsDialog.xaml`
- `Witcher3StringEditor/Services/*TranslationModel*`

**QA Checklist**
- Build: solution compiles.
- Manual: opening Settings does not auto-refresh models; clicking Refresh performs the request.
- Regression: existing provider routing remains unchanged when no model is selected.

---

## Issue 4: Terminology/style local loading scaffolding and validation hooks
**Description**
Keep terminology/style support local-file based and add validation hook surfaces. Avoid heavy constructor/open-path work.

**Acceptance Criteria**
- Loader and validator interfaces are local-file only (no external service dependency).
- Parsing/validation work is deferred or user-triggered.
- Settings surfaces expose status placeholders without changing translation behavior.

**Files to touch**
- `Witcher3StringEditor.Common/Terminology/*`
- `Witcher3StringEditor.Data/Terminology/*`
- `Witcher3StringEditor/Services/Terminology*`
- `Witcher3StringEditor.Dialogs/ViewModels/SettingsDialogViewModel.cs`

**QA Checklist**
- Build: solution compiles.
- Manual: open Settings and trigger terminology/style refresh without UI freeze.
- Regression: translation output flow unchanged when no terminology/style file is configured.

---

## Issue 5: Translation profiles local persistence and preview scaffolding
**Description**
Keep profile listing/preview local (JSON) and inert by default. Add only compile-safe contracts/stubs for catalog, resolver, and preview.

**Acceptance Criteria**
- Profile contracts/models live in `Common` and compile.
- JSON-backed `Data` scaffolding exists with TODO markers.
- No profile is auto-applied unless explicitly selected by user action.

**Files to touch**
- `Witcher3StringEditor.Common/Profiles/*`
- `Witcher3StringEditor.Data/Profiles/*`
- `Witcher3StringEditor/Services/*Profile*`
- `Witcher3StringEditor.Dialogs/ViewModels/SettingsDialogViewModel.cs`

**QA Checklist**
- Build: solution compiles.
- Manual: settings profile list/preview loads without blocking window open.
- Regression: default translation behavior unchanged when no profile is selected.

---

## Issue 6: Constructor audit and deferred-load guardrails for Settings surfaces
**Description**
Audit constructors and startup paths for hidden file/network/profile/model loading. Ensure Settings constructor remains lightweight and all expensive work is command-triggered.

**Acceptance Criteria**
- Audit report exists listing constructor/startup paths and load triggers.
- Settings constructor performs no model/profile/terminology/style loading work.
- Explicit commands exist for all deferred settings loads.
- Default status text remains `Not loaded yet.` until the user triggers a load.

**Files to touch**
- `docs/inspections/constructor-loading-audit.md`
- `Witcher3StringEditor.Dialogs/ViewModels/SettingsDialogViewModel.cs`
- `Witcher3StringEditor.Dialogs/Views/SettingsDialog.xaml`

**QA Checklist**
- Build: solution compiles.
- Manual: open Settings; verify status panels read `Not loaded yet.` with no background refresh.
- Manual: click load/refresh actions; verify statuses change only after user action.
- Regression: opening Settings remains responsive and does not trigger unexpected network calls.

---

## Issue 7: Deferred settings-load service scaffolding (interface + no-op implementation)
**Description**
Add compile-safe interface and no-op service to centralize future command-triggered loading behavior without wiring runtime features yet.

**Acceptance Criteria**
- New shared interface defines explicit deferred-load methods for cached models, profiles, terminology, and style guide.
- No-op implementation compiles and returns inert results.
- No behavior change unless future issues wire the service into view models.

**Files to touch**
- `Witcher3StringEditor.Common/Settings/ISettingsDeferredLoadService.cs`
- `Witcher3StringEditor/Services/NoopSettingsDeferredLoadService.cs`

**QA Checklist**
- Build: solution compiles.
- Manual: no visible behavior changes expected.
- Regression: no startup/runtime load regressions.
