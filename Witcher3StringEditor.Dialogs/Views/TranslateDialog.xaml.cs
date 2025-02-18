using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
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
public partial class TranslateDialog
{
    private readonly AsyncRequestRecipient<bool> requestRecipient = new();
    private readonly NotificationRecipient<string> notificationRecipient = new();

    public TranslateDialog()
    {
        InitializeComponent();
        WeakReferenceMessenger.Default.Register<NotificationRecipient<string>, NotificationMessage<string>, string>(notificationRecipient, "TranslateCharactersNumberExceedLimit", static (r, m) =>
        {
            r.Receive(m);
            MessageBox.Show(Strings.TranslateCharactersNumberExceedLimitMessage,
                            Strings.TranslateCharactersNumberExceedLimitCaption,
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
        });
        WeakReferenceMessenger.Default.Register<NotificationRecipient<string>, NotificationMessage<string>, string>(notificationRecipient, "TranslatedTextInvalid", static (r, m) =>
        {
            r.Receive(m);
            MessageBox.Show(Strings.TranslatedTextInvalidMessage,
                            Strings.TranslatedTextInvalidCaption,
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
        });
        WeakReferenceMessenger.Default.Register<NotificationRecipient<string>, NotificationMessage<string>, string>(notificationRecipient, "TranslateError", static (r, m) =>
        {
            r.Receive(m);
            MessageBox.Show(m.Message,
                            Strings.TranslateErrorCaption,
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
        });
        WeakReferenceMessenger.Default.Register<AsyncRequestRecipient<bool>, AsyncRequestMessage<bool>, string>(requestRecipient, "TranslatedTextNoSaved", static (r, m) =>
        {
            r.Receive(m);
            m.Reply(MessageBox.Show(Strings.TranslatedTextNoSavedMessage,
                                    Strings.TranslatedTextNoSavedCaption,
                                    MessageBoxButton.YesNo,
                                    MessageBoxImage.Question) == MessageBoxResult.Yes);
        });
        WeakReferenceMessenger.Default.Register<AsyncRequestRecipient<bool>, AsyncRequestMessage<bool>, string>(requestRecipient, "TranslatorIsTranslating", static (r, m) =>
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
        WeakReferenceMessenger.Default.UnregisterAll(requestRecipient);
        WeakReferenceMessenger.Default.UnregisterAll(notificationRecipient);
    }
}