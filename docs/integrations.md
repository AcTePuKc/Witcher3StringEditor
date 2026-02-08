# Integrations Architecture (Short Spec)

## Goals
- Keep all new integrations **local-only** (SQLite or JSON).
- Add safe, inert scaffolding for future features without changing current workflows.
- Prefer small interfaces and TODO hooks over real implementations.
- Prioritize stubs for translation memory, Ollama model selection, terminology/style loading, and translation profiles.

## Requested Focus Areas
- Database-backed translation memory (local-only, SQLite).
- Ollama integration with model selection (local-only; stubbed HTTP).
- Terminology and style pack loading (file-based loaders only).
- Translation profiles for local settings overrides (JSON-backed).

## Current Scaffolding Status
- **Translation memory**: SQLite bootstrap + `ITranslationMemoryStore` exist; lookup/save are inert until enabled.
- **Translation memory workflow**: `ITranslationMemoryService` + no-op implementation exist to coordinate
  context-driven lookups/saves once routing is wired.
- **Ollama provider + model selection**: settings + provider stubs exist with placeholder model listing.
- **Terminology/style loading**: loaders, prompt builder interface, and sample packs exist; prompt injection/validation remain TODO.
- **Translation profiles**: JSON-backed profile store + resolver stubs exist; profiles include terminology/style paths and
  translation memory flags, but routing is not wired yet.
- **Issue drafts**: task breakdown lives in `docs/integration-issues.md`.

## Architecture Overview
### Translation Providers + Model Selection
- **Interfaces** live in `Witcher3StringEditor.Common/Translation/`.
- **Registry (active)** resolves provider names to `ITranslationProvider` instances via DI-backed
  `Witcher3StringEditor/Services/TranslationProviderRegistry.cs`.
- **Registry (legacy)** still exists at `Witcher3StringEditor.Common/Translation/TranslationProviderRegistry.cs` but
  currently has no confirmed call sites and should be treated as a compatibility shim only.
- **Legacy adapter stub** lives in `Witcher3StringEditor/Services/LegacyTranslationProviderRegistryAdapter.cs` and
  can be wired if a legacy call site is discovered.
- **Usage review (current)**:
  - `App.xaml.cs` registers the DI-backed registry with `ITranslationProviderRegistry`.
  - `TranslationRouter` and `LegacyTranslationRouter` resolve providers through `ITranslationProviderRegistry`.
  - No direct usage of the legacy registry class is currently detected.
- **Translation router** (`ITranslationRouter`) routes between the legacy `ITranslator` flow and provider flow
  depending on settings; provider calls are guarded and return structured failures when providers error out.
  The router request now carries optional provider/model names plus a `UseProviderForTranslation` flag so call
  sites can request provider routing when the router supports it.
- **Model catalog stub** lives in `Witcher3StringEditor.Common/Translation/ITranslationModelCatalog.cs` with a
  no-op implementation in `Witcher3StringEditor/Services/NoopTranslationModelCatalog.cs`.
- **Pipeline context** lives in `Witcher3StringEditor.Common/Translation/TranslationPipelineContext.cs` and carries
  profile/provider/model/terminology/translation memory selections.
- **Ollama stub** lives in `Witcher3StringEditor.Integrations.Ollama/` with settings + model listing placeholder.
- **Settings bridge** lives in `IAppSettings` (`TranslationProviderName`, `TranslationModelName`, `TranslationBaseUrl`,
  `CachedTranslationModels`, `UseTerminologyPack`, `UseStyleGuide`, `UseTranslationMemory`, `TranslationMemoryPath`).

### Provider Selection Behavior
- **Current legacy path**: the translation flow still uses the existing `ITranslator` selection and execution
  logic; provider routing is **not** invoked unless explicitly configured in settings.
- **Provider routing**: if a provider is selected **and** the registry can resolve it, the provider path is
  selected (currently stubbed). If provider resolution fails, the router logs a warning and short-circuits to the
  legacy translator path.
  Provider/model names are resolved from the router request first, with app settings as fallback.
- **Fallback + error handling**: provider failures return structured errors (provider name + failure kind), and the
  router can fall back to the configured legacy translator; if no fallback is configured, the translation dialog
  surfaces an error message.
- **Safety by default**: provider routing is **opt-in** and must remain inert until users explicitly set a provider
  name/model. Default settings keep the legacy translator path active and stable.
- **Translation dialog toggle**: the dialog exposes a `Use provider routing` toggle that sets
  `UseProviderForTranslation` on router requests. Routing must remain opt-in: the router should only attempt provider
  calls when this toggle is enabled, even if provider settings are present.
- **Provider validation step (planned)**: before any provider execution, the router should run a pre-flight validation
  step that checks provider name, model, base URL, and cached model availability. Validation failures should surface a
  warning and fall back to legacy translation without throwing.
- The Translation dialog header now shows a read-only provider/model/base URL summary sourced from app settings to
  surface future provider selection without changing routing.
- The header display uses `TranslationModelName` directly; when it is empty the UI shows “(none selected)”.
- The translation dialog also includes a read-only provider readiness status line derived from current settings,
  highlighting missing configuration without activating routing.
- Translation view models surface a status line for provider routing attempts (provider + model) and for legacy
  fallbacks, so users can confirm which path the router chose without changing behavior.
- Model lists are only refreshed from the Settings dialog on explicit user action; translation dialogs do not
  auto-refresh models.

### Translation Memory (Database-Backed)
- **Interfaces/models** live in `Witcher3StringEditor.Common/TranslationMemory/`.
- **Workflow stub** lives in `Witcher3StringEditor.Common/TranslationMemory/ITranslationMemoryService.cs` with a
  no-op implementation in `Witcher3StringEditor/Services/NoopTranslationMemoryService.cs`. The translation dialog
  view models call `LookupAsync` before translation and `SaveAsync` after successful translations, but the default
  no-op service returns empty results and stores nothing.
- **Settings stub** lives in `Witcher3StringEditor.Common/TranslationMemory/TranslationMemorySettings.cs`.
- **Settings provider stub** lives in `Witcher3StringEditor.Common/TranslationMemory/ITranslationMemorySettingsProvider.cs`
  with a basic implementation in `Witcher3StringEditor/Services/TranslationMemorySettingsProvider.cs`.
- **SQLite storage** lives in `Witcher3StringEditor.Data/TranslationMemory/`.
- **Database initializer stub** lives in `Witcher3StringEditor.Common/TranslationMemory/ITranslationMemoryDatabaseInitializer.cs`
  with a no-op implementation in `Witcher3StringEditor.Data/TranslationMemory/NoopTranslationMemoryDatabaseInitializer.cs`.
- Data is stored under AppData via `Witcher3StringEditor.Data/Storage/` helpers.

### Terminology + Style Packs
- **Models + loader interface** live in `Witcher3StringEditor.Common/Terminology/`.
- **Loader implementation** lives in `Witcher3StringEditor/Services/TerminologyLoader.cs`.
- **Prompt builder interface** lives in `Witcher3StringEditor.Common/Terminology/ITerminologyPromptBuilder.cs` with a
  no-op implementation in `Witcher3StringEditor/Services/NoopTerminologyPromptBuilder.cs`.
- **Sample fixtures** live under `docs/samples/` for TSV/CSV and Markdown style guides.
- **Settings selection**: the Settings dialog provides separate file pickers for terminology packs (`.tsv`/`.csv`)
  and style guides (`.md`). Selections are stored as file paths and loaded on demand when preview is enabled.
- **Preview validation**: the Settings dialog view model depends on terminology + style guide loaders to validate
  selections (on file selection and toggle changes) and surface status text without changing translation output.
- **Validation service stub**: `ITerminologyValidationService` provides async validation hooks for terminology and
  style guides, with a no-op implementation returning “Not validated.” until real checks are added.
- **Enablement flags**: `UseTerminologyPack` and `UseStyleGuide` are preview toggles only. The Settings dialog loads
  the selected file and reports status (loaded/failed) but does not enforce terminology or style rules during
  translation yet.
- **Future enforcement**: prompt injection and post-translation validation remain planned work; enabling the flags
  should not change translation output until those hooks are implemented.

### Output Post-Processing (Opt-in)
- **Post-processing interface** lives in `Witcher3StringEditor.Common/Translation/ITranslationPostProcessor.cs` and
  accepts a `TranslationContext` describing languages, provider/model selection, and profile settings.
- **No-op implementation** lives in `Witcher3StringEditor/Services/NoopTranslationPostProcessor.cs` and returns the
  original text unchanged.
- **View model usage**: translation view models call the post-processor before displaying translation results,
  so future rules are applied consistently in single-item, batch, and translation-memory flows.
- **Dependency injection**: `ITranslationPostProcessor` is registered in `App.xaml.cs` and injected into
  `TranslationViewModelBase`, keeping post-processing inert until non-no-op rules are added.
- **Planned scope**: opt-in, string-level rules for stripping polite prefixes or boilerplate in translation outputs.
  This runs after provider/legacy translation and before results are shown to the user.

### Translation Profiles
- **Profile models** live in `Witcher3StringEditor.Common/Profiles/`.
- Profiles can carry provider/model/base URL plus terminology/style paths, file path aliases, enablement toggles,
  and translation memory flags.
- **Profile store** lives in `Witcher3StringEditor.Data/Profiles/` (JSON-backed, AppData).
- **Resolver stub** lives in `Witcher3StringEditor/Services/` to merge profiles with settings later.
- **Settings resolver stub** lives in `Witcher3StringEditor.Common/Profiles/ITranslationProfileSettingsResolver.cs`
  with a no-op implementation in `Witcher3StringEditor/Services/NoopTranslationProfileSettingsResolver.cs`.
- **Selection service stub** lives in `Witcher3StringEditor.Common/Profiles/ITranslationProfileSelectionService.cs`
  with a no-op implementation in `Witcher3StringEditor/Services/NoopTranslationProfileSelectionService.cs`.
- **Catalog stub** lives in `Witcher3StringEditor.Common/Profiles/ITranslationProfileCatalog.cs` with a no-op
  implementation in `Witcher3StringEditor/Services/NoopTranslationProfileCatalog.cs` for lightweight profile
  listings in future settings UI.
- **Pipeline context builder** lives in `Witcher3StringEditor/Services/TranslationPipelineContextBuilder.cs` and
  produces a read-only context for future routing.

## Inventory/Discovery Notes
- There is a parallel `Witcher3StringEditor/Integrations/` folder with early integration interfaces.
  The inventory pass should confirm whether to consolidate on `Witcher3StringEditor.Common/*` or keep both.

## TranslationProviderRegistry Ownership Decision
- **Decision**: `Witcher3StringEditor.Services` is the long-term owner of the concrete registry implementation.
  `Witcher3StringEditor.Common` should retain only interfaces/DTOs shared across layers.
- **Rationale**:
  - The active registry is already DI-backed and lives in Services.
  - All runtime usage goes through `ITranslationProviderRegistry`, and the DI container binds to the Services registry.
  - Keeping implementations in Services avoids coupling Common to app-level composition and provider lifetimes.
- **Future steps**:
  1. Confirm no runtime call sites still instantiate `Common.TranslationProviderRegistry`.
  2. If legacy usage is found, add a short-lived adapter or migration path in Services.
  3. Remove the legacy registry class once no call sites remain and update any docs/tests referencing it.

## Wiring Map (Current → Future)
- **Settings dialog** should remain the single source of truth for provider/model/profile selection.
- **Main translation flow** (`MainWindowViewModel`) should eventually call the provider registry when
  `TranslationProviderName` is configured; otherwise it should keep using the existing translator list.
- **Translation memory** is queried right before provider/translator execution and saved after a successful
  result in the translation dialog view models (single-item and batch). The default service remains inert.
- **Terminology/style** should be loaded on demand (path in settings or profile) and injected into the
  provider request metadata, with validation hooks after translation. (TODO: inject + validate.)

## Wiring Sequence (Recommended Order)
1. **Inventory confirmation**: finalize entrypoints for translation routing, settings persistence, and dialog wiring
   (Issue 1).
2. **Provider registry + model catalog**: wire provider resolution and model listing stubs (Issue 3), leaving the
   legacy translator flow as the default path.
3. **Terminology/style loaders**: load packs on demand for preview and future prompt injection (Issue 4).
4. **Translation profile storage + resolver**: persist profiles locally and add a resolver stub for merging with
   settings (Issue 5).
5. **Translation memory scaffolding**: finalize TM/QA store and settings stubs, keep no-op workflow (Issue 2).
6. **Settings + UI placeholders**: surface provider/model/terminology/profile selections without changing behavior
   (Issues 8, 9, 10).
7. **Pipeline context builder**: collect settings/profile data into a read-only context for future routing (Issue 7).
8. **Router expansion**: allow request/profile overrides but keep fallbacks and no-op defaults (Issue 11).
9. **Future activation**: only after explicit feature flagging and QA signoff should providers/TM/terminology affect
   translation output.

## No-op Behavior Notes (Safety Defaults)
- **Provider routing**: if no provider is selected, or resolution fails, the router falls back to legacy translators.
  Provider calls must never execute unless the user has explicitly configured a provider and model **and** enabled the
  opt-in routing toggle.
- **Model discovery**: cached model lists are refreshed only on explicit user action; translation dialogs do not
  auto-refresh or perform background calls.
- **Translation memory**: the no-op service returns empty results and performs no writes unless enabled, and even then
  the default initializer is inert until a feature flag is added.
- **Terminology/style**: enablement toggles only affect preview loading and status text. Prompt injection and
  post-translation validation remain TODO.
- **Profiles**: selecting a profile only changes read-only summaries until profile resolution is wired into routing.

## Translation Router Reference Map
- **Interface + request DTO** live in `Witcher3StringEditor.Common/Translation/ITranslationRouter.cs`.
- **Router implementations** live in `Witcher3StringEditor/Services/LegacyTranslationRouter.cs` and
  `Witcher3StringEditor/Services/TranslationRouter.cs`, with the latter performing provider-name checks and
  fallback logging.
- **View model call sites**:
  - `Witcher3StringEditor/ViewModels/MainWindowViewModel.cs`
  - `Witcher3StringEditor.Dialogs/ViewModels/TranslationViewModelBase.cs`
  - `Witcher3StringEditor.Dialogs/ViewModels/TranslationDialogViewModel.cs`
  - `Witcher3StringEditor.Dialogs/ViewModels/SingleItemTranslationViewModel.cs`
  - `Witcher3StringEditor.Dialogs/ViewModels/BatchItemsTranslationViewModel.cs`

## Translation Flow + Fallback Investigation
See `docs/fallback-investigation.md` for the current single-item/batch flow trace and fallback logic map.

## Fallback Triggers + Scope (Planned)
- **Scope**: provider fallback policy applies **only** in translation windows (single-item and batch dialogs). It does
  not apply to background workflows, import/export flows, or any non-translation UI.
- **Trigger conditions** (planned): The router will fall back to the legacy translator under the following conditions, which are grouped by whether a provider call is attempted.
  - **Fallback before provider attempt:**
    - Provider explicitly disabled in settings/profile (treated as opt-out).
    - Provider resolution fails (unknown name or registry missing provider).
    - Requested model missing/unavailable (catalog lookup fails or model list is empty).
  - **Fallback after failed provider attempt:**
    - Provider returns an error (e.g., network/HTTP failure, invalid response, validation error).
    - Provider call timeout (configurable timeout expires before response).
- **Fallback behavior**: if a trigger condition is hit, the router should fall back to the legacy translator when
  configured; otherwise surface a localized error message in the translation window.
- **Open questions / telemetry**:
  - What metrics should be logged for fallback decisions (provider name, model name, failure kind, latency)?
  - Where should logs be written (local file, existing logging sink, in-memory diagnostics panel)?
  - Should failures be rate-limited or deduplicated to avoid noisy logs during batch runs?
  - Do we need per-provider timeout settings vs. global timeout defaults?

## Planned Tasks (Issue Breakdown Summary)
1. Inventory pass to confirm settings persistence, translation entry points, UI hooks, and integration namespaces.
2. Wire provider registry + model discovery (Ollama first) with no behavior changes.
3. Keep translation memory + QA stores inert until explicitly enabled.
4. Extend terminology/style loading with prompt injection + validation hooks (TODOs only).
5. Add profile selection wiring (store + resolver) without altering existing translator flow.
6. Validate translation memory settings wiring (UseTranslationMemory + TranslationMemoryPath now exist, but they
   are not consumed by the translation flow yet).
7. Introduce a translation pipeline context builder to combine settings, profiles, and terminology (TODOs only).
8. Add opt-in post-processing rules for AI output cleanup (polite prefix stripping), defaulting to no-op.

## Constraints
- No external services.
- No UI redesigns; only minimal placeholders in settings if needed.
- Every change must compile and remain inert by default.
