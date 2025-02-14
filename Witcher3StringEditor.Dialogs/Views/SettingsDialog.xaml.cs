using CommunityToolkit.Mvvm.Messaging;
using System.Windows;
using Witcher3StringEditor.Dialogs.Locales;
using Witcher3StringEditor.Dialogs.Recipients;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;

namespace Witcher3StringEditor.Dialogs.Views;

/// <summary>
///     SettingsDialog.xaml 的交互逻辑
/// </summary>
public partial class SettingsDialog
{
    private readonly WindowClosingRecipient closingRecipient = new();

    public SettingsDialog()
    {
        InitializeComponent();
        WeakReferenceMessenger.Default.Register<WindowClosingRecipient, WindowClosingMessage, string>(closingRecipient, "InitializationIncomplete", static (r, m) =>
        {
            r.Receive(m);
            m.Reply(MessageBox.Show(Strings.InitializationIncompleteMessage,
                                    Strings.InitializationIncompleteCaption,
                                    MessageBoxButton.YesNo,
                                    MessageBoxImage.Question) == MessageBoxResult.Yes);
        });
        WeakReferenceMessenger.Default.Register<WindowClosingRecipient, WindowClosingMessage, string>(closingRecipient, "IncompleteAiTranslationSettings", static (r, m) =>
        {
            r.Receive(m);
            m.Reply(MessageBox.Show(Strings.IncompleteAiTranslationSettingsMessage,
                                    Strings.IncompleteAiTranslationSettingsCaption,
                                    MessageBoxButton.YesNo,
                                    MessageBoxImage.Question) == MessageBoxResult.Yes);
        });
    }

    private void Window_Closed(object sender, EventArgs e)
        => WeakReferenceMessenger.Default.UnregisterAll(closingRecipient);
}