# Scaffold Audit: Common / Services / Data Stubs

## Scope
This audit scans the existing scaffolding in `Witcher3StringEditor.Common`, `Witcher3StringEditor/Services`, and
`Witcher3StringEditor.Data`, then calls out duplicate or overlapping stubs across namespaces (including integration
layers) so follow-up work can consolidate safely.

## Summary Highlights
- Multiple translation provider registry abstractions exist across Common, Services, and Integrations, with both legacy
  and in-memory stubs in parallel.
- Translation memory has two parallel interface/model families (Common vs Integrations) plus Services no-op stubs and a
  Data SQLite implementation that is not wired to runtime yet.
- Translation profile stores and catalogs exist in both Common/Data (JSON-backed) and Services/Integrations (no-op),
  creating duplication and unclear ownership.
- Terminology loaders and style guides exist in Common/Services and in Integrations (stub parsing) with overlapping
  responsibilities.

## Existing Stubs & Scaffolds

### Translation Providers
- `Witcher3StringEditor.Common.Translation.InMemoryTranslationProviderRegistry`: in-memory registry stub.
- `Witcher3StringEditor.Common.Translation.TranslationProviderRegistry`: legacy registry with TODOs pointing to
  `Witcher3StringEditor.Services.TranslationProviderRegistry`.
- `Witcher3StringEditor.Services.NoopTranslationProviderRegistry`: no-op registry returning empty descriptors.
- `Witcher3StringEditor.Services.TranslationProviderRegistry`: active registry that de-dupes providers.
- `Witcher3StringEditor.Integrations.Providers.*`: separate provider interfaces and in-memory registry.

**Overlap / duplication**
- There are *two* provider interface families:
  - `Witcher3StringEditor.Common.Translation` (used by Services registry).
  - `Witcher3StringEditor.Integrations.Providers` (separate types and registry).
- There are *three* registry classes with similar responsibilities:
  - Common `TranslationProviderRegistry` (legacy),
  - Common `InMemoryTranslationProviderRegistry` (stub),
  - Services `TranslationProviderRegistry` and `NoopTranslationProviderRegistry`.

### Translation Memory
- `Witcher3StringEditor.Common.TranslationMemory`: interfaces and settings models (`ITranslationMemoryStore`,
  `ITranslationMemoryService`, `TranslationMemorySettings`, etc.).
- `Witcher3StringEditor.Services.NoopTranslationMemoryStore` and `NoopTranslationMemoryStoreFactory`: placeholder
  storage wiring.
- `Witcher3StringEditor.Services.NoopTranslationMemoryService` and `NoopTranslationMemoryImportService`: orchestration
  stubs.
- `Witcher3StringEditor.Data.TranslationMemory`: SQLite-backed initializer and store factory (not wired).
- `Witcher3StringEditor.Integrations.Storage`: a *second* translation memory model and interface family.

**Overlap / duplication**
- There are *two* separate translation memory APIs:
  - `Witcher3StringEditor.Common.TranslationMemory` (used by Services/Data stubs).
  - `Witcher3StringEditor.Integrations.Storage` (separate models and methods).
- Services include no-op stores and services *alongside* Data SQLite implementations, implying a pending decision on
  which layer owns runtime wiring.

### Terminology & Style Guides
- `Witcher3StringEditor.Common.Terminology`: interfaces and models (`ITerminologyLoader`, `IStyleGuideLoader`,
  `TerminologyPack`, `StyleGuide`, etc.).
- `Witcher3StringEditor.Services.TerminologyLoader`: concrete loader for `.csv`, `.tsv`, `.md`.
- `Witcher3StringEditor.Services.NoopTerminologyPromptBuilder` and `NoopTerminologyValidationService`: placeholders.
- `Witcher3StringEditor.Integrations.Terminology.StubTerminologyLoader`: stub loader that also parses files and style
  guide sections.

**Overlap / duplication**
- `Services.TerminologyLoader` and `Integrations.Terminology.StubTerminologyLoader` both parse terminology and style
  guides, but live in different namespaces with different model types.

### Translation Profiles
- `Witcher3StringEditor.Common.Profiles`: interfaces and models (`ITranslationProfileStore`,
  `ITranslationProfileCatalog`, `TranslationProfile`), with JSON settings in the Common model.
- `Witcher3StringEditor.Data.Profiles`: JSON-backed store and catalog (`JsonTranslationProfileStore`,
  `TranslationProfileCatalog`).
- `Witcher3StringEditor.Services.*`: no-op store, catalog, resolver, selection, and preview services.
- `Witcher3StringEditor.Integrations.Profiles`: separate profile store/loader interfaces and stubs.

**Overlap / duplication**
- There are *two* profile store interface families:
  - `Witcher3StringEditor.Common.Profiles.ITranslationProfileStore` (List/Get).
  - `Witcher3StringEditor.Integrations.Profiles.ITranslationProfileStore` (GetAll/Save/Delete).
- Data already provides JSON-backed storage in `Witcher3StringEditor.Data.Profiles`, while Services contain no-op
  implementations, signaling a pending decision about which store should be wired.

## Notes for Follow-Up
- Decide whether the Integrations namespace is the future public surface or if Common/Services should own the runtime
  types, then remove/bridge duplicates accordingly.
- Consolidate provider registry and translation memory interfaces to prevent multiple incompatible registries.
- Confirm whether `Witcher3StringEditor.Data` JSON/SQLite implementations should replace Services no-ops once runtime
  wiring is approved.
