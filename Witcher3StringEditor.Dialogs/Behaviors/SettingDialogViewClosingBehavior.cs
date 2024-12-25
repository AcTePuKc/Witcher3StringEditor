using Microsoft.Xaml.Behaviors;
using System.Windows;
using Witcher3StringEditor.Core;
using Witcher3StringEditor.Dialogs.Validators;
using Witcher3StringEditor.Dialogs.ViewModels;
using Witcher3StringEditor.Dialogs.Views;
using Witcher3StringEditor.Dialogs.Locales;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

namespace Witcher3StringEditor.Dialogs.Behaviors;

internal class SettingDialogViewClosingBehavior : Behavior<SettingsDialog>
{
    private readonly SettingsManager settingsManager = SettingsManager.Instance;

    protected override void OnAttached()
    {
        AssociatedObject.Closing += AssociatedObject_Closing;
    }

    protected override void OnDetaching()
    {
        AssociatedObject.Closing -= AssociatedObject_Closing;
    }

    private void AssociatedObject_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        if (AssociatedObject.DataContext is not SettingDialogViewModel viewModel) return;
        var settings = viewModel.AppSettings;
        var validations = AppSettingsValidator.Instance;
        var result = validations.Validate(settings);
        if (!result.IsValid)
        {
            e.Cancel = true;

            if (MessageBox.Show(Strings.PleaseCheckSettings,
                    Strings.Warning,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning) ==
                MessageBoxResult.Yes)
            {
                Environment.Exit(0);
            }
        }
        else
        {
            settingsManager.Save(settings);
        }
    }
}