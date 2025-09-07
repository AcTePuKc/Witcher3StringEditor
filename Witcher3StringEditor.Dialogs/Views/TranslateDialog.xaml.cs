using System.Windows;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Witcher3StringEditor.Locales;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

namespace Witcher3StringEditor.Dialogs.Views;

/// <summary>
///     TranslateDialogView.xaml 的交互逻辑
/// </summary>
public partial class TranslateDialog : IRecipient<ValueChangedMessage<string>>, IRecipient<AsyncRequestMessage<bool>>
{
    public TranslateDialog()
    {
        InitializeComponent();
        RegisterMessageHandlers();
    }

    public void Receive(AsyncRequestMessage<bool> message)
    {
    }

    public void Receive(ValueChangedMessage<string> message)
    {
    }

    private void RegisterMessageHandlers()
    {
        RegisterNotificationMessageHandlers();
        RegisterAsyncRequestMessageHandlers();
    }

    private void RegisterAsyncRequestMessageHandlers()
    {
        WeakReferenceMessenger.Default.Register<TranslateDialog, AsyncRequestMessage<bool>, string>(
            this,
            "TranslatedTextNoSaved",
            (_, m) =>
            {
                m.Reply(MessageBox.Show(
                    Strings.TranslatedTextNoSavedMessage,
                    Strings.TranslatedTextNoSavedCaption,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question) == MessageBoxResult.Yes);
            });
        WeakReferenceMessenger.Default.Register<TranslateDialog, AsyncRequestMessage<bool>, string>(
            this,
            "TranslationDialogClosing",
            (_, m) =>
            {
                m.Reply(MessageBox.Show(
                    Strings.TranslatorTranslatingMessage,
                    Strings.TranslatorTranslatingCaption,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question) == MessageBoxResult.Yes);
            });
        WeakReferenceMessenger.Default.Register<TranslateDialog, AsyncRequestMessage<bool>, string>(
            this,
            "TranslationModeSwitch",
            (_, m) =>
            {
                m.Reply(MessageBox.Show(
                    Strings.TranslationModeSwitchMessage,
                    Strings.TranslationModeSwitchCaption,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question) == MessageBoxResult.Yes);
            });

        WeakReferenceMessenger.Default.Register<TranslateDialog, AsyncRequestMessage<bool>, string>(
            this,
            "TranslationNotEmpty",
            (_, m) =>
            {
                m.Reply(MessageBox.Show(
                    Strings.TranslationNotEmptyMessage,
                    Strings.TranslationNotEmptyCaption,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question) == MessageBoxResult.Yes);
            });
    }

    private void RegisterNotificationMessageHandlers()
    {
        WeakReferenceMessenger.Default.Register<TranslateDialog, ValueChangedMessage<string>, string>(
            this,
            "TranslatedTextInvalid",
            (r, m) =>
            {
                _ = MessageBox.Show(Strings.TranslatedTextInvalidMessage,
                    Strings.TranslatedTextInvalidCaption,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            });

        WeakReferenceMessenger.Default.Register<TranslateDialog, ValueChangedMessage<string>, string>(
            this,
            "TranslateError",
            (r, m) =>
            {
                _ = MessageBox.Show(m.Value,
                    Strings.TranslateErrorCaption,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            });
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        WeakReferenceMessenger.Default.UnregisterAll(this);
    }
}