# Implementation Notes (Phase 0 Scaffolding)

This document captures **implementation-level details** for Phase 0 scaffolding. It intentionally avoids full feature
implementation and keeps all changes compile-safe and inert by default.

## Scope (Phase 0)
- Interfaces, DTOs, and stub implementations only.
- Local-only storage (SQLite/JSON) where persistence is needed.
- Documentation updates that clarify future wiring without changing behavior.

## Explicit Do/Don’t Rules
**Do**
- Add compile-safe interfaces, DTOs, and stubs with TODO markers.
- Keep all integrations inert by default; defer wiring and behavior changes.
- Prefer local-only storage paths (AppData, local JSON, or SQLite).
- Add minimal placeholders for settings panels if absolutely necessary.

**Don’t**
- Do not change translation routing behavior.
- Do not change UI behavior (placeholders only, no workflow changes).
- Do not introduce external services or network dependencies.
- Do not refactor existing translation flows.

## Suggested Placement (if unsure, create an inventory task)
- **Contracts**: `Witcher3StringEditor.Common/*`
- **Local storage**: `Witcher3StringEditor.Data/*`
- **Integrations**: `Witcher3StringEditor.Integrations.*/*`
- **Runtime wiring (future)**: `Witcher3StringEditor/Services/*`
- **UI placeholders (future)**: `Witcher3StringEditor.Dialogs/*`

## Follow-up Tasks (Examples)
- Add provider registry and model catalog stubs (no routing changes).
- Add translation memory database initializer interface + SQLite stub.
- Add terminology/style loaders (TSV/CSV/Markdown parsing stubs).
- Add translation profile model + JSON store stub.

## QA Expectations (Phase 0)
- Build succeeds (`dotnet build`).
- No new UI behavior or routing changes.
- No external network calls or services introduced.

## Phase 0 Exit Criteria
Phase 0 is complete only when **all items below are checked**. Phase 1 is **blocked** until then.

**Required stubs/checks**
- [ ] Provider interfaces (translation provider contracts + request/response DTOs).
- [ ] Provider registry/resolver (inert by default; no routing changes).
- [ ] Translation profiles stubs (models + local catalog/loader placeholders).
- [ ] Translation memory (TM) stubs (interfaces + local storage placeholders).
- [ ] Terminology/style stubs (models + loaders or parsers; no enforcement).
- [ ] `SafeFireAndForget` placeholder (centralized helper with TODO behavior).
- [ ] Inspections scaffolding (inspection models + TODO hooks, no runtime changes).
