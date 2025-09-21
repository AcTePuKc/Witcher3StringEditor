namespace Witcher3StringEditor.Dialogs.Views;

/// <summary>
///     Interaction logic for DeleteDataDialog.xaml
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