# Task Breakdown (GitHub Issues)

## Issue 1: Inventory pass for integration entrypoints
**Description**
Locate and document the translation workflow entrypoints, settings persistence, and UI surfaces where integrations should eventually plug in. This is a read-only pass to avoid guessing integration points.

**Acceptance Criteria**
- A markdown report lists relevant classes, methods, and settings paths.
- No production code changes are required.

**Files to touch**
- `docs/inventory/integration-entrypoints.md` (new report)

**QA Checklist**
- Build: not required (docs-only).
- Manual: confirm report lists UI entrypoints and settings storage.
- Regression: none expected.

---

## Issue 2: Translation memory SQLite schema & storage stubs
**Description**
Add a concrete schema definition document and a minimal database bootstrapper stub to align with the local-only storage requirement. Keep implementations inert and compile-safe.

**Acceptance Criteria**
- A schema document describes tables/indices and initial migration steps.
- A new bootstrapper interface + stub is added without wiring into runtime flows.
- All changes compile.

**Files to touch**
- `docs/integrations/translation-memory-schema.md` (new)
- `Witcher3StringEditor.Common/TranslationMemory` (new interface)
- `Witcher3StringEditor.Data/TranslationMemory` (new stub)

**QA Checklist**
- ✅ Build: solution builds.
- Manual: none (stubs only).
- Regression: ensure no settings or translation pipeline wiring.

---

## Issue 3: Ollama model selection scaffolding
**Description**
Extend the Ollama integration with model-selection request/response DTOs and a stubbed catalog method. No UI wiring or HTTP calls yet.

**Acceptance Criteria**
- DTOs exist for model listing and selection.
- A stub method returns placeholder data when configured.
- No changes to Settings or translation pipeline.

**Files to touch**
- `Witcher3StringEditor.Integrations.Ollama` (new DTOs and stub methods)
- `docs/integrations/ollama.md` (new)

**QA Checklist**
- ✅ Build: solution builds.
- Manual: none (stubs only).
- Regression: no settings or translation pipeline wiring.

---

## Issue 4: Terminology & style loaders (local assets)
**Description**
Add local file loader stubs for terminology and style resources, with minimal parsing logic and TODOs for validation. Keep logic isolated.

**Acceptance Criteria**
- Loaders accept `.csv`, `.tsv`, and `.md` inputs with basic parsing.
- Output models align with current terminology abstractions.
- No runtime wiring or enforcement.

**Files to touch**
- `Witcher3StringEditor.Common/Terminology` (models if needed)
- `Witcher3StringEditor.Data/Terminology` (new loader stubs)
- `docs/samples/` (sample assets)

**QA Checklist**
- ✅ Build: solution builds.
- Manual: optional run of loader in isolation.
- Regression: no settings or translation pipeline wiring.

---

## Issue 5: Translation profiles persistence and preview stubs
**Description**
Create minimal preview/selection stubs that read local JSON profiles without changing core flows. Keep implementations inert by default.

**Acceptance Criteria**
- Stub preview and selection services compile.
- JSON catalog loading is read-only and local.
- A profile summary listing interface exists for lightweight catalog views.
- No settings or UI wiring.

**Files to touch**
- `Witcher3StringEditor.Common/Profiles` (new interfaces if required)
- `Witcher3StringEditor/Integrations/Profiles` (summary catalog interface)
- `Witcher3StringEditor.Data/Profiles` (stub services)
- `docs/integrations/translation-profiles.md` (new)

**QA Checklist**
- ✅ Build: solution builds.
- Manual: optional catalog load in isolation.
- Regression: no settings or translation pipeline wiring.
