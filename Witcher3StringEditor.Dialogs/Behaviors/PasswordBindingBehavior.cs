using System.Windows;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;

namespace Witcher3StringEditor.Dialogs.Behaviors;

internal class PasswordBindingBehavior : Behavior<PasswordBox>
{
    public static readonly DependencyProperty PasswordProperty
        = DependencyProperty.Register(nameof(Password), typeof(string), typeof(PasswordBindingBehavior),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnPasswordChanged));

    public string Password
    {
        get => (string)GetValue(PasswordProperty);
        set => SetValue(PasswordProperty, value);
    }

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.PasswordChanged += OnPasswordBoxPasswordChanged;
    }

    protected override void OnDetaching()
    {
        AssociatedObject.PasswordChanged -= OnPasswordBoxPasswordChanged;
        base.OnDetaching();
    }

    private void OnPasswordBoxPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (Password != AssociatedObject.Password) Password = AssociatedObject.Password;
    }

    private static void OnPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PasswordBindingBehavior behavior
            && e.NewValue is string newPassword
            && behavior.AssociatedObject.Password != newPassword)
            behavior.AssociatedObject.Password = newPassword;
    }
}