# MainWindow XAML Root Cause Analysis

## Scope
This document records the primary root causes behind MainWindow XAML failures and defines preventive checks for future changes.

## Primary Root Causes

### 1) `LangKeys` resolution failure
- **Symptom**: XAML designer/build reports unresolved members when binding or referencing localized keys.
- **Root cause**: The `LangKeys` type was not consistently resolvable from the XAML compilation context (namespace mapping mismatch, assembly mapping mismatch, or non-public visibility).
- **Impact**: Localization references in MainWindow failed to compile or render correctly in the designer.

### 2) Behavior type resolution failure
- **Symptom**: XAML parser reports unknown behavior/action types (for example attached behaviors in `Interaction.Behaviors`).
- **Root cause**: Behavior classes or their namespace/assembly mapping were not correctly exposed to XAML (missing reference, incorrect `xmlns`, or incompatible behavior package/type mapping).
- **Impact**: Event-to-command or behavior wiring in MainWindow failed at compile-time and/or design-time.

## Prevention Checklist

Use this checklist before merging MainWindow XAML changes:

1. **Namespace visibility**
   - Confirm all XAML-referenced classes are `public`.
   - Confirm namespace declarations in XAML match C# namespaces exactly.

2. **Project references**
   - Confirm the UI project references the assembly containing `LangKeys` and any behavior types.
   - Confirm referenced projects build successfully in the current solution configuration.

3. **Assembly mapping validation**
   - Confirm each `xmlns` uses the correct CLR namespace and assembly mapping.
   - Confirm behavior libraries (e.g., XAML behaviors package) are present and compatible with the target framework.
   - Perform a clean rebuild to validate XAML compile-time resolution.

## Recommended Validation Steps
- Run a clean build after any XAML namespace or behavior wiring change.
- Open MainWindow in designer (if available) and verify no unresolved type/member warnings.
- Start the app and verify localized content and behavior-triggered interactions initialize without runtime XAML exceptions.
