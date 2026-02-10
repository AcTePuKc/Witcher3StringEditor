# Model Ownership Map

## Purpose
This document identifies the authoritative model/contract locations for translation-related feature groups (profiles,
terminology, translation providers, translation memory). It also flags duplicate or legacy locations that should not be
used for new work, along with rationale so future changes stay consistent and compile-safe.

## Authoritative model locations
| Model group | Authoritative namespace/folder | Rationale |
| --- | --- | --- |
| Profiles | `Witcher3StringEditor.Common.Profiles` (`Witcher3StringEditor.Common/Profiles`) | The Common project hosts the app-wide contracts and DTOs consumed by Services and UI. Keeping profile models here avoids duplicate definitions and keeps pipeline/context builders aligned with a single source of truth. |
| Terminology & style | `Witcher3StringEditor.Common.Terminology` (`Witcher3StringEditor.Common/Terminology`) | Common already defines loader interfaces and terminology/style models that Services use at runtime. Consolidating on this namespace prevents parallel model sets from drifting. |
| Translation providers | `Witcher3StringEditor.Common.Translation` (`Witcher3StringEditor.Common/Translation`) | Provider contracts, request/response DTOs, and model discovery DTOs live in Common and are wired through registries and routers. New provider-related models should extend these types to avoid fragmentation. |
| Translation memory (TM) | `Witcher3StringEditor.Common.TranslationMemory` (`Witcher3StringEditor.Common/TranslationMemory`) | Common defines the TM interfaces and settings used across Services and Data stubs. This keeps storage wiring compile-safe and prevents duplicate store/entry definitions. |

## Current duplicate model locations (profiles & terminology)
These duplicates exist today and should not be extended. Use them only for inventory or migration tasks.

### Profiles
- **Authoritative**
  - `Witcher3StringEditor.Common/Profiles/TranslationProfile.cs`
  - `Witcher3StringEditor.Common/Profiles/TranslationProfileSummary.cs`
  - `Witcher3StringEditor.Common/Profiles/ITranslationProfileStore.cs`
- **Duplicate (non-authoritative)**
  - `Witcher3StringEditor/Integrations/Profiles/TranslationProfile.cs`
  - `Witcher3StringEditor/Integrations/Profiles/TranslationProfileSummary.cs`
  - `Witcher3StringEditor/Integrations/Profiles/ITranslationProfileStore.cs`

### Terminology & style
- **Authoritative**
  - `Witcher3StringEditor.Common/Terminology/TerminologyModels.cs`
  - `Witcher3StringEditor.Common/Terminology/ITerminologyLoader.cs`
  - `Witcher3StringEditor.Common/Terminology/IStyleGuideLoader.cs`
- **Duplicate (non-authoritative)**
  - `Witcher3StringEditor/Integrations/Terminology/TerminologyModels.cs`
  - `Witcher3StringEditor/Integrations/Terminology/ITerminologyLoader.cs`

## Do not use (duplicate or legacy locations)
Use these only for inventory or migration tasks; do not introduce new models here.

- `Witcher3StringEditor.Integrations.Profiles` (`Witcher3StringEditor/Integrations/Profiles`)
  - **Why not:** Contains a parallel profile model/store surface that overlaps the Common models. Continuing here would
    split storage and pipeline ownership.
- `Witcher3StringEditor.Integrations.Terminology` (`Witcher3StringEditor/Integrations/Terminology`)
  - **Why not:** Duplicates terminology pack models and loader interfaces already defined in Common and implemented in
    Services; risks inconsistent parsing and prompt injection behavior.
- `Witcher3StringEditor.Integrations.Providers` (`Witcher3StringEditor/Integrations/Providers`)
  - **Why not:** Duplicates provider request/response DTOs and registry interfaces that are already standardized in
    Common and used by runtime routing.
- `Witcher3StringEditor.Integrations.Storage` (`Witcher3StringEditor/Integrations/Storage`)
  - **Why not:** Contains a parallel translation memory model surface that overlaps `Common.TranslationMemory`, which
    already powers Services/Data TM stubs.

## Decision notes
- The **Common** project is the authoritative location for contracts and DTOs so Services, Data, UI, and Integrations
  can agree on a single schema without runtime wiring changes.
- **Services** and **Data** should continue to host runtime implementations and storage stubs, but they should only
  consume Common models rather than redefine them.
- **Rationale to prevent re-duplication:** All new profile and terminology work must reference the Common namespaces
  above. If additional data is required, extend the Common models and update services to consume them rather than
  creating parallel DTOs in `Integrations/*`. This keeps serialization, settings, and pipeline context aligned to a
  single schema and prevents conflicting defaults or mismatched fields.
