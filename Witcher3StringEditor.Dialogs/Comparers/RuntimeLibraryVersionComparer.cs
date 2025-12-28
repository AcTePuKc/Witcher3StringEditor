using System.ComponentModel;
using Microsoft.Extensions.DependencyModel;
using Syncfusion.Data;

namespace Witcher3StringEditor.Dialogs.Comparers;

/// <summary>
///     A custom comparer for sorting RuntimeLibrary objects based on their version strings
/// </summary>
public class RuntimeLibraryVersionComparer : IComparer<object>, ISortDirection
{
    /// <summary>
    ///     Compares two RuntimeLibrary objects based on their version strings
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns>
    ///     A value indicating whether the first object is less than, equal to, or greater than the second object
    /// </returns>
    public int Compare(object? x, object? y)
    {
        // Extract version strings from RuntimeLibrary objects
        var versionStringX = ((RuntimeLibrary)x!).Version;
        var versionStringY = ((RuntimeLibrary)y!).Version;

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