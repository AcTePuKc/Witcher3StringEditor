# Behavior Compatibility Check (Dialogs Behaviors + Main Project Reference)

Date: 2026-02-10

## Scope
Validate that the historical behavior types used by XAML remain resolvable from `Witcher3StringEditor.Dialogs.Behaviors`, and that the main WPF project keeps a project reference to `Witcher3StringEditor.Dialogs`.

## Result
No renames or removals were required. All required behavior classes already exist with public visibility in the expected namespace, and current XAML references are aligned.

## Verified Behavior Types
The four behavior classes requested for compatibility checks are present:

1. `SfDataGridSizeChangedBehavior`
2. `SfDataGridQueryRowHeightBehavior`
3. `SfDataGridCurrentCellEndEditBehavior`
4. `FrameworkElementDropFileBehavior`

Additional compatibility behavior found and unchanged:
- `SfDataGridRecordDeletingBehavior`

## XAML Resolution Confirmation
`MainWindow.xaml` still references the behavior namespace from the Dialogs assembly:

- `xmlns:b="clr-namespace:Witcher3StringEditor.Dialogs.Behaviors;assembly=Witcher3StringEditor.Dialogs"`

And still instantiates all four compatibility behaviors in `i:Interaction.Behaviors`.

## Project Reference Confirmation
`Witcher3StringEditor/Witcher3StringEditor.csproj` includes:

- `ProjectReference Include="..\Witcher3StringEditor.Dialogs\Witcher3StringEditor.Dialogs.csproj"`

No additional stubs were added because type resolution targets are already present and compile-safe.

## Follow-up (if future regression occurs)
If any of these behavior types are renamed/removed later, keep one of the following approaches:

1. Add compile-safe no-op compatibility stubs with the original names in `Witcher3StringEditor.Dialogs.Behaviors`, or
2. Update all affected XAML tags to the new behavior type names in a single compatibility PR.


## 2026-02-10 Namespace Mapping Recheck
Requested verification completed without requiring XAML mapping changes.

### 1) Locales mapping (`xmlns:l`)
Verified all dialog and main-window XAML files continue to use:

- `xmlns:l="clr-namespace:Witcher3StringEditor.Locales;assembly=Witcher3StringEditor.Locales"`

No mismatches found.

### 2) Behaviors mapping (`xmlns:b`)
Verified behavior namespace mappings are correct for each scope:

- Main app XAML uses cross-assembly mapping:
  - `xmlns:b="clr-namespace:Witcher3StringEditor.Dialogs.Behaviors;assembly=Witcher3StringEditor.Dialogs"`
- Dialog project XAML uses same-assembly mapping:
  - `xmlns:b="clr-namespace:Witcher3StringEditor.Dialogs.Behaviors"`

Both mappings resolve to the actual behavior namespace in `Witcher3StringEditor.Dialogs`.

### 3) Mapping updates
No assembly or CLR namespace changes were detected, so no mapping edits were applied.

### 4) Post-check for `x:Static` and behavior tags
A repository-wide XAML scan confirmed:

- `x:Static l:LangKeys.*` references remain consistent with the verified `xmlns:l` mapping.
- `b:*Behavior` tags remain consistent with the verified behavior mappings and known behavior types.
