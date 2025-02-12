using CommunityToolkit.Mvvm.Messaging;
using System.Windows;
using Witcher3StringEditor.Dialogs.Locales;
using Witcher3StringEditor.Dialogs.Recipients;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;

namespace Witcher3StringEditor.Dialogs.Views;

/// <summary>
/// TranslateDiaglogView.xaml 的交互逻辑
/// </summary>
public partial class TranslateDiaglog
{
    private readonly SimpleStringRecipient translateRecipient = new();
    private readonly TranslatedTextNoSavedRecipient noSavedRecipient = new();
    private readonly TranslatorTranslatingRecipient translatingRecipient = new();

    public TranslateDiaglog()
    {
        InitializeComponent();
        WeakReferenceMessenger.Default.Register<SimpleStringRecipient, SimpleStringMessage, string>(translateRecipient, "TranslateCharactersNumberExceedLimit", static (r, m) =>
        {
            r.Receive(m);
            MessageBox.Show(Strings.TranslateCharactersNumberExceedLimitMessage,
                            Strings.TranslateCharactersNumberExceedLimitCaption,
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
        });
        WeakReferenceMessenger.Default.Register<SimpleStringRecipient, SimpleStringMessage, string>(translateRecipient, "TranslatedTextInvalid", static (r, m) =>
        {
            r.Receive(m);
            MessageBox.Show(Strings.TranslatedTextInvalidMessage,
                            Strings.TranslatedTextInvalidCaption,
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
        });
        WeakReferenceMessenger.Default.Register<SimpleStringRecipient, SimpleStringMessage, string>(translateRecipient, "TranslateError", static (r, m) =>
        {
            r.Receive(m);
            MessageBox.Show(m.Message,
                            Strings.TranslateErrorCaption,
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
        });
        WeakReferenceMessenger.Default.Register<TranslatedTextNoSavedRecipient, TranslatedTextNoSavedMessage>(noSavedRecipient, static (r, m) =>
        {
            r.Receive(m);
            m.Reply(MessageBox.Show(Strings.TranslatedTextNoSavedMessage,
                                    Strings.TranslatedTextNoSavedCaption,
                                    MessageBoxButton.YesNo,
                                    MessageBoxImage.Question) == MessageBoxResult.Yes);
        });
        WeakReferenceMessenger.Default.Register<TranslatorTranslatingRecipient, TranslatorTranslatingMessage>(translatingRecipient, static (r, m) =>
        {
            r.Receive(m);
            m.Reply(MessageBox.Show(Strings.TranslatorTranslatingMessage,
                                    Strings.TranslatorTranslatingCaption,
                                    MessageBoxButton.YesNo,
                                    MessageBoxImage.Question) == MessageBoxResult.Yes);
        });

    }

    private void Window_Closed(object sender, EventArgs e)
    {
        WeakReferenceMessenger.Default.UnregisterAll(translateRecipient);
        WeakReferenceMessenger.Default.UnregisterAll(noSavedRecipient);
        WeakReferenceMessenger.Default.UnregisterAll(translatingRecipient);
    }
}