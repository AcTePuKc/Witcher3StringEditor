using CommunityToolkit.Mvvm.Messaging;
using System.Windows;
using Witcher3StringEditor.Dialogs.Locales;
using Witcher3StringEditor.Dialogs.Recipients;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;

namespace Witcher3StringEditor.Dialogs.Views
{
    /// <summary>
    /// BatchTranslateDialog.xaml 的交互逻辑
    /// </summary>
    public partial class BatchTranslateDialog : Window
    {
        private readonly WindowClosingRecipient closingRecipient = new();

        public BatchTranslateDialog()
        {
            InitializeComponent();

            WeakReferenceMessenger.Default.Register<WindowClosingRecipient, WindowClosingMessage, string>(closingRecipient, "BatchTranslateDialogClosing", (r, m) =>
            {
                r.Receive(m);
                m.Reply(MessageBox.Show(Strings.DialogClosingWhenTranslatingMessage, Strings.Warning, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No);
            });
        }

        private void Window_Closed(object sender, EventArgs e) 
            => WeakReferenceMessenger.Default.UnregisterAll(closingRecipient);
    }
}