using System.Windows;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Witcher3StringEditor.Locales;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

namespace Witcher3StringEditor.Dialogs.Views;

/// <summary>
///     SaveDialog.xaml 的交互逻辑
/// </summary>
public partial class SaveDialog : IRecipient<ValueChangedMessage<bool>>
{
    public SaveDialog()
    {
        InitializeComponent();
        RegisterMessageHandler();
    }

    public void Receive(ValueChangedMessage<bool> message)
    {
    }

    private void RegisterMessageHandler()
    {
        WeakReferenceMessenger.Default.Register<SaveDialog, ValueChangedMessage<bool>, string>(
            this, "Save", static (r, m) =>
            {
                _ = MessageBox.Show(m.Value ? Strings.SaveSuccess : Strings.SaveFailure,
                    Strings.SaveResult,
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            });
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        WeakReferenceMessenger.Default.UnregisterAll(this);
    }
}