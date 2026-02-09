# Integrations Architecture & Roadmap

## Overview
The integration roadmap focuses on adding local, opt-in capabilities without changing the existing translation flow. The current plan relies on small, compile-safe abstractions so future work can plug in concrete implementations for translation memory, provider selection, terminology/style, and profiles without large refactors.

## Architecture Goals
- Keep integrations isolated behind interfaces and stubs.
- Store all data locally (SQLite/JSON).
- Avoid wiring into Settings or translation flows until the related issue is complete.

## Planned Components
### Translation Memory (SQLite-backed)
- Use a local SQLite schema for translation memory entries.
- Keep initialization and data access behind `ITranslationMemoryStore` and related abstractions.
- Add a lightweight bootstrapper to ensure database creation and migrations are centralized.
- Add a migration plan interface to describe future schema evolution without wiring to runtime yet.

### Ollama Provider & Model Selection
- Build on the existing provider stub to list models when enabled.
- Keep HTTP integrations behind the Ollama provider with no cross-cutting dependencies.
- Introduce DTOs for model list requests/responses as placeholders for future API wiring.

### Terminology & Style Loading
- Load terminology and style assets from local files (`.csv`, `.tsv`, `.md`).
- Provide loader and validation stubs for later enforcement logic.
- Add source descriptors to represent terminology/style inputs without changing loaders yet.

### Translation Profiles
- Maintain a catalog of profiles stored locally (JSON).
- Provide preview and selection services without changing core translation behavior.
- Add a summary catalog interface to support lightweight profile listing for settings or selection flows.
- Add a validation interface for future profile verification without runtime wiring.

## Deliverables & Sequencing
- See `docs/task-breakdown.md` for issue-level tasks, acceptance criteria, and QA checklists.
- Phase 0 scaffolding status: `docs/inspections/phase0-status.md`.
