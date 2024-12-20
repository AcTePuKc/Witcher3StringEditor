using Microsoft.Xaml.Behaviors;
using System.ComponentModel;
using System.Windows;
using Witcher3StringEditor.Locales;
using Witcher3StringEditor.ViewModels;
using Witcher3StringEditor.Views;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;

namespace Witcher3StringEditor.Behaviors;

internal class MainWindowClosingBehavior : Behavior<MainWindow>
{
    protected override void OnAttached()
    {
        AssociatedObject.Closing += AssociatedObject_Closing;
    }

    protected override void OnDetaching()
    {
        AssociatedObject.Closing -= AssociatedObject_Closing;
    }

    private void AssociatedObject_Closing(object? sender, CancelEventArgs e)
    {
        if (AssociatedObject.DataContext is not MainWindowViewModel viewModel) return;
        if (viewModel.W3Items.Any() && MessageBox.Show(Strings.ExitQuestionMessage,
                Strings.ExitQuestionCaption,
                MessageBoxButton.YesNo,
                MessageBoxImage.Question) == MessageBoxResult.No)
        {
            e.Cancel = true;
        }
    }
}