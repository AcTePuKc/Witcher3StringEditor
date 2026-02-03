# Integrations Roadmap

This document describes the planned architecture and incremental tasks for adding translation memory, Ollama integration, terminology/style support, and translation profiles. All changes are intended to be **local-only**, incremental, and safe by default.

## Architecture (Planned)

- **Translation provider abstraction**: `ITranslationProvider` defines the provider contract, with a small DTO surface (`TranslationRequest`, `TranslationResult`, `ModelInfo`). Providers will be registered in a `TranslationProviderRegistry` keyed by provider name.
- **Routing**: The existing `ITranslator` flow remains the default. Future routing will map user-selected providers to the registry and project settings, while keeping the current GTranslate flow intact.
- **Translation memory (TM)**: A local storage layer (SQLite or JSON) will store source/target text pairs and metadata. A separate interface will mediate retrieval and persistence.
- **Terminology & style**: Terminology packs and style guides will be loaded from local files. A lightweight loader will parse `.csv`, `.tsv`, and `.md` into a common model.
- **Translation profiles**: A profile aggregates provider selection, model, terminology packs, and TM usage flags. Profiles are stored locally and referenced by name in settings.

## Planned Tasks (GitHub Issues)

### Issue 1: Inventory existing translation and settings entry points
**Description**
Perform an inventory pass to locate translation entry points, settings persistence, and dialog/view models for translation configuration. Document findings and risks for later agents.

**Acceptance Criteria**
- A markdown report listing file paths, classes, and methods that participate in translation.
- Settings persistence mechanism identified.
- Risks or architectural constraints documented.

**Files to touch**
- `docs/integrations.md` (append inventory report section)
- Optional: `docs/inventory/translation-entrypoints.md`

**QA Checklist**
- ✅ Build succeeds (`dotnet build`).
- ✅ App starts and translation dialog still opens.
- ✅ No functional changes beyond documentation.

---

### Issue 2: Ollama provider settings + stub provider
**Description**
Add a minimal Ollama provider skeleton with settings model (BaseUrl, Model, optional parameters). Implement a stub `ListModelsAsync` and `TranslateAsync` that returns placeholder results with TODOs.

**Acceptance Criteria**
- `OllamaSettings` model exists in a common/shared project.
- `OllamaTranslationProvider` implements `ITranslationProvider` with TODOs.
- No network calls or external dependencies.

**Files to touch**
- `Witcher3StringEditor.Common/Translation/` (provider + settings models)
- `docs/integrations.md` (link to the stub)

**QA Checklist**
- ✅ Build succeeds (`dotnet build`).
- ✅ Provider registry can register the stub.
- ✅ No UI changes beyond placeholders.

---

### Issue 3: Translation memory interfaces + local store stub
**Description**
Define interfaces and a stub local store for translation memory and QA metadata. Propose a minimal SQLite schema (or JSON structure) and add bootstrap placeholders.

**Acceptance Criteria**
- `ITranslationMemoryStore` interface added.
- Stub implementation added with TODOs for storage.
- Schema proposal documented (SQLite tables or JSON structure).

**Files to touch**
- `Witcher3StringEditor.Common/TranslationMemory/` (new folder)
- `docs/integrations.md` (schema notes)

**QA Checklist**
- ✅ Build succeeds (`dotnet build`).
- ✅ No runtime usage unless explicitly enabled.
- ✅ No external services required.

---

### Issue 4: Terminology & style pack loaders
**Description**
Add models for terminology/style packs and implement minimal local loaders for `.csv`, `.tsv`, and `.md` files. Provide sample files for validation and QA.

**Acceptance Criteria**
- `TerminologyPack` model exists.
- Minimal parsers for `.csv`, `.tsv`, `.md` added.
- Sample files added under `docs/samples/`.

**Files to touch**
- `Witcher3StringEditor.Common/Terminology/` (new folder)
- `docs/samples/`
- `docs/integrations.md`

**QA Checklist**
- ✅ Build succeeds (`dotnet build`).
- ✅ Loaders parse samples without exceptions.
- ✅ No translation behavior changes.

---

### Issue 5: Translation profiles (local settings)
**Description**
Define a profile model that groups provider selection, model, TM settings, and terminology/style choices. Add persistence stubs using existing settings mechanisms.

**Acceptance Criteria**
- `TranslationProfile` model exists.
- Settings persistence stub added (local-only).
- Profiles are inert unless explicitly used.

**Files to touch**
- `Witcher3StringEditor.Common/TranslationProfiles/` (new folder)
- Settings persistence file(s) once identified
- `docs/integrations.md`

**QA Checklist**
- ✅ Build succeeds (`dotnet build`).
- ✅ Existing settings load/save unchanged.
- ✅ No UI changes beyond placeholders.

---

### Issue 6: UI placeholders for provider/model selection
**Description**
Add minimal UI placeholders for selecting provider, model, and terminology packs. Wire to settings but keep translation flow unchanged.

**Acceptance Criteria**
- UI placeholders added with TODO labels.
- Settings binding exists but no behavior change.
- Existing translation flow still uses `ITranslator`.

**Files to touch**
- Dialog/view models for translation settings
- `docs/integrations.md`

**QA Checklist**
- ✅ Build succeeds (`dotnet build`).
- ✅ Translation dialog still works.
- ✅ No regressions in existing UI.

## Integration Notes

- Keep each PR small, focused, and compilation-safe.
- Default behavior remains `ITranslator` until provider routing is implemented.
- All storage must remain local and optional.
