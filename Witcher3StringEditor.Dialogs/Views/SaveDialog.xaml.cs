using CommunityToolkit.Mvvm.Messaging;
using Witcher3StringEditor.Dialogs.Locales;
using Witcher3StringEditor.Dialogs.Recipients;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;

namespace Witcher3StringEditor.Dialogs.Views;

/// <summary>
///     SaveDialog.xaml 的交互逻辑
/// </summary>
public partial class SaveDialog
{
    private readonly SaveResultRecipient saveResultRecipient = new();

    public SaveDialog()
    {
        InitializeComponent();

        WeakReferenceMessenger.Default.Register<SaveResultRecipient, SaveResultMessage>(saveResultRecipient, (r, m) =>
        {
            r.Receive(m);
            MessageBox.Show(m.IsSucess ? Strings.SaveSuccess : Strings.SaveFailure, Strings.SaveResult, MessageBoxButton.OK, MessageBoxImage.Information);
        });
    }

    private void Window_Closed(object sender, EventArgs e)
        => WeakReferenceMessenger.Default.UnregisterAll(saveResultRecipient);
}