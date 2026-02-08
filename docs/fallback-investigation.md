# Translation Flow + Fallback Investigation

## Scope
This document traces the current translation flows for single-item and batch translation, and lists the fallback/default
selection logic, error handling, and provider/legacy translator routing behavior.

## Single Translation Flow (TranslationDialogViewModel → SingleItemTranslationViewModel → Router)
1. **Entry point**: `MainWindowViewModel.ShowTranslateDialog` constructs the dialog view model.
   - File: `Witcher3StringEditor/ViewModels/MainWindowViewModel.cs`
   - Method: `ShowTranslateDialog`
   - Trigger: user opens the Translation dialog (command) and optional selected item.
   - Behavior: resolves the legacy `ITranslator` by name from `appSettings.Translator` and passes it + `ITranslationRouter`
     into `TranslationDialogViewModel`.
2. **Dialog setup**: `TranslationDialogViewModel` initializes `SingleItemTranslationViewModel` as the default view.
   - File: `Witcher3StringEditor.Dialogs/ViewModels/TranslationDialogViewModel.cs`
   - Method: constructor
   - Trigger: dialog creation.
3. **Translate command**: `SingleItemTranslationViewModel.Translate` invokes the router.
   - File: `Witcher3StringEditor.Dialogs/ViewModels/SingleItemTranslationViewModel.cs`
   - Method: `Translate`
   - Trigger: user presses Translate in single-item mode (and the target field is empty).
   - Behavior: builds a `TranslationRouterRequest` and calls `TranslationRouter.TranslateAsync` via
     `ExecuteTranslationTask`.
4. **Router call**: `ExecuteTranslationTask` waits for router completion or cancellation.
   - File: `Witcher3StringEditor.Dialogs/ViewModels/SingleItemTranslationViewModel.cs`
   - Method: `ExecuteTranslationTask`
   - Trigger: inside `Translate`.

## Batch Translation Flow (BatchItemsTranslationViewModel → Router)
1. **Switch to batch**: `TranslationDialogViewModel.Switch` swaps the current view model.
   - File: `Witcher3StringEditor.Dialogs/ViewModels/TranslationDialogViewModel.cs`
   - Method: `Switch`
   - Trigger: user toggles the translation mode.
2. **Start batch**: `BatchItemsTranslationViewModel.Start` runs the batch workflow.
   - File: `Witcher3StringEditor.Dialogs/ViewModels/BatchItemsTranslationViewModel.cs`
   - Method: `Start`
   - Trigger: user presses Start in batch mode.
3. **Batch loop**: `ProcessTranslationItems` iterates item range and calls `ProcessSingleItem`.
   - File: `Witcher3StringEditor.Dialogs/ViewModels/BatchItemsTranslationViewModel.cs`
   - Method: `ProcessTranslationItems`
   - Trigger: `ExecuteBatchTranslation` after range setup.
4. **Router call**: `TranslateItem` sends each string through `TranslationRouter.TranslateAsync`.
   - File: `Witcher3StringEditor.Dialogs/ViewModels/BatchItemsTranslationViewModel.cs`
   - Method: `TranslateItem`
   - Trigger: per item in the batch loop.

## Translator / Provider Invocation Details
- **Router interface**: `ITranslationRouter.TranslateAsync` accepts a `TranslationRouterRequest`.
  - File: `Witcher3StringEditor.Common/Translation/ITranslationRouter.cs`
- **Active implementation**: `LegacyTranslationRouter` is registered in DI.
  - File: `Witcher3StringEditor/App.xaml.cs` (service registration)
  - File: `Witcher3StringEditor/Services/LegacyTranslationRouter.cs` (implementation)
- **Provider invocation**: when a provider is resolved, `LegacyTranslationRouter` builds a
  `TranslationRequest` and calls `ITranslationProvider.TranslateAsync`.
  - File: `Witcher3StringEditor/Services/LegacyTranslationRouter.cs`
  - Method: `TranslateWithProvider`
- **Legacy translator invocation**: `LegacyTranslationRouter` calls `ITranslator.TranslateAsync`.
  - File: `Witcher3StringEditor/Services/LegacyTranslationRouter.cs`
  - Method: `TranslateWithLegacyTranslator`

## Fallback Logic, Default Selection, and Trigger Conditions
### Provider selection and fallback
- **Provider resolution**: `TryResolveProvider` only succeeds when a provider name is available
  (`request.ProviderName` or `appSettings.TranslationProviderName`) **and** the registry resolves it.
  - File: `Witcher3StringEditor/Services/LegacyTranslationRouter.cs`
  - Method: `TryResolveProvider`
- **Provider failure fallback**: if provider translation fails and
  `ShouldFallbackToLegacyTranslator()` returns true, the router falls back to legacy translation.
  - File: `Witcher3StringEditor/Services/LegacyTranslationRouter.cs`
  - Methods: `TranslateAsync`, `ShouldFallbackToLegacyTranslator`
  - Trigger conditions:
    - `appSettings.Translator` is non-empty.
    - At least one legacy `ITranslator` is registered.
- **Provider error metadata**: provider failures are tagged using
  `TranslationFailureMetadata` so UI layers can detect provider-specific errors.
  - File: `Witcher3StringEditor.Common/Translation/TranslationFailureMetadata.cs`
  - File: `Witcher3StringEditor.Common/Translation/ResultExtensions.cs`

### Legacy translator selection defaults
- **Default translator resolution**: `ResolveLegacyTranslator` uses `appSettings.Translator` if set,
  otherwise the first registered translator. If the configured name is missing, it also falls back to the first.
  - File: `Witcher3StringEditor/Services/LegacyTranslationRouter.cs`
  - Method: `ResolveLegacyTranslator`
- **Dialog entry selection**: the dialog uses `appSettings.Translator` directly to pick the translator
  instance and will throw if the name is missing from the DI list.
  - File: `Witcher3StringEditor/ViewModels/MainWindowViewModel.cs`
  - Method: `ShowTranslateDialog`

### UI-facing fallback/default cues
- **Active engine label**: the translation dialog prefers provider display if `TranslationProviderName`
  is set; otherwise it shows the legacy translator name or “Unknown.”
  - File: `Witcher3StringEditor.Dialogs/ViewModels/TranslationDialogViewModel.cs`
  - Methods: `BuildActiveEngineDescription`, `BuildSelectedProviderSummary`

## Error Handling and Cancellation
- **Single-item flow**:
  - `Translate` catches exceptions, emits `TranslateError`, and logs failures.
  - Provider-specific errors are extracted via `ResultExtensions.GetProviderError` and sent through messenger.
  - File: `Witcher3StringEditor.Dialogs/ViewModels/SingleItemTranslationViewModel.cs`
- **Batch flow**:
  - Each failure increments `FailureCount` and logs errors.
  - Provider errors are reported only once via `NotifyProviderFailureOnce`.
  - File: `Witcher3StringEditor.Dialogs/ViewModels/BatchItemsTranslationViewModel.cs`
- **Router-level cancellation**:
  - Provider and legacy paths return `Result.Fail("Translation was cancelled.")` on cancellation.
  - File: `Witcher3StringEditor/Services/LegacyTranslationRouter.cs`
- **Dialog close/switch**:
  - Switching or closing can cancel pending operations or prompt to save unsaved text.
  - File: `Witcher3StringEditor.Dialogs/ViewModels/TranslationDialogViewModel.cs`

## Notes / Follow-Up Hooks
- `TranslationRouter` (non-legacy) is a stub and not registered in DI.
  - File: `Witcher3StringEditor/Services/TranslationRouter.cs`
- Translation memory, terminology/style injection, and profiles are represented in settings and
  pipeline context, but are not wired into translation execution yet.
  - File: `Witcher3StringEditor/Services/TranslationPipelineContextBuilder.cs`
