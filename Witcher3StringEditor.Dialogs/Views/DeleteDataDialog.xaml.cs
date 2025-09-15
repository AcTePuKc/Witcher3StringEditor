namespace Witcher3StringEditor.Dialogs.Views;

/// <summary>
///     DeleteDataDialog.xaml 的交互逻辑
/// </summary>
public partial class DeleteDataDialog
{
    public DeleteDataDialog()
    {
        InitializeComponent();
    }

    private void DeleteDataDialog_OnClosed(object? sender, EventArgs e)
    {
        SfDataGrid.SearchHelper.Dispose();
        SfDataGrid.Dispose();
    }
}