using CommunityToolkit.Mvvm.DependencyInjection;
using iNKORE.UI.WPF.Modern.Controls;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Helpers;
using System.Windows;
using Witcher3StringEditor.ViewModels;

namespace Witcher3StringEditor.Views;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainView
{
    private readonly GridRowSizingOptions gridRowResizingOptions
        = new() { ExcludeColumns = ["StrID", "KeyHex", "KeyName"] };

    //To get the calculated height from GetAutoRowHeight method.
    private double autoHeight = double.NaN;

    public MainView()
    {
        InitializeComponent();
        DataGrid.SearchHelper.AllowFiltering = true;
        DataContext = Ioc.Default.GetService<MainViewModel>();
    }

    private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        DataGrid.GetVisualContainer().RowHeightManager.Reset();
        DataGrid.GetVisualContainer().InvalidateMeasureInfo();
    }

    private void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        DataGrid.SearchHelper.Search(args.QueryText);
    }

    private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (string.IsNullOrEmpty(sender.Text)) DataGrid.SearchHelper.ClearSearch();
    }

    private void DataGrid_QueryRowHeight(object sender, QueryRowHeightEventArgs e)
    {
        if (!DataGrid.GridColumnSizer.GetAutoRowHeight(e.RowIndex, gridRowResizingOptions, out autoHeight)) return;
        if (!(autoHeight > 30)) return;
        e.Height = autoHeight;
        e.Handled = true;
    }

    private void DataGrid_CurrentCellEndEdit(object sender, CurrentCellEndEditEventArgs e)
    {
        DataGrid.InvalidateRowHeight(e.RowColumnIndex.RowIndex);
        DataGrid.GetVisualContainer().InvalidateMeasureInfo();
    }
}