using System.ComponentModel;
using Microsoft.Extensions.DependencyModel;
using Syncfusion.Data;

namespace Witcher3StringEditor.Dialogs.SortComparers;

public class VersionStringSortCompare:IComparer<RuntimeLibrary>,ISortDirection
{
    public int Compare(RuntimeLibrary? x, RuntimeLibrary? y)
    {
        var versionStringX = x!.Version;
        var versionStringY = y!.Version;
        
        try
        {
            var versionX = new Version(versionStringX);
            var versionY = new Version(versionStringY);

            var comparisonResult = versionX.CompareTo(versionY);

            return SortDirection == ListSortDirection.Descending ? -comparisonResult : comparisonResult;
        }
        catch (Exception)
        {
            return string.Compare(versionStringX, versionStringY, StringComparison.Ordinal);
        }
    }

    public ListSortDirection SortDirection { get; set; }
}