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

## Authoritative Locations (Confirmed)
Use the Common project for contracts and DTOs so Services, Data, UI, and Integrations all share a single source of
truth. Keep implementations/stubs in Services or Data as needed, but do not duplicate models outside Common.

| Model group | Authoritative namespace/folder | Rationale |
| --- | --- | --- |
| Translation providers | `Witcher3StringEditor.Common.Translation` (`Witcher3StringEditor.Common/Translation`) | Provider contracts and DTOs already live here and flow through registries/routers, preventing parallel schemas. |
| Terminology & style | `Witcher3StringEditor.Common.Terminology` (`Witcher3StringEditor.Common/Terminology`) | Common hosts terminology and style models consumed by Services; keeping them here prevents drift. |
| Translation profiles | `Witcher3StringEditor.Common.Profiles` (`Witcher3StringEditor.Common/Profiles`) | Centralizes profile models for UI and pipeline context builders, avoiding duplicate definitions. |
| Translation memory (TM) | `Witcher3StringEditor.Common.TranslationMemory` (`Witcher3StringEditor.Common/TranslationMemory`) | Common already defines TM interfaces/settings used by Services/Data stubs. |

### Do Not Use (Duplicate or Legacy Locations)
Do not introduce new models in these locations; use them only for inventory or migration work.

- `Witcher3StringEditor.Integrations.Providers` (`Witcher3StringEditor/Integrations/Providers`)
  - **Rationale:** Duplicates provider DTOs/registry contracts that already exist in Common, risking split schemas.
- `Witcher3StringEditor.Integrations.Terminology` (`Witcher3StringEditor/Integrations/Terminology`)
  - **Rationale:** Overlaps terminology pack models/loader surfaces in Common, increasing drift risk.
- `Witcher3StringEditor.Integrations.Profiles` (`Witcher3StringEditor/Integrations/Profiles`)
  - **Rationale:** Parallel profile surfaces conflict with Common profile models used by UI and Services.
- `Witcher3StringEditor.Integrations.Storage` (`Witcher3StringEditor/Integrations/Storage`)
  - **Rationale:** Duplicates TM model surfaces already defined in `Common.TranslationMemory`.

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
- Scaffold checklist (Phase 0): `docs/inspections/scaffold-progress.md`.

## Decision Log (P0)
**Decision (Model ownership):** The Common project is the authoritative home for translation-related contracts and DTOs
(profiles, terminology/style, providers, translation memory). Duplicate `Integrations/*` model surfaces are legacy only
and should not be extended for new work. See `docs/inspections/model-ownership.md` for details.

## Next Steps
- Execute the remaining issue drafts in `docs/task-breakdown.md` in order, keeping changes compile-safe and inert.
- When moving into Phase 1 wiring, start by removing reliance on legacy `Integrations/*` models and use the Common
  contracts exclusively.
- Reconfirm the scaffold checklist in `docs/inspections/scaffold-progress.md` before any runtime wiring begins.
