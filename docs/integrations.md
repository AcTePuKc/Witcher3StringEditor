# Integrations Architecture & Roadmap

## Overview
The integration roadmap focuses on adding local, opt-in capabilities without changing the existing translation flow. The
current plan relies on small, compile-safe abstractions so future work can plug in concrete implementations for
translation memory, provider selection (including Ollama model selection), terminology/style loading, and translation
profiles without large refactors.

## Architecture Goals
- Keep integrations isolated behind interfaces and stubs.
- Store all data locally (SQLite/JSON).
- Avoid wiring into Settings or translation flows until the related issue is complete.
- Favor compile-safe placeholders over functional parsing or runtime logic.
- Represent style guides with explicit sections/rule lists to support future validation.

## Planned Components
### Translation Memory (SQLite-backed)
- Use a local SQLite schema for translation memory entries.
- Keep initialization and data access behind `ITranslationMemoryStore` and related abstractions.
- Add a store factory stub to construct a SQLite-backed store without runtime wiring.
- Track schema evolution via a migration plan interface.
- Represent feature toggles and storage configuration via `TranslationMemorySettings` (`Enabled`, `DatabasePath`).
- Prefer async methods like `LookupAsync` and `UpsertAsync` for future database-backed workflows (no registration yet).

### Ollama Provider & Model Selection
- Build on the existing provider stub to list models when enabled.
- Keep HTTP integrations behind the Ollama provider with no cross-cutting dependencies.
- Track model selection as an opt-in setting (UI placeholders only).
- Model summaries include `Id`, `DisplayName`, and `IsDefault` for future default selection behavior.

### Terminology & Style Loading
- Define loader interfaces and no-op stubs for local files (`.csv`, `.tsv`, `.md`).
- Keep parsing deterministic and local-only.
- Use a `TerminologyEntry` model with `Term`, `Translation`, `Notes`, and `Mode` fields to support future validation.
- Extend `StyleGuide` to include section/rule lists alongside required/forbidden/tone buckets.
- Add placeholder validation interfaces for later enforcement logic.

### Translation Profiles
- Define interfaces and no-op stubs for a local profile catalog.
- Provide preview and selection abstractions without changing core translation behavior.
- Keep profile storage local (JSON) and inert by default.

### UI Placeholders & Settings Surfaces (No Wiring)
- Settings dialog already includes placeholders for profile selection, translation memory toggles, terminology packs,
  and style guide file paths; keep these as the primary UI surfaces for future wiring.
- Provider/model selection UI exists in the same settings view, but model discovery should remain inert until Phase 1.

### Provider Failure Reporting (DTO-only)
- Capture provider failure metadata in a DTO for future diagnostics.
- Keep this data model unused until a logging/telemetry path is approved.

### QA Diagnostics Reporting (DTO-only)
- Capture QA diagnostics as a DTO with `Code`, `Message`, `Severity`, and `Location`.
- Keep this data model unused until QA reporting requirements are defined.

## Deliverables & Sequencing
- See `docs/task-breakdown.md` for issue-level tasks, acceptance criteria, and QA checklists.
- Phase 0 scaffolding status: `docs/inspections/phase0-status.md`.
