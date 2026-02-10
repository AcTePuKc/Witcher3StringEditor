# Settings Dialog Startup Investigation

Date: 2026-02-10
Status: Complete (stabilization documented)

## Observed symptom
Opening the Settings dialog was at risk of becoming progressively slower and less reliable as new integration scaffolding accumulated. The immediate symptom was startup-path fragility: dialog open could eventually surface provider/network errors, file parsing exceptions, or profile I/O failures before the user triggered any refresh action.

## Root cause
The Settings dialog had accumulated multiple extension points (provider models, profiles, terminology, and style guide status) that are all potentially expensive to initialize. The risk was not a single crashing line; it was architectural drift toward constructor-time work and implicit auto-load behavior when the dialog opens.

If that drift continued, opening Settings could trigger network calls (`/api/tags`), filesystem scans/parsing, and profile I/O before the user explicitly requested refresh actions. That startup coupling would make the dialog brittle (slow opens, transient failures shown immediately, and avoidable initialization exceptions).

## Affected methods
The following methods are the stabilization-critical surfaces because they touch I/O or long-running work:

- `SettingsDialogViewModel.RefreshModels()` (network call to provider endpoint, model cache updates).
- `SettingsDialogViewModel.RefreshTranslationProfiles()` -> `LoadTranslationProfilesAsync()` (profile catalog reads).
- `SettingsDialogViewModel.RefreshTerminologyStatus()` -> `UpdateTerminologyStatusAsync()` (filesystem checks + terminology parsing/validation).
- `SettingsDialogViewModel.RefreshStyleGuideStatus()` -> `UpdateStyleGuideStatusAsync()` (filesystem checks + style guide parsing/validation).
- `MainWindowViewModel.ShowSettingsDialog()` (dialog creation path that must remain lightweight and non-blocking).

## File-level fix summary
The stabilization fix was implemented as a contract-level change across view-model startup surfaces and docs:

- `Witcher3StringEditor/ViewModels/Dialogs/SettingsDialogViewModel.cs`: preserve lightweight constructor behavior and keep expensive work behind explicit refresh/test commands.
- `Witcher3StringEditor/ViewModels/MainWindowViewModel.cs`: keep `ShowSettingsDialog()` focused on dialog creation/open only.
- `docs/integrations.md`: codify startup rules (no auto-refresh on open, explicit user-triggered loading).
- `docs/inspections/scaffold-progress.md`: track this as completed checklist item P0-36.

## Final mitigation
Stabilization was finalized as a non-blocking Settings startup contract:

1. **Constructor remains lightweight:** no automatic model refresh, profile scan, terminology/style loading, or DB bootstrap on dialog open.
2. **Explicit placeholder states:** status panels initialize to `Not loaded yet.` for model/profile/terminology/style sections.
3. **User-triggered refresh only:** expensive work is executed only from explicit Refresh/Test actions.
4. **File-picker behavior constrained:** selecting files only updates paths; parsing/validation remains action-triggered.
5. **Documentation lock-in:** the startup contract is recorded in `docs/integrations.md` and tracked as a completed stabilization item in `docs/inspections/scaffold-progress.md`.

This mitigation keeps current scaffolding safe while preserving future extensibility for provider/model selection, translation memory, terminology/style loading, and translation profiles.

## Do not do this again
- Do not start network calls from constructor/open-path code (e.g., model list fetches such as `/api/tags`).
- Do not auto-refresh models/profiles/terminology/style state when the dialog opens.
- Do not perform file parsing/validation during file selection; keep that work explicit and command-triggered.
- Do not let constructor-time work block UI thread responsiveness.
- Do not introduce background startup tasks that mutate settings state before users request refresh/test.
