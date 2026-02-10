# Integration Entrypoints Inventory (Phase 0)

This inventory is read-only and intended to de-risk future wiring tasks.

## Translation Memory (local SQLite/JSON only)

### Confirmed entrypoints
- Settings source: `Witcher3StringEditor/Models/AppSettings.cs` (`UseTranslationMemory`, `TranslationMemoryPath`).
- Runtime settings projection: `Witcher3StringEditor/Services/TranslationMemorySettingsProvider.cs`.
- Service orchestration surface: `Witcher3StringEditor/Services/TranslationMemoryService.cs`.
- Local storage contracts: `Witcher3StringEditor.Common/TranslationMemory/*`.
- SQLite storage implementation: `Witcher3StringEditor.Data/TranslationMemory/*`.

### Hook points
- Resolve settings and create store via `ITranslationMemoryStoreFactory`.
- Keep read/write hooks behind `ITranslationMemoryService` to avoid touching UI call sites.

### Risks
- Avoid constructor-time DB initialization from Settings dialogs.
- Keep TM disabled/inert when path is not configured.

## Ollama integration + model selection

### Confirmed entrypoints
- Provider stub and settings model: `Witcher3StringEditor.Integrations.Ollama/*`.
- Provider/model surfaces in Settings VM: `Witcher3StringEditor.Dialogs/ViewModels/SettingsDialogViewModel.cs`.
- Settings UI placeholders: `Witcher3StringEditor.Dialogs/Views/SettingsDialog.xaml`.
- Provider routing orchestration: `Witcher3StringEditor/Services/TranslationRouter.cs` and related provider registry services.

### Hook points
- Keep model list refresh command-triggered (`RefreshModelsCommand`).
- Persist selected provider/model/base URL through local settings/local store abstractions.

### Risks
- No automatic model fetch during Settings window construction/open.
- Keep provider fallback behavior unchanged by default.

## Terminology and style loading

### Confirmed entrypoints
- Contracts and models: `Witcher3StringEditor.Common/Terminology/*`.
- Current loader service: `Witcher3StringEditor/Services/TerminologyLoader.cs`.
- No-op validator: `Witcher3StringEditor/Services/NoopTerminologyValidationService.cs`.
- Settings VM/UI status and path commands:
  - `Witcher3StringEditor.Dialogs/ViewModels/SettingsDialogViewModel.cs`
  - `Witcher3StringEditor.Dialogs/Views/SettingsDialog.xaml`

### Hook points
- Keep file selection and refresh actions command-based.
- Defer parsing/validation to explicit refresh actions.

### Risks
- Avoid parsing on `AppSettings` property-changed events.
- Ensure unresolved terminology/style files do not block translation flow.

## Translation profiles

### Confirmed entrypoints
- Contracts and models: `Witcher3StringEditor.Common/Profiles/*`.
- Local JSON store/catalog: `Witcher3StringEditor.Data/Profiles/*`.
- Resolver/settings bridge:
  - `Witcher3StringEditor/Services/TranslationProfileSettingsResolver.cs`
  - `Witcher3StringEditor/Services/TranslationPipelineContextBuilder.cs`
- Settings profile UI placeholders:
  - `Witcher3StringEditor.Dialogs/ViewModels/SettingsDialogViewModel.cs`
  - `Witcher3StringEditor.Dialogs/Views/SettingsDialog.xaml`

### Hook points
- Keep profile list/preview loading behind explicit refresh commands.
- Apply selected profile through resolver abstractions only.

### Risks
- Do not auto-apply profiles unless explicitly selected.
- Keep profile catalog loading non-blocking and local-only.

## Follow-up confirmation task (if needed)

If file ownership changes before implementation, create a dedicated “inventory refresh” issue to re-confirm:
- ownership of `Common` contracts,
- `Data` local persistence implementations,
- and `Dialogs` settings command surfaces.
