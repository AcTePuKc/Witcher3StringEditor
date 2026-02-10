# SettingsDialog MinWidth Binding Audit

Date: 2026-02-10
Status: Complete

## Scope
- `Witcher3StringEditor.Dialogs/Views/SettingsDialog.xaml`
- Dialog XAML files under `Witcher3StringEditor.Dialogs/Views`

## Search commands
- `rg -n 'MinWidth=' Witcher3StringEditor.Dialogs/Views/SettingsDialog.xaml Witcher3StringEditor.Dialogs/Views`
- `rg -n 'Setter Property="MinWidth"|MinWidth="\{Binding' -g '*.xaml'`
- `rg -n 'MinWidth' -g '*.xaml'`

## Findings
- `SettingsDialog.xaml` currently contains **no** `MinWidth` property assignments.
- No dialog XAML file contains `Setter Property="MinWidth"`.
- No dialog XAML file contains a `MinWidth` **binding** (`MinWidth="{Binding ...}"`).
- Existing `MinWidth` usages in dialog XAML are fixed numeric literals (for example `MinWidth="100"`, `MinWidth="70"`, or `MinWidth="0"`), so they cannot evaluate to `DependencyProperty.UnsetValue`.

## Stability conclusion
No code change was required for `MinWidth` binding fallback (`FallbackValue` / `TargetNullValue`) because there are no `MinWidth` bindings in the inspected Settings dialog surfaces.

## Follow-up guidance
If a future change introduces `MinWidth="{Binding ...}"` or a style setter with a binding value, use:
- `FallbackValue=0`
- `TargetNullValue=0`

and ensure any converter in that binding path returns `0d` instead of `DependencyProperty.UnsetValue`.
