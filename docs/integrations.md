# Integrations Plan (Translation Memory, Ollama, Terminology, Profiles)

## Architecture overview
- **Settings/UI**
  - Settings are surfaced in `SettingsDialog` and bound to `AppSettings` for persistence.
  - Provider selection, model selection, terminology/style flags, and profile selection should remain inert by default until services are wired.  
- **Provider pipeline**
  - Providers implement `ITranslationProvider` and are resolved by `ITranslationProviderRegistry`.
  - Model discovery flows through `ITranslationModelCatalog` and the provider `ListModelsAsync` call.
  - `TranslationRouter` handles routing and should remain the orchestration point for provider selection.
- **Translation memory (TM)**
  - TM settings are sourced from `ITranslationMemorySettingsProvider`.
  - Persistence uses local storage (SQLite or JSON) in `Witcher3StringEditor.Data`.
  - Database initialization should be a lightweight boot step before the TM store is used.
- **Terminology & style**
  - Packs are represented by `TerminologyPack` and loaded via `ITerminologyLoader`/`IStyleGuideLoader`.
  - Validation and prompt injection should remain stubbed until prompt formatting is specified.
- **Translation profiles**
  - Profiles are stored locally using a profile store.
  - The profile catalog provides a summary list for UI selection, while the resolver constructs a concrete profile for pipeline use.

## Planned work as GitHub issues

### Issue 1: Inventory pass for integration entrypoints
**Description**  
Confirm where settings persistence, translation routing, and UI binding occur before wiring new integrations. Document entrypoints and any lifecycle constraints.

**Acceptance Criteria**
- A markdown report lists entrypoints for settings persistence, translation routing, and translation helper usage.
- Risks and constraints are called out (e.g., startup ordering, service lifetimes).
- The report includes file paths and class names for each entrypoint.

**Files to touch**
- `docs/integrations.md`
- `docs/reports/integration-inventory.md` (new)

**QA checklist**
- Open the report and verify each entry includes a file path and class name.
- Confirm the report includes a section for settings persistence and translation routing.

---

### Issue 2: Wire local translation memory storage (SQLite) behind a feature toggle
**Description**  
Use the existing SQLite store as the default TM persistence when TM is enabled, but keep it inert by default. Make the database initialization explicit and controlled.

**Acceptance Criteria**
- TM services use SQLite storage only when TM is enabled in settings.
- Database initialization runs before the TM store is first accessed.
- TM remains disabled by default with no behavior change for users who do not opt in.

**Files to touch**
- `Witcher3StringEditor.Data/TranslationMemory/SqliteTranslationMemoryStore.cs`
- `Witcher3StringEditor.Data/TranslationMemory/SqliteTranslationMemoryDatabaseInitializer.cs`
- `Witcher3StringEditor/App.xaml.cs`
- `Witcher3StringEditor/Services/TranslationMemorySettingsProvider.cs`

**QA checklist**
- Build the solution.
- Toggle the TM setting and confirm no errors are thrown on startup.
- Verify that disabling TM keeps behavior unchanged.

---

### Issue 3: Ollama provider wiring + model selection scaffolding
**Description**  
Expose Ollama as a selectable provider with model discovery support. Ensure model list refresh uses the provider model catalog but does not introduce a new workflow.

**Acceptance Criteria**
- Ollama provider is registered and visible in provider selection.
- Model selection uses `ListModelsAsync` and does not block the UI.
- Provider configuration stays in local settings (BaseUrl, model, timeout).

**Files to touch**
- `Witcher3StringEditor.Integrations.Ollama/OllamaTranslationProvider.cs`
- `Witcher3StringEditor.Integrations.Ollama/OllamaSettings.cs`
- `Witcher3StringEditor.Dialogs/ViewModels/SettingsDialogViewModel.cs`
- `Witcher3StringEditor/Models/AppSettings.cs`
- `Witcher3StringEditor/App.xaml.cs`

**QA checklist**
- Build the solution.
- Open settings, select Ollama, and refresh models (no crash).
- Validate that the selected model persists between app restarts.

---

### Issue 4: Terminology + style guide loading pipeline
**Description**  
Keep terminology/style loading local and lightweight. Add validation hooks and retain existing UI with no workflow change.

**Acceptance Criteria**
- Terminology/style files load via the existing loader and surface a status message.
- Validation remains a no-op by default but the hooks are in place.
- No UI/UX changes beyond existing status text and file selection.

**Files to touch**
- `Witcher3StringEditor/Services/TerminologyLoader.cs`
- `Witcher3StringEditor/Services/NoopTerminologyValidationService.cs`
- `Witcher3StringEditor.Dialogs/ViewModels/SettingsDialogViewModel.cs`

**QA checklist**
- Build the solution.
- Provide a sample terminology file and confirm it loads without crashing.
- Confirm disabling terminology leaves translation behavior unchanged.

---

### Issue 5: Translation profiles storage + resolver wiring
**Description**  
Use local profile storage to list and resolve translation profiles. Keep profile selection optional and inert unless chosen.

**Acceptance Criteria**
- Profile list is populated from the local store.
- Selected profile resolves to a concrete `TranslationProfile` instance when requested.
- No profile selected results in the default app settings behavior.

**Files to touch**
- `Witcher3StringEditor.Data/Profiles/JsonTranslationProfileStore.cs`
- `Witcher3StringEditor.Data/Profiles/TranslationProfileCatalog.cs`
- `Witcher3StringEditor/Services/NoopTranslationProfileResolver.cs`
- `Witcher3StringEditor.Dialogs/ViewModels/SettingsDialogViewModel.cs`
- `Witcher3StringEditor/App.xaml.cs`

**QA checklist**
- Build the solution.
- Open settings and confirm profiles are listed if a profile file exists.
- Leave profile empty and verify translation flow continues unchanged.

