# Integrations Plan (TM, Ollama, Terminology, Profiles)

## Architecture overview (stub-first)
- **Provider contracts**
  - Use the existing provider interfaces in `Witcher3StringEditor.Integrations.Providers`.
  - The Ollama provider in `Witcher3StringEditor.Integrations.Ollama` remains a stub with no HTTP calls.
- **Local-only storage**
  - Translation memory and profiles remain local (SQLite/JSON) when implemented.
  - No external services are introduced; local file paths are used for configuration.
  - Translation memory service/store interfaces live in `Witcher3StringEditor/Integrations/Storage` and remain unused.
- **Terminology + style packs**
  - Loader interfaces live under `Witcher3StringEditor.Integrations.Terminology` and return in-memory packs.
  - A minimal parsing stub supports TSV/CSV terminology and Markdown style guides (no UI/DI wiring).
  - Sample files live under `docs/samples/` to validate parsing without integration wiring.
- **Translation profiles**
  - Profile models live under `Witcher3StringEditor/Integrations/Profiles`.
  - JSON-backed profile storage remains local-only and returns empty lists when missing files are present.
- **No UI/DI wiring**
  - Integration scaffolding stays inert: no DI registration and no UI changes.


## Phase 0: compile-safe scaffolding
Phase 0 focuses on compile-safe scaffolding and documentation updates only.

**Phase 0 constraints**
- No full feature implementation.
- No large refactors.
- No behavior changes unless explicitly marked as placeholders.
- No external services; local-only storage (e.g., SQLite/JSON) when applicable.
- UI/UX changes must be minimal placeholders for settings panels.
- Changes must compile (or be explicitly deferred to a follow-up issue).

## Planned work as GitHub issues

### Issue 0: Inventory pass for authoritative integration models
**Description**
Locate the source-of-truth models for translation profiles, terminology, and translation memory (Common vs Integrations)
before wiring any runtime flows.

**Acceptance Criteria**
- Inventory report lists authoritative namespaces and models for each integration area.
- Potential conflicts (Common vs Integrations namespace duplicates) are documented.
- Follow-up tasks are added when ownership is unclear.

**Files to touch**
- `docs/inspections/inventory.md`
- `docs/integrations.md`


**Behavior Impact**
- Default: no runtime behavior change; scaffolding only.
- UI: placeholders only when explicitly called out (no workflow changes).

**Build Commands**
- `dotnet build`

**Reminder (Phase 0)**
- Keep changes compile-safe scaffolding only (stubs/interfaces/TODOs).
- No full feature implementation, no external services, local-only storage.

**QA checklist**
- Build: `dotnet build`.
- Manual: open the inventory report and confirm each area has file paths.
- No regressions: documentation-only changes.

---

### Issue 1: Ollama provider stub + settings model
**Description**
Provide a stub Ollama provider and a settings model with BaseUrl, ModelName, and related fields. No DI or UI wiring.

**Acceptance Criteria**
- Ollama provider returns stub results for `ListModelsAsync` and `TranslateAsync`.
- `OllamaSettings` includes BaseUrl, ModelName, and optional tuning fields.
- No DI registration or UI changes are introduced.

**Files to touch**
- `Witcher3StringEditor.Integrations.Ollama/OllamaTranslationProvider.cs`
- `Witcher3StringEditor.Integrations.Ollama/OllamaSettings.cs`
- `docs/integrations.md`


**Behavior Impact**
- Default: no runtime behavior change; scaffolding only.
- UI: placeholders only when explicitly called out (no workflow changes).

**Build Commands**
- `dotnet build`

**Reminder (Phase 0)**
- Keep changes compile-safe scaffolding only (stubs/interfaces/TODOs).
- No full feature implementation, no external services, local-only storage.

**QA checklist**
- Build: `dotnet build`.
- Manual: inspect the provider to confirm no HTTP calls are made.
- No regressions: translation flow remains unchanged.

---

### Issue 2: Translation memory local storage scaffolding
**Description**
Add local-only translation memory scaffolding (interfaces + stubs) without runtime wiring.

**Acceptance Criteria**
- A stub translation memory store returns empty results and accepts saves.
- A stub translation memory service returns empty results and accepts saves.
- No runtime wiring; the store remains unused by production code.
- All new types are documented in the integrations plan.

**Files to touch**
- `Witcher3StringEditor/Integrations/Storage/ITranslationMemoryStore.cs`
- `Witcher3StringEditor/Integrations/Storage/ITranslationMemoryService.cs`
- `Witcher3StringEditor/Integrations/Storage/StubTranslationMemoryStore.cs`
- `Witcher3StringEditor/Integrations/Storage/StubTranslationMemoryService.cs`
- `docs/integrations.md`


**Behavior Impact**
- Default: no runtime behavior change; scaffolding only.
- UI: placeholders only when explicitly called out (no workflow changes).

**Build Commands**
- `dotnet build`

**Reminder (Phase 0)**
- Keep changes compile-safe scaffolding only (stubs/interfaces/TODOs).
- No full feature implementation, no external services, local-only storage.

**QA checklist**
- Build: `dotnet build`.
- Manual: confirm the stub store is not registered in DI.
- No regressions: app behavior is unchanged.

---

### Issue 3: Terminology + style pack loader scaffolding (minimal parsing)
**Description**
Provide minimal parsing stubs for terminology and style guides with TSV/CSV and Markdown support.

**Acceptance Criteria**
- Stub loaders implement `ITerminologyLoader` and `IStyleGuideLoader`.
- Loaders parse basic TSV/CSV terminology rows and Markdown style guide sections.
- Sample files under `docs/samples/` parse without runtime wiring or UI changes.
- No UI or runtime wiring is added.

**Files to touch**
- `Witcher3StringEditor/Integrations/Terminology/ITerminologyLoader.cs`
- `Witcher3StringEditor/Integrations/Terminology/IStyleGuideLoader.cs`
- `Witcher3StringEditor/Integrations/Terminology/StubTerminologyLoader.cs`
- `docs/samples/*`
- `docs/integrations.md`


**Behavior Impact**
- Default: no runtime behavior change; scaffolding only.
- UI: placeholders only when explicitly called out (no workflow changes).

**Build Commands**
- `dotnet build`

**Reminder (Phase 0)**
- Keep changes compile-safe scaffolding only (stubs/interfaces/TODOs).
- No full feature implementation, no external services, local-only storage.

**QA checklist**
- Build: `dotnet build`.
- Manual: inspect the stub loader to confirm parsing is minimal and TODOs remain.
- No regressions: terminology usage remains unchanged.

---

### Issue 4: Translation profile model + JSON store scaffolding
**Description**
Define a minimal translation profile model and add a JSON-backed store that returns empty lists when the file is missing.
Keep the store inert by default (no DI/UI wiring).

**Acceptance Criteria**
- Translation profile model includes core identifiers, provider/model, and optional terminology/style/tm fields.
- JSON-backed profile store returns empty lists when the file is missing.
- No runtime wiring or UI additions are introduced.

**Files to touch**
- `Witcher3StringEditor/Integrations/Profiles/TranslationProfile.cs`
- `Witcher3StringEditor/Integrations/Profiles/JsonTranslationProfileStore.cs`
- `docs/integrations.md`


**Behavior Impact**
- Default: no runtime behavior change; scaffolding only.
- UI: placeholders only when explicitly called out (no workflow changes).

**Build Commands**
- `dotnet build`

**Reminder (Phase 0)**
- Keep changes compile-safe scaffolding only (stubs/interfaces/TODOs).
- No full feature implementation, no external services, local-only storage.

**QA checklist**
- Build: `dotnet build`.
- Manual: inspect the JSON store to confirm missing-file behavior.
- No regressions: default translation flow remains unchanged.
