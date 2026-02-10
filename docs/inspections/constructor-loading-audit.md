# Constructor Loading Audit (File/Network/Profile/Model)

## Scope
Inventory pass focused on constructors and startup paths that can trigger file I/O, network I/O, profile reads, or model-loading work.

## Findings

### 1) `SettingsDialogViewModel` constructor
- **File:** `Witcher3StringEditor.Dialogs/ViewModels/SettingsDialogViewModel.cs`
- **Previous behavior:** hydrated model options from cached settings during construction.
- **Current behavior:** constructor performs lightweight dependency assignment + status initialization only.
- **Current default statuses:** `ModelStatusText`, `ProfileStatusText`, `SelectedProfilePreview`, `TerminologyStatusText`, `StyleGuideStatusText`, `ProviderConnectionStatusText` are initialized to `Not loaded yet.`
- **Deferred operations (explicit commands only):**
  - `LoadCachedModelsCommand` (local cache only)
  - `RefreshModelsCommand` (network call to Ollama)
  - `RefreshTranslationProfilesCommand` (profile catalog)
  - `RefreshTerminologyStatusCommand` (file validation/load)
  - `RefreshStyleGuideStatusCommand` (file validation/load)

### 2) `MainWindowViewModel` constructor
- **File:** `Witcher3StringEditor/ViewModels/MainWindowViewModel.cs`
- **Behavior:** dependency resolution and message registration only.
- **Deferred work:** file-open/update checks happen in commands (`WindowLoaded`, `OpenFile`, etc.), not constructor.

### 3) App startup (`App.InitializeApplication`)
- **File:** `Witcher3StringEditor/App.xaml.cs`
- **Behavior:** expected app bootstrap and settings load; not a dialog constructor.
- **Notes:** this is acceptable startup initialization and outside settings-constructor scope.

## Actionable rule for future changes
- Do not place model refresh, profile loading, terminology/style parsing, or network checks in Settings-related constructors.
- All loading must remain behind explicit user commands/buttons.

