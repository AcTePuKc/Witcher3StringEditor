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
