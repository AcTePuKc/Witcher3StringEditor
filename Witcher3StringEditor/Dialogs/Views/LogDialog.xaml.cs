using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.ScrollAxis;
using System.Windows;

namespace Witcher3StringEditor.Dialogs.Views
{
    /// <summary>
    /// LogDialog.xaml 的交互逻辑
    /// </summary>
    public partial class LogDialog : Window
    {
        //To get the calculated height from GetAutoRowHeight method.
        private double autoHeight = double.NaN;

        public LogDialog()
        {
            InitializeComponent();
        }

        private void DataGrid_QueryRowHeight(object sender, QueryRowHeightEventArgs e)
        {
            if (DataGrid.GridColumnSizer.GetAutoRowHeight(e.RowIndex, new GridRowSizingOptions(), out autoHeight))
            {
                if (autoHeight > 30)
                {
                    e.Height = autoHeight;
                    e.Handled = true;
                }
            }
        }

        private void DataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            DataGridScrollToEnd();
            DataGrid.View.CollectionChanged += (s, e) => DataGridScrollToEnd();
        }

        private void DataGridScrollToEnd()
        {
            var lastItem = DataGrid.View.Records.LastOrDefault();
            if (lastItem != null)
            {
                var rowIndex = DataGrid.ResolveToRowIndex(lastItem);
                DataGrid.ScrollInView(new RowColumnIndex(rowIndex, 0));
            }
        }
    }
}