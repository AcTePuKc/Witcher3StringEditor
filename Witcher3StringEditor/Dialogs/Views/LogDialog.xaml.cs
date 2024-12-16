using Syncfusion.UI.Xaml.Grid;

namespace Witcher3StringEditor.Dialogs.Views;

/// <summary>
///     LogDialog.xaml 的交互逻辑
/// </summary>
public partial class LogDialog
{
    //To get the calculated height from GetAutoRowHeight method.
    private double autoHeight = double.NaN;

    public LogDialog()
    {
        InitializeComponent();
    }

    private void DataGrid_QueryRowHeight(object sender, QueryRowHeightEventArgs e)
    {
        if (!DataGrid.GridColumnSizer.GetAutoRowHeight(e.RowIndex, new GridRowSizingOptions(), out autoHeight))
            return;
        if (!(autoHeight > 30)) return;
        e.Height = autoHeight;
        e.Handled = true;
    }
}