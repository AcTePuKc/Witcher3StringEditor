> Archived: This task list is superseded by [../integrations.md](../integrations.md), [../implementation.md](../implementation.md), and [../inspections/scaffold-progress.md](../inspections/scaffold-progress.md).

# Task Breakdown (GitHub Issues)

## Issue 1: Inventory pass for integration entrypoints
**Description**
Locate and document the translation workflow entrypoints, settings persistence, and UI surfaces where integrations should eventually plug in. This is a read-only pass to avoid guessing integration points.

**Behavior impact**
- None. Documentation-only inventory.

**Acceptance Criteria**
- A markdown report lists relevant classes, methods, and settings paths.
- No production code changes are required.

**Files to touch**
- `docs/inventory/integration-entrypoints.md` (new report)

**Build command**
- `dotnet build Witcher3StringEditor.slnx`

**Reminder**
- Keep this read-only: do not add hooks or runtime wiring during the inventory pass.

**QA Checklist**
- Build: optional (docs-only).
- Manual: confirm the report lists UI entrypoints, settings storage, and translation flow hooks.
- Regression: no behavior changes expected.

---

## Issue 2: Translation memory SQLite schema & storage stubs
**Description**
Add a concrete schema definition document and a minimal database bootstrapper stub to align with the local-only storage requirement. Keep implementations inert and compile-safe.

**Behavior impact**
- None. Interfaces/stubs only, no runtime wiring.

**Acceptance Criteria**
- A schema document describes tables/indices and initial migration steps.
- A new bootstrapper interface + stub is added without wiring into runtime flows.
- A SQLite store factory stub can build a store from settings without runtime wiring.
- `TranslationMemorySettings` exposes `Enabled` and `DatabasePath` for local configuration.
- All changes compile.

**Files to touch**
- `docs/integrations/translation-memory-schema.md` (new)
- `Witcher3StringEditor.Common/TranslationMemory` (new interface)
- `Witcher3StringEditor.Data/TranslationMemory` (new stub)
  - `SqliteTranslationMemoryStoreFactory.cs` (new)

**Build command**
- `dotnet build Witcher3StringEditor.slnx`

**Reminder**
- Do not register the store or initializer with production services yet.

**QA Checklist**
- ✅ Build: solution builds.
- Manual: run the initializer in isolation (no runtime wiring).
- Regression: ensure no settings or translation pipeline wiring.

---

## Issue 3: Ollama model selection scaffolding
**Description**
Extend the Ollama integration with model-selection request/response DTOs and a stubbed catalog method. No Settings
or translation flow wiring; no HTTP calls yet.

**Behavior impact**
- None. DTOs and stub interfaces only.

**Acceptance Criteria**
- DTOs exist for model listing and selection.
- A stub method returns placeholder data when configured.
- No changes to Settings or translation pipeline.

**Files to touch**
- `Witcher3StringEditor.Integrations.Ollama` (new DTOs and stub methods)
- `docs/integrations/ollama.md` (new)

**Build command**
- `dotnet build Witcher3StringEditor.slnx`

**Reminder**
- Do not add HTTP calls or inject the provider into runtime flows.

**QA Checklist**
- ✅ Build: solution builds.
- Manual: call the model-listing stub in isolation (no HTTP calls).
- Regression: no settings or translation pipeline wiring.

---

## Issue 4: Terminology & style loaders (local assets)
**Description**
Add terminology and style loader interfaces plus no-op stubs for local assets. Expand the style guide model to include
section/rule lists for future validation. Skip runtime wiring and enforcement for now; keep only compile-safe
placeholders.

**Behavior impact**
- None. Interfaces/stubs only, no parsing or runtime wiring.

**Acceptance Criteria**
- Loader interfaces and no-op stubs compile.
- Style guide model includes section/rule lists for future validation.
- Source descriptors include file type hints (`.csv`, `.tsv`, `.md`) without parsing.
- `TerminologyEntry` includes `Term`, `Translation`, `Notes`, and `Mode` fields.
- No runtime wiring or enforcement.

**Files to touch**
- `Witcher3StringEditor.Common/Terminology` (models if needed)
- `Witcher3StringEditor/Integrations/Terminology` (stubs/models)
- `Witcher3StringEditor.Data/Terminology` (new loader stubs)
- `docs/samples/` (sample assets, static)

**Build command**
- `dotnet build Witcher3StringEditor.slnx`

**Reminder**
- Avoid adding any parsing or validation logic until runtime wiring is approved.

**QA Checklist**
- ✅ Build: solution builds.
- Manual: run loader stubs against sample files in isolation.
- Regression: no settings or translation pipeline wiring.

---

## Issue 5: Translation profiles persistence and preview stubs
**Description**
Create minimal preview/selection interfaces and no-op stubs for profiles. Keep implementations inert and avoid runtime
wire-ups or UI/Settings connections for now.

**Behavior impact**
- None. Interfaces/stubs only, no runtime wiring.

**Acceptance Criteria**
- Stub preview and selection services compile.
- A profile summary listing interface exists for lightweight catalog views.
- No settings, UI, or pipeline wiring.

**Files to touch**
- `Witcher3StringEditor.Common/Profiles` (new interfaces if required)
- `Witcher3StringEditor/Integrations/Profiles` (summary catalog interface)
- `Witcher3StringEditor.Data/Profiles` (stub services)
- `docs/integrations/translation-profiles.md` (new)

**Build command**
- `dotnet build Witcher3StringEditor.slnx`

**Reminder**
- Do not add JSON parsing or persistence until runtime wiring is approved.

**QA Checklist**
- ✅ Build: solution builds.
- Manual: call the profile catalog stub in isolation.
- Regression: no settings or translation pipeline wiring.

---

## Issue 6: Provider failure DTO scaffolding
**Description**
Introduce a DTO to capture provider failure metadata (provider name, failure kind, message) so future diagnostics can
standardize error reporting. Keep this unused by runtime flows for now.

**Behavior impact**
- None. DTO-only scaffolding, no runtime wiring.

**Acceptance Criteria**
- DTO exists with provider name, failure kind, and message fields.
- DTO is not referenced by any runtime services yet.
- All changes compile.

**Files to touch**
- `Witcher3StringEditor.Common/Translation/TranslationProviderFailureDto.cs`
- `docs/integrations.md`

**Build command**
- `dotnet build Witcher3StringEditor.slnx`

**Reminder**
- Do not wire the DTO into logging or UI until diagnostics requirements are defined.

**QA Checklist**
- ✅ Build: solution builds.
- Manual: inspect the DTO model in isolation (no runtime usage).
- Regression: translation flow remains unchanged.

---

## Issue 7: QA diagnostic message DTO scaffolding
**Description**
Introduce a DTO for QA diagnostics with code, message, severity, and location fields. Keep this unused by runtime flows
for now.

**Behavior impact**
- None. DTO-only scaffolding, no runtime wiring.

**Acceptance Criteria**
- DTO exists with `Code`, `Message`, `Severity`, and `Location` fields.
- DTO is not referenced by any runtime services yet.
- All changes compile.

**Files to touch**
- `Witcher3StringEditor.Common/QualityAssurance/QualityAssuranceDiagnosticDto.cs`
- `docs/integrations.md`

**Build command**
- `dotnet build Witcher3StringEditor.slnx`

**Reminder**
- Do not wire the DTO into logging or UI until diagnostics requirements are defined.

**QA Checklist**
- ✅ Build: solution builds.
- Manual: inspect the DTO model in isolation (no runtime usage).
- Regression: QA flows remain unchanged.

---

## Issue 8: Terminology prompt metadata (provider routing only)
**Description**
Build a terminology prompt block from loaded terminology/style packs and attach it to translation provider request
metadata when provider routing is enabled. Keep this limited to metadata wiring only, with no enforcement or UI changes.

**Behavior impact**
- Provider requests include optional terminology prompt metadata when provider routing is enabled.

**Acceptance Criteria**
- Terminology/style packs are loaded via existing loaders and merged as needed.
- A prompt block is generated and stored in provider request metadata keys.
- Metadata is only attached when provider routing is enabled.
- Translation flow remains unchanged when routing is disabled.
- All changes compile.

**Files to touch**
- `Witcher3StringEditor/Services/LegacyTranslationRouter.cs`
- `Witcher3StringEditor/Services/TerminologyPromptBuilder.cs` (new)
- `Witcher3StringEditor.Common/Terminology/TerminologyPromptMetadata.cs` (new)
- `docs/integrations.md`

**Build command**
- `dotnet build Witcher3StringEditor.slnx`

**Reminder**
- Do not enforce terminology rules or modify UI flows.

**QA Checklist**
- ✅ Build: solution builds.
- Manual: run a provider-routed translation and confirm metadata is present in the provider request (debug only).
- Regression: routing disabled behaves as before.
