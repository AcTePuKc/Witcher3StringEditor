using CommunityToolkit.Mvvm.Messaging;
using Witcher3StringEditor.Dialogs.Recipients;

namespace Witcher3StringEditor.Dialogs.Views;

/// <summary>
///     SaveDialog.xaml 的交互逻辑
/// </summary>
public partial class SaveDialog
{
    private readonly SaveResultRecipient recipient;

    public SaveDialog()
    {
        InitializeComponent();
        recipient = new SaveResultRecipient();
        WeakReferenceMessenger.Default.Register(recipient);
    }
}