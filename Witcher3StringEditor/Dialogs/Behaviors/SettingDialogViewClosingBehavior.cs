using Microsoft.Xaml.Behaviors;
using System.IO;
using System.Windows;
using Witcher3StringEditor.Core;
using Witcher3StringEditor.Dialogs.Views;
using Witcher3StringEditor.Locales;
using Witcher3StringEditor.Dialogs.Models;

namespace Witcher3StringEditor.Dialogs.Behaviors;

internal class SettingDialogViewClosingBehavior : Behavior<SettingsDialog>
{
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
        if (AssociatedObject.DataContext is not Settings settings) return;
        if (Path.GetFileName(settings.GameExePath) != "witcher3.exe"
            || Path.GetFileName(settings.W3StringsPath) != "w3strings.exe")
        {
            e.Cancel = true;

            if (MessageBox.Show(Strings.PleaseCheckSettings, Strings.Warning, MessageBoxButton.YesNo,
                    MessageBoxImage.Warning) == MessageBoxResult.Yes) Environment.Exit(0);
        }
        else
        {
            SettingsManager.Save(settings);
        }
    }
}