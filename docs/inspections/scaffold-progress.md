# Phase 0 Scaffold Progress

Last updated: 2026-02-11

## How to update
- When checking an item, include the completion date and a short note in parentheses (e.g., `[x] Define ... (2026-02-10: stubbed interfaces added)`).
- Update the `Last updated` date when any item changes.

## Providers
- [x] Define translation provider contracts (interfaces + request/response DTOs). (2026-02-09: `ITranslationProvider` contracts and DTOs live in `Witcher3StringEditor.Common/Translation`.)
- [x] Add inert provider registry/resolver scaffolding. (2026-02-09: `ITranslationProviderRegistry` + in-memory/legacy stubs added in `Witcher3StringEditor.Common/Translation` and `Witcher3StringEditor/Services`.)
- [x] Insert TODO hook locations for provider calls (no routing changes). (2026-02-09: `TranslationRouter`/`LegacyTranslationRouter` contain provider hooks with fallback behavior.)

## Translation memory (TM)
- [x] Define TM storage interfaces and DTOs. (2026-02-09: `Witcher3StringEditor.Common/TranslationMemory` contains stubs and DTOs.)
- [x] Add local persistence stub (SQLite/JSON placeholder only). (2026-02-09: SQLite store + bootstrap stubs in `Witcher3StringEditor.Data/TranslationMemory`.)
- [x] Document expected schema and migration strategy (no runtime wiring). (2026-02-09: schema in `SqliteBootstrap` and migration planner interface in `ITranslationMemoryMigrationPlanner`.)

## Translation profiles
- [x] Define profile models and validation interfaces. (2026-02-09: models + validators in `Witcher3StringEditor.Common/Profiles`.)
- [x] Add local profile catalog/loader placeholder (JSON stub). (2026-02-09: JSON store and stub loader in `Witcher3StringEditor.Data/Profiles` and `Witcher3StringEditor/Integrations/Profiles`.)
- [ ] Document intended UI entry points without wiring. (Pending: add a short section in `docs/integrations.md` or a dedicated profiles doc describing the settings dialog entry point.)

## Terminology & style
- [x] Define terminology/style models and source descriptors. (2026-02-09: models + source descriptor in `Witcher3StringEditor.Common/Terminology`.)
- [x] Add loader stubs for CSV/TSV/Markdown sources. (2026-02-09: `StubTerminologyLoader` supports CSV/TSV/MD parsing.)
- [x] Add TODO placeholders for prompt injection and QA checks. (2026-02-09: terminology prompt builder stub includes TODO for injection.)

## SafeFireAndForget
- [ ] Add centralized helper placeholder with TODO behavior. (Pending: add a stub helper class and note expected usage.)
- [ ] Document intended usage sites (no behavior changes). (Pending: add a short inventory note listing candidate call sites.)

## Docs
- [x] Update `docs/integrations.md` with architecture notes and planned tasks. (2026-02-09: roadmap and sequencing captured.)
- [x] Update `docs/task-breakdown.md` with issue-level breakdowns. (2026-02-09: issue list and acceptance criteria captured.)
- [x] Add inspection notes and inventory findings in `docs/inspections/`. (2026-02-09: inventory + inspection notes present.)

## Pending P0 checklist additions
- [ ] P0-28: Document translation memory database settings + initializer stubs (no runtime wiring).
- [ ] P0-29: Add Ollama model catalog + selection placeholders (DTOs/stubs only).
- [ ] P0-30: Provide terminology/style pack loader placeholders (CSV/TSV/Markdown).
- [ ] P0-31: Define translation profile persistence + selection stubs (local-only, inert).
- [ ] P0-33: Add provider failure DTO scaffolding for diagnostics (metadata only).
- [ ] P0-34: Add QA diagnostic DTO scaffolding (code, message, severity, location).
- [ ] P0-35: Add SafeFireAndForget helper placeholder + usage notes (no behavior change).
