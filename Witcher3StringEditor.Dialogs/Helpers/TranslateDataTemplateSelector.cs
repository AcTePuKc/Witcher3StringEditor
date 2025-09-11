using System.Windows;
using System.Windows.Controls;
using Witcher3StringEditor.Dialogs.ViewModels;

namespace Witcher3StringEditor.Dialogs.Helpers;

internal class TranslateDataTemplateSelector : DataTemplateSelector
{
    public DataTemplate? TranslateDataTemplate { get; set; }

    public DataTemplate? BatchTranslateDataTemplate { get; set; }

    public override DataTemplate? SelectTemplate(object? item, DependencyObject container)
    {
        return item switch
        {
            TranslateContentViewModel => TranslateDataTemplate,
            BatchTranslateContentViewModel => BatchTranslateDataTemplate,
            _ => null
        };
    }
}