# Phase 0 Scaffold Progress

Last updated: 2026-02-09

## How to update
- When checking an item, include the completion date and a short note in parentheses (e.g., `[x] Define ... (2026-02-10: stubbed interfaces added)`).
- Update the `Last updated` date when any item changes.

## Providers
- [ ] Define translation provider contracts (interfaces + request/response DTOs).
- [ ] Add inert provider registry/resolver scaffolding.
- [ ] Insert TODO hook locations for provider calls (no routing changes).

## Translation memory (TM)
- [ ] Define TM storage interfaces and DTOs.
- [ ] Add local persistence stub (SQLite/JSON placeholder only).
- [ ] Document expected schema and migration strategy (no runtime wiring).

## Translation profiles
- [ ] Define profile models and validation interfaces.
- [ ] Add local profile catalog/loader placeholder (JSON stub).
- [ ] Document intended UI entry points without wiring.

## Terminology & style
- [ ] Define terminology/style models and source descriptors.
- [ ] Add loader stubs for CSV/TSV/Markdown sources.
- [ ] Add TODO placeholders for prompt injection and QA checks.

## SafeFireAndForget
- [ ] Add centralized helper placeholder with TODO behavior.
- [ ] Document intended usage sites (no behavior changes).

## Docs
- [ ] Update `docs/integrations.md` with architecture notes and planned tasks.
- [ ] Update `docs/task-breakdown.md` with issue-level breakdowns.
- [ ] Add inspection notes and inventory findings in `docs/inspections/`.
