using CommunityToolkit.Mvvm.DependencyInjection;
using iNKORE.UI.WPF.Modern;
using iNKORE.UI.WPF.Modern.Controls;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Helpers;
using System.Windows;
using Windows.UI.ViewManagement;
using Witcher3StringEditor.ViewModels;

namespace Witcher3StringEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainView : Window
    {
        //To get the calculated height from GetAutoRowHeight method.
        private double autoHeight = double.NaN;

        private readonly GridRowSizingOptions gridRowResizingOptions
            = new() { ExcludeColumns = ["StrID", "KeyHex", "KeyName"] };

        public MainView()
        {
            InitializeComponent();
            DataGrid.SearchHelper.AllowFiltering = true;
            DataContext = Ioc.Default.GetService<MainViewModel>();
            ThemeManager.Current.ApplicationTheme = IsDarkMode() == true ? ApplicationTheme.Dark : ApplicationTheme.Light;
        }

        private static bool IsDarkMode()
        {
            var settings = new UISettings();
            var foreground = settings.GetColorValue(UIColorType.Foreground);
            return IsColorLight(foreground);
        }

        private static bool IsColorLight(Windows.UI.Color color)
        {
            return (((5 * color.G) + (2 * color.R) + color.B) > (8 * 128));
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
            if (string.IsNullOrEmpty(sender.Text))
            {
                DataGrid.SearchHelper.ClearSearch();
            }
        }

        private void DataGrid_QueryRowHeight(object sender, QueryRowHeightEventArgs e)
        {
            if (DataGrid.GridColumnSizer.GetAutoRowHeight(e.RowIndex, gridRowResizingOptions, out autoHeight))
            {
                if (autoHeight > 30)
                {
                    e.Height = autoHeight;
                    e.Handled = true;
                }
            }
        }

        private void DataGrid_CurrentCellEndEdit(object sender, CurrentCellEndEditEventArgs e)
        {
            DataGrid.InvalidateRowHeight(e.RowColumnIndex.RowIndex);
            DataGrid.GetVisualContainer().InvalidateMeasureInfo();
        }
    }
}