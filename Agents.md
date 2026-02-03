# Agents

This repository uses role-based agents to plan, scaffold, and coordinate future integrations.
Agents are responsible for **analysis, task decomposition, and stubs only** — not full feature implementation.

The goal is to keep changes incremental, reviewable, and safe.

---

## Global Rules (apply to all agents)

- Do NOT implement full features.
- Do NOT perform large refactors.
- Prefer interfaces, stubs, and TODO markers.
- Every PR must compile, or be explicitly gated behind a feature flag.
- One concern per PR.
- Write all documentation, task descriptions, and comments in English.
- When uncertain, create a task instead of guessing.

---

## Agent Roles

### 1. Repo Inventory Agent

**Purpose**
Identify where and how new integrations should attach to the existing codebase.

**Responsibilities**
- Locate Translation Helper entrypoints.
- Identify settings persistence mechanism.
- Identify UI dialogs/tabs relevant to translation or configuration.
- Map serializer/import/export extension points.

**Deliverables**
- Markdown report with:
  - file paths
  - class names
  - methods/events to hook into
  - risks or architectural constraints

**Non-goals**
- No code changes.
- No refactoring.

---

### 2. Provider Architecture Agent

**Purpose**
Create a clean abstraction for translation providers.

**Responsibilities**
- Define `ITranslationProvider` and related request/response models.
- Add a minimal provider registry or resolver.
- Insert TODO hooks where the Translation Helper should call the provider.

**Deliverables**
- Compilable interfaces and stubs.
- Clear TODO markers for follow-up agents.

**Non-goals**
- No real translation logic.
- No HTTP calls.

---

### 3. Ollama Integration Agent

**Purpose**
Prepare Ollama as a selectable translation provider.

**Responsibilities**
- Define Ollama settings model (BaseUrl, Model, parameters).
- Stub HTTP client and response models.
- Add `ListModelsAsync` skeleton (real endpoint may be TODO).

**Deliverables**
- Compilable provider stub.
- Configuration models.

**Non-goals**
- No UI wiring beyond placeholders.
- No prompt tuning or translation quality work.

---

### 4. Database / Translation Memory Agent

**Purpose**
Introduce a local persistence layer for translation memory and QA metadata.

**Responsibilities**
- Propose SQLite schema.
- Create database bootstrap logic.
- Define interfaces for TM and QA storage.

**Deliverables**
- Schema definition.
- Store interfaces + minimal stub implementation.

**Non-goals**
- No advanced fuzzy matching.
- No performance optimizations.

---

### 5. Terminology & Style Agent

**Purpose**
Enable loading and applying terminology and style rules.

**Responsibilities**
- Define terminology pack model.
- Implement loaders for `.tsv`, `.csv`, and `.md` (minimal parsing).
- Add stubs for prompt injection and post-translation validation.

**Deliverables**
- Loaders and models.
- Sample files under `docs/samples/`.

**Non-goals**
- No enforcement logic beyond reporting.
- No UI-heavy features.

---

### 6. UI & Settings Agent

**Purpose**
Expose new integration options to the user.

**Responsibilities**
- Add minimal UI elements for:
  - provider selection
  - model selection
  - terminology pack loading
- Persist settings using existing mechanisms.

**Deliverables**
- UI stubs wired to settings.
- No functional promises.

**Non-goals**
- No redesign of existing UI.
- No workflow changes.

---

### 7. QA Agent

**Purpose**
Ensure safety, stability, and non-regression.

**Responsibilities**
- Define QA checklists per task.
- Validate build, startup, and basic flows.
- Ensure new features are feature-flagged or inert by default.

**Deliverables**
- QA checklist markdown.
- Manual smoke test instructions.

**Non-goals**
- No automated test suite expansion (unless explicitly tasked).

---

## Ownership & Coordination

- Each agent produces **small, reviewable PRs**.
- Agents communicate via markdown reports and TODO markers.
- Integration details live in `docs/integrations.md`.

---

## Success Criteria

- The app builds and runs at every stage.
- New integrations are visible but safe.
- Future agents can implement features without re-architecting the app.

