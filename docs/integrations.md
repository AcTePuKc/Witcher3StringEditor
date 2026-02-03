# Integrations Architecture & Task Plan

## Purpose
This document summarizes the planned integration architecture for future translation features (translation memory, Ollama provider selection, terminology/style loading, and translation profiles) without implementing full features yet. The intent is to keep changes incremental, safe, and compilable while exposing clear extension points for future agents.

## Architecture Summary

### Provider Abstractions
- **Goal:** Standardize translation provider usage behind a shared interface, regardless of implementation (e.g., Ollama, existing GTranslate providers).
- **Core abstractions:**
  - `ITranslationProvider` for translate + model enumeration stubs.
  - Request/response models that capture source/target language, optional glossary/style, and provider model selection.
- **Placement suggestion:** `Witcher3StringEditor/Integrations/Providers/`.
- **Notes:** No HTTP or real translation logic should be implemented yet. Providers remain inert until wired by future tasks.

### Terminology & Style Loaders
- **Goal:** Load terminology packs and style guides from local files and expose them for prompt injection and validation.
- **Core abstractions:**
  - `TerminologyPack`, `TerminologyEntry`, `StyleGuide`.
  - Loader interfaces for TSV/CSV terminology and Markdown style guides.
- **Placement suggestion:** `Witcher3StringEditor/Integrations/Terminology/`.
- **Sample files:**
  - Terminology TSV: `docs/samples/terminology.sample.tsv`.
  - Style guide Markdown: `docs/samples/style-guide.sample.md`.

### Database Layer (Translation Memory)
- **Goal:** Introduce a local-only persistence abstraction for translation memory and QA metadata.
- **Core abstractions:**
  - `ITranslationMemoryStore` for save/lookups.
  - `TranslationMemoryEntry` model.
- **Placement suggestion:** `Witcher3StringEditor/Integrations/Storage/`.
- **Notes:** Concrete SQLite or JSON implementations should be added in a future issue; current work should only stub the interface and models.

### Profiles
- **Goal:** Persist reusable translation profile settings (provider, model, glossary, style guide, QA settings).
- **Core abstractions:**
  - `TranslationProfile` model.
  - `ITranslationProfileStore` interface for persistence.
- **Placement suggestion:** `Witcher3StringEditor/Integrations/Profiles/`.
- **Notes:** Profiles should reference local file paths only and never external services.

### Task Groups
- **Goal:** Provide a lightweight grouping concept for translation tasks (batch operations, QA sweeps, or model-specific runs).
- **Core abstractions:**
  - `TranslationTaskGroup` model to capture batch metadata and target profile.
- **Placement suggestion:** `Witcher3StringEditor/Integrations/Tasks/`.
- **Notes:** This is a data-only model for now.

## Sample Files
- Terminology sample: `docs/samples/terminology.sample.tsv`.
- Style guide sample: `docs/samples/style-guide.sample.md`.

## GitHub Issue Breakdown

### Issue 1 — Inventory pass for Translation Helper integration points
**Description**
Identify exact integration points for provider selection, terminology/style injection, and translation memory in the existing Translation Helper workflow. Document class names, methods, and settings persistence paths.

**Acceptance Criteria**
- Report lists file paths, classes, and methods relevant to Translation Helper.
- Report includes risks/constraints and suggested hookup points.
- No code changes beyond documentation.

**Files to touch**
- `docs/integrations.md` (append inventory results or link a new report).

**QA Checklist**
- Build not required (doc-only).
- Verify references and file paths match repo layout.

---

### Issue 2 — Provider abstraction scaffolding
**Description**
Add core provider interfaces and request/response models to standardize translation providers (including future Ollama integration). No real provider logic, no HTTP.

**Acceptance Criteria**
- `ITranslationProvider` and related models compile.
- Interfaces are isolated from UI and services.
- TODO markers clarify future wiring.

**Files to touch**
- `Witcher3StringEditor/Integrations/Providers/ITranslationProvider.cs`
- `Witcher3StringEditor/Integrations/Providers/TranslationProviderModels.cs`

**QA Checklist**
- ✅ Build solution.
- ✅ Open app to confirm no regressions (basic startup).

---

### Issue 3 — Terminology & style loader scaffolding
**Description**
Introduce terminology/style models and loader interfaces for TSV/CSV terminology packs and Markdown style guides.

**Acceptance Criteria**
- Models and interfaces compile.
- Sample files added under `docs/samples/`.
- Loaders are stubs with TODOs (no parsing implementation required yet).

**Files to touch**
- `Witcher3StringEditor/Integrations/Terminology/TerminologyModels.cs`
- `Witcher3StringEditor/Integrations/Terminology/ITerminologyLoader.cs`
- `Witcher3StringEditor/Integrations/Terminology/IStyleGuideLoader.cs`
- `docs/samples/terminology.sample.tsv`
- `docs/samples/style-guide.sample.md`

**QA Checklist**
- ✅ Build solution.
- ✅ Open sample files to confirm format.

---

### Issue 4 — Translation memory storage scaffolding
**Description**
Define a local-only translation memory storage interface and data model. No database implementation yet.

**Acceptance Criteria**
- `ITranslationMemoryStore` compiles.
- `TranslationMemoryEntry` model included.
- TODO markers note SQLite/JSON implementations.

**Files to touch**
- `Witcher3StringEditor/Integrations/Storage/ITranslationMemoryStore.cs`

**QA Checklist**
- ✅ Build solution.

---

### Issue 5 — Translation profiles & task groups scaffolding
**Description**
Add profile and task-group models plus a profile store interface for local-only persistence.

**Acceptance Criteria**
- `TranslationProfile` and `ITranslationProfileStore` compile.
- `TranslationTaskGroup` model compiles.
- No UI or persistence wiring yet.

**Files to touch**
- `Witcher3StringEditor/Integrations/Profiles/TranslationProfile.cs`
- `Witcher3StringEditor/Integrations/Profiles/ITranslationProfileStore.cs`
- `Witcher3StringEditor/Integrations/Tasks/TranslationTaskGroup.cs`

**QA Checklist**
- ✅ Build solution.

---

### Issue 6 — Settings & UI placeholders (future)
**Description**
Add minimal settings placeholders for provider/model selection, terminology/style file selection, and profile selection. Wire to existing settings persistence but keep functionality inert.

**Acceptance Criteria**
- UI placeholders exist in settings or dialogs with no workflow changes.
- Settings are saved/loaded.
- No provider logic invoked.

**Files to touch**
- TBD after inventory pass (likely Settings dialogs/viewmodels).

**QA Checklist**
- ✅ Build solution.
- ✅ Manually open settings dialog; verify placeholders render without errors.

