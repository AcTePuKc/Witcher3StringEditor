using System.ComponentModel;
using Microsoft.Extensions.DependencyModel;
using Syncfusion.Data;

namespace Witcher3StringEditor.Dialogs.SortComparers;

/// <summary>
///     A custom comparer for sorting RuntimeLibrary objects based on their version strings
/// </summary>
public class VersionStringSortCompare : IComparer<RuntimeLibrary>, ISortDirection
{
    /// <summary>
    ///     Compares two RuntimeLibrary objects based on their version strings
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public int Compare(RuntimeLibrary? x, RuntimeLibrary? y)
    {
        // Extract version strings from RuntimeLibrary objects
        var versionStringX = x!.Version;
        var versionStringY = y!.Version;

        try
        {
            // Parse version strings into Version objects
            var versionX = new Version(versionStringX);
            var versionY = new Version(versionStringY);

            var comparisonResult = versionX.CompareTo(versionY); // Compare versions
            return SortDirection == ListSortDirection.Descending
                ? -comparisonResult
                : comparisonResult; // Return comparison result
        }
        catch (Exception)
        {
            return string.Compare(versionStringX, versionStringY,
                StringComparison.Ordinal); // Compare version strings if parsing fails
        }
    }

    /// <summary>
    ///     Gets or sets the sort direction for the comparison
    /// </summary>
    public ListSortDirection SortDirection { get; set; }
}