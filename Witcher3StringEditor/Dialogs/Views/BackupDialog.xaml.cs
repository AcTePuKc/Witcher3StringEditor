using Syncfusion.UI.Xaml.Grid;

namespace Witcher3StringEditor.Dialogs.Views
{
    /// <summary>
    /// HistoryDialog.xaml 的交互逻辑
    /// </summary>
    public partial class BackupDialog
    {
        //To get the calculated height from GetAutoRowHeight method.
        private double autoHeight = double.NaN;

        private readonly GridRowSizingOptions gridRowResizingOptions = new();

        public BackupDialog()
        {
            InitializeComponent();
        }

        private void DataGrid_QueryRowHeight(object sender, QueryRowHeightEventArgs e)
        {
            if (!DataGrid.GridColumnSizer.GetAutoRowHeight(e.RowIndex, gridRowResizingOptions, out autoHeight)) return;
            if (!(autoHeight > 24)) return;
            e.Height = autoHeight;
            e.Handled = true;
        }
    }
}