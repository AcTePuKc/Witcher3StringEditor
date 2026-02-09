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

## Planned Components
### Translation Memory (SQLite-backed)
- Use a local SQLite schema for translation memory entries.
- Keep initialization and data access behind `ITranslationMemoryStore` and related abstractions.
- Add a store factory stub to construct a SQLite-backed store without runtime wiring.
- Track schema evolution via a migration plan interface.

### Ollama Provider & Model Selection
- Build on the existing provider stub to list models when enabled.
- Keep HTTP integrations behind the Ollama provider with no cross-cutting dependencies.
- Track model selection as an opt-in setting (UI placeholders only).

### Terminology & Style Loading
- Define loader interfaces and no-op stubs for local files (`.csv`, `.tsv`, `.md`).
- Keep parsing deterministic and local-only.
- Add placeholder validation interfaces for later enforcement logic.

### Translation Profiles
- Define interfaces and no-op stubs for a local profile catalog.
- Provide preview and selection abstractions without changing core translation behavior.
- Keep profile storage local (JSON) and inert by default.

### Provider Failure Reporting (DTO-only)
- Capture provider failure metadata in a DTO for future diagnostics.
- Keep this data model unused until a logging/telemetry path is approved.

## Deliverables & Sequencing
- See `docs/task-breakdown.md` for issue-level tasks, acceptance criteria, and QA checklists.
- Phase 0 scaffolding status: `docs/inspections/phase0-status.md`.
