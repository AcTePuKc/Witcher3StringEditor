# Integrations Architecture (Short Spec)

## Goals
- Keep all new integrations **local-only** (SQLite or JSON).
- Add safe, inert scaffolding for future features without changing current workflows.
- Prefer small interfaces and TODO hooks over real implementations.

## Current Scaffolding Status
- **Translation memory**: SQLite bootstrap + `ITranslationMemoryStore` exist; lookup/save are inert until enabled.
- **Ollama provider + model selection**: settings + provider stubs exist with placeholder model listing.
- **Terminology/style loading**: loaders, prompt builder interface, and sample packs exist; prompt injection/validation remain TODO.
- **Translation profiles**: JSON-backed profile store + resolver stubs exist; profiles include terminology/style paths and
  translation memory flags, but routing is not wired yet.
- **Issue drafts**: task breakdown lives in `docs/integration-issues.md`.

## Architecture Overview
### Translation Providers + Model Selection
- **Interfaces** live in `Witcher3StringEditor.Common/Translation/`.
- **Registry** (future) resolves provider names to `ITranslationProvider` instances.
- **Translation router** (`ITranslationRouter`) routes between the legacy `ITranslator` flow and provider flow
  depending on settings; provider calls are guarded and return structured failures when providers error out.
  The router request now carries optional provider/model names so call sites can override settings when needed.
- **Model catalog stub** lives in `Witcher3StringEditor.Common/Translation/ITranslationModelCatalog.cs` with a
  no-op implementation in `Witcher3StringEditor/Services/NoopTranslationModelCatalog.cs`.
- **Pipeline context** lives in `Witcher3StringEditor.Common/Translation/TranslationPipelineContext.cs` and carries
  profile/provider/model/terminology/translation memory selections.
- **Ollama stub** lives in `Witcher3StringEditor.Integrations.Ollama/` with settings + model listing placeholder.
- **Settings bridge** lives in `IAppSettings` (`TranslationProviderName`, `TranslationModelName`, `TranslationBaseUrl`, `CachedTranslationModels`).

### Provider Selection Behavior
- **Current legacy path**: the translation flow still uses the existing `ITranslator` selection and execution
  logic; provider routing is **not** invoked unless explicitly configured in settings.
- **Planned provider routing**: if a provider is selected **and** the registry can resolve it, the provider path is
  selected (currently stubbed). If provider resolution fails, the router short-circuits to the legacy translator path.
  Provider/model names are resolved from the router request first, with app settings as fallback.
- **Fallback + error handling**: provider failures return structured errors (provider name + failure kind), and the
  router can fall back to the configured legacy translator; if no fallback is configured, the translation dialog
  surfaces an error message.
- **Safety by default**: provider routing is **opt-in** and must remain inert until users explicitly set a provider
  name/model. Default settings keep the legacy translator path active and stable.
- The Translation dialog header now shows a read-only provider/model/base URL summary sourced from app settings to
  surface future provider selection without changing routing.
- The header display uses `TranslationModelName` directly; when it is empty the UI shows “(none selected)”.
- Model lists are only refreshed from the Settings dialog on explicit user action; translation dialogs do not
  auto-refresh models.

### Translation Memory (Database-Backed)
- **Interfaces/models** live in `Witcher3StringEditor.Common/TranslationMemory/`.
- **Settings stub** lives in `Witcher3StringEditor.Common/TranslationMemory/TranslationMemorySettings.cs`.
- **SQLite storage** lives in `Witcher3StringEditor.Data/TranslationMemory/`.
- **Database initializer stub** lives in `Witcher3StringEditor.Common/TranslationMemory/ITranslationMemoryDatabaseInitializer.cs`
  with a no-op implementation in `Witcher3StringEditor/Services/NoopTranslationMemoryDatabaseInitializer.cs`.
- Data is stored under AppData via `Witcher3StringEditor.Data/Storage/` helpers.

### Terminology + Style Packs
- **Models + loader interface** live in `Witcher3StringEditor.Common/Terminology/`.
- **Loader implementation** lives in `Witcher3StringEditor/Services/TerminologyLoader.cs`.
- **Prompt builder interface** lives in `Witcher3StringEditor.Common/Terminology/ITerminologyPromptBuilder.cs` with a
  no-op implementation in `Witcher3StringEditor/Services/NoopTerminologyPromptBuilder.cs`.
- **Sample fixtures** live under `docs/samples/` for TSV/CSV and Markdown style guides.

### Translation Profiles
- **Profile models** live in `Witcher3StringEditor.Common/Profiles/`.
- Profiles can carry provider/model/base URL plus terminology/style paths and translation memory enablement flags.
- **Profile store** lives in `Witcher3StringEditor.Data/Profiles/` (JSON-backed, AppData).
- **Resolver stub** lives in `Witcher3StringEditor/Services/` to merge profiles with settings later.
- **Pipeline context builder** lives in `Witcher3StringEditor/Services/TranslationPipelineContextBuilder.cs` and
  produces a read-only context for future routing.

## Inventory/Discovery Notes
- There is a parallel `Witcher3StringEditor/Integrations/` folder with early integration interfaces.
  The inventory pass should confirm whether to consolidate on `Witcher3StringEditor.Common/*` or keep both.

## Wiring Map (Current → Future)
- **Settings dialog** should remain the single source of truth for provider/model/profile selection.
- **Main translation flow** (`MainWindowViewModel`) should eventually call the provider registry when
  `TranslationProviderName` is configured; otherwise it should keep using the existing translator list.
- **Translation memory** should be queried right before provider/translator execution and saved after a
  successful result. (TODO: hook into the translation command path.)
- **Terminology/style** should be loaded on demand (path in settings or profile) and injected into the
  provider request metadata, with validation hooks after translation. (TODO: inject + validate.)

## Translation Router Reference Map
- **Interface + request DTO** live in `Witcher3StringEditor.Common/Translation/ITranslationRouter.cs`.
- **Router implementations** live in `Witcher3StringEditor/Services/TranslationRouter.cs` and
  `Witcher3StringEditor/Services/NoopTranslationRouter.cs`.
- **View model call sites**:
  - `Witcher3StringEditor/ViewModels/MainWindowViewModel.cs`
  - `Witcher3StringEditor.Dialogs/ViewModels/TranslationViewModelBase.cs`
  - `Witcher3StringEditor.Dialogs/ViewModels/TranslationDialogViewModel.cs`
  - `Witcher3StringEditor.Dialogs/ViewModels/SingleItemTranslationViewModel.cs`
  - `Witcher3StringEditor.Dialogs/ViewModels/BatchItemsTranslationViewModel.cs`

## Planned Tasks (Issue Breakdown Summary)
1. Inventory pass to confirm settings persistence, translation entry points, UI hooks, and integration namespaces.
2. Wire provider registry + model discovery (Ollama first) with no behavior changes.
3. Keep translation memory + QA stores inert until explicitly enabled.
4. Extend terminology/style loading with prompt injection + validation hooks (TODOs only).
5. Add profile selection wiring (store + resolver) without altering existing translator flow.
6. Add minimal settings stubs for translation memory enablement + local database path.
7. Introduce a translation pipeline context builder to combine settings, profiles, and terminology (TODOs only).

## Constraints
- No external services.
- No UI redesigns; only minimal placeholders in settings if needed.
- Every change must compile and remain inert by default.
