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

### Ollama Provider & Model Selection
- Build on the existing provider stub to list models when enabled.
- Keep HTTP integrations behind the Ollama provider with no cross-cutting dependencies.

### Terminology & Style Loading
- Load terminology and style assets from local files (`.csv`, `.tsv`, `.md`).
- Provide loader and validation stubs for later enforcement logic.

### Translation Profiles
- Maintain a catalog of profiles stored locally (JSON).
- Provide preview and selection services without changing core translation behavior.

## Deliverables & Sequencing
- See `docs/task-breakdown.md` for issue-level tasks, acceptance criteria, and QA checklists.
