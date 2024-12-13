using Syncfusion.UI.Xaml.Grid;
using System.Windows;

namespace Witcher3StringEditor.Dialogs.Views
{
    /// <summary>
    /// HistoryDialog.xaml 的交互逻辑
    /// </summary>
    public partial class BackupDialog : Window
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
            if (DataGrid.GridColumnSizer.GetAutoRowHeight(e.RowIndex, gridRowResizingOptions, out autoHeight))
            {
                if (autoHeight > 24)
                {
                    e.Height = autoHeight;
                    e.Handled = true;
                }
            }
        }
    }
}