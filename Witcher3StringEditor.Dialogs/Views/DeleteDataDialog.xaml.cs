namespace Witcher3StringEditor.Dialogs.Views;

/// <summary>
///     Interaction logic for DeleteDataDialog.xaml
///     This dialog allows users to confirm deletion of selected W3 string items
///     Displays the items to be deleted in a data grid for review before deletion
/// </summary>
public partial class DeleteDataDialog
{
    /// <summary>
    ///     Initializes a new instance of the DeleteDataDialog class
    ///     Calls InitializeComponent to set up the UI components defined in the XAML file
    /// </summary>
    public DeleteDataDialog()
    {
        InitializeComponent();
    }

    /// <summary>
    ///     Handles the closed event of the delete data dialog
    ///     Disposes of resources to prevent memory leaks when the dialog is closed
    /// </summary>
    /// <param name="sender">The object that triggered the event</param>
    /// <param name="e">The event arguments</param>
    private void DeleteDataDialog_OnClosed(object? sender, EventArgs e)
    {
        SfDataGrid.SearchHelper.Dispose();
        SfDataGrid.Dispose();
    }
}