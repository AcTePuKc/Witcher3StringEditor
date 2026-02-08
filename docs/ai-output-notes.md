# AI Output Notes (Bulgarian + English Prompt Sampling)

## Scope
This note captures a minimal sampling plan for **Ollama** and **legacy** translators (GTranslate-backed) on Bulgarian and English prompts, along with observed outputs, polite-prefix patterns, and opt-in post-processing rules. It is intentionally light-weight and safe to run locally without changing app behavior.

## Test Prompts
Use a consistent, short set of prompts so outputs are comparable across providers and models.

**English prompts**
1. "Translate this to Bulgarian: The innkeeper said the rooms were full."
2. "Translate this to Bulgarian: The witcher sharpened his sword before the hunt."

**Bulgarian prompts**
1. "Преведи на английски: Ханджията каза, че стаите са заети."
2. "Преведи на английски: Вещерът наточи меча си преди лова."

## Environment Checks
### Ollama CLI
Command:
```
ollama --version
```
Result:
```
bash: command not found: ollama
```

### Legacy Translators (GTranslate-backed)
Legacy translators run inside the desktop app runtime and typically require network access and API availability. In this environment, the app runtime is not launched and no API keys are configured, so legacy translator outputs were not executed here.

## Output Capture
Because the Ollama CLI was unavailable and the app runtime was not launched, **no model outputs were captured** in this environment. The table below is provided as the expected capture format for future runs.

| Provider | Model | Prompt | Output | Notes |
| --- | --- | --- | --- | --- |
| Ollama | (missing CLI) | English #1 | _n/a_ | CLI unavailable. |
| Ollama | (missing CLI) | English #2 | _n/a_ | CLI unavailable. |
| Ollama | (missing CLI) | Bulgarian #1 | _n/a_ | CLI unavailable. |
| Ollama | (missing CLI) | Bulgarian #2 | _n/a_ | CLI unavailable. |
| Legacy | MicrosoftTranslator | English #1 | _n/a_ | App runtime not executed. |
| Legacy | GoogleTranslator | English #2 | _n/a_ | App runtime not executed. |
| Legacy | YandexTranslator | Bulgarian #1 | _n/a_ | App runtime not executed. |
| Legacy | MicrosoftTranslator | Bulgarian #2 | _n/a_ | App runtime not executed. |

## Observed Polite Prefixes
No polite-prefix patterns were observed because outputs were not captured. For future runs, pay special attention to leading boilerplate like the following (examples to watch for):

**English candidates**
- "Sure," "Certainly," "Of course," "Here is the translation:"

**Bulgarian candidates**
- "Разбира се," "С удоволствие," "Ето превода:" "Превод:"

## Proposed Opt-In Post-Processing Rules (String Stripping)
These rules are **opt-in** and should only run when explicitly enabled (e.g., via settings or profile flags). They should preserve the original output when disabled.

1. **Trim leading polite phrases**
   - Remove common English prefixes (case-insensitive):
     - `"sure,"`, `"certainly,"`, `"of course,"`, `"here is the translation:"`
   - Remove common Bulgarian prefixes (case-insensitive):
     - `"разбира се,"`, `"с удоволствие,"`, `"ето превода:"`, `"превод:"`

2. **Trim leading punctuation + whitespace after strip**
   - After removing a prefix, trim leading spaces, `:` and `-` if present.

3. **Keep line count stable when possible**
   - Only trim leading text on the first line; avoid altering newlines beyond the first line.

4. **Allow per-provider overrides**
   - Keep the rule list configurable so providers with different templates can add or remove prefixes.

### Risks
- **False positives**: Legitimate output may start with one of these phrases (e.g., a narrative sentence).
- **Language ambiguity**: Mixed-language outputs might be incorrectly stripped.
- **Quoted dialogue**: Removing a prefix might strip intended quoted text (e.g., a sentence starting with “Of course,”).
- **Provider drift**: Different models may change preambles, requiring ongoing maintenance of the prefix list.

## Follow-Up Actions
- Re-run tests on a machine with the Ollama CLI installed and at least one local model (e.g., `llama3`, `mistral`).
- Capture legacy translator outputs from inside the app runtime with network access available.
- Update this note with actual outputs and replace placeholder entries.
