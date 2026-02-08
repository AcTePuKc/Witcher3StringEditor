# Integrations Plan (TM, Ollama, Terminology, Profiles)

## Architecture overview (stub-first)
- **Provider contracts**
  - Use the existing provider interfaces in `Witcher3StringEditor.Integrations.Providers`.
  - The Ollama provider in `Witcher3StringEditor.Integrations.Ollama` remains a stub with no HTTP calls.
- **Local-only storage**
  - Translation memory and profiles remain local (SQLite/JSON) when implemented.
  - No external services are introduced; local file paths are used for configuration.
- **Terminology + style packs**
  - Loader interfaces live under `Witcher3StringEditor.Integrations.Terminology` and return in-memory packs.
  - A stub loader provides empty packs until real parsers are introduced.
- **No UI/DI wiring**
  - Integration scaffolding stays inert: no DI registration and no UI changes.

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
- No runtime wiring; the store remains unused by production code.
- All new types are documented in the integrations plan.

**Files to touch**
- `Witcher3StringEditor/Integrations/Storage/ITranslationMemoryStore.cs`
- `Witcher3StringEditor/Integrations/Storage/StubTranslationMemoryStore.cs`
- `docs/integrations.md`

**QA checklist**
- Build: `dotnet build`.
- Manual: confirm the stub store is not registered in DI.
- No regressions: app behavior is unchanged.

---

### Issue 3: Terminology + style pack loader scaffolding
**Description**
Provide stub loaders for terminology and style guides that return empty packs.

**Acceptance Criteria**
- Stub loaders implement `ITerminologyLoader` and `IStyleGuideLoader`.
- Loaders return empty packs and do not parse files yet.
- No UI or runtime wiring is added.

**Files to touch**
- `Witcher3StringEditor/Integrations/Terminology/ITerminologyLoader.cs`
- `Witcher3StringEditor/Integrations/Terminology/IStyleGuideLoader.cs`
- `Witcher3StringEditor/Integrations/Terminology/StubTerminologyLoader.cs`
- `docs/integrations.md`

**QA checklist**
- Build: `dotnet build`.
- Manual: inspect the stub loader to confirm it is no-op.
- No regressions: terminology usage remains unchanged.

---

### Issue 4: Translation profile storage scaffolding
**Description**
Provide a stub profile store so profile persistence can be added later.

**Acceptance Criteria**
- Stub profile store returns an empty list and accepts saves.
- No runtime wiring or UI additions are introduced.
- Profile model remains local and future-facing.

**Files to touch**
- `Witcher3StringEditor/Integrations/Profiles/ITranslationProfileStore.cs`
- `Witcher3StringEditor/Integrations/Profiles/StubTranslationProfileStore.cs`
- `docs/integrations.md`

**QA checklist**
- Build: `dotnet build`.
- Manual: verify the stub store is unused in the app.
- No regressions: default translation flow remains unchanged.
