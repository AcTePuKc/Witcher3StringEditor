using Syncfusion.UI.Xaml.Grid;

namespace Witcher3StringEditor.Dialogs.Views
{
    /// <summary>
    /// DeleteDataDialog.xaml 的交互逻辑
    /// </summary>
    public partial class DeleteDataDialog
    {
        //To get the calculated height from GetAutoRowHeight method.
        private double autoHeight = double.NaN;

        private readonly GridRowSizingOptions gridRowResizingOptions
            = new() { ExcludeColumns = ["StrID", "KeyHex", "KeyName"] };

        public DeleteDataDialog()
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