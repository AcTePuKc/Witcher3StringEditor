using System.Windows;
using System.Windows.Controls;
using Witcher3StringEditor.Dialogs.ViewModels;

namespace Witcher3StringEditor.Dialogs.TemplateSelectors;

internal class TranslationViewTemplateSelector : DataTemplateSelector
{
    public DataTemplate? TranslateDataTemplate { get; set; }

    public DataTemplate? BatchTranslateDataTemplate { get; set; }

    public override DataTemplate? SelectTemplate(object? item, DependencyObject container)
    {
        return item switch
        {
            SingleItemTranslationViewModel => TranslateDataTemplate,
            BatchItemsTranslationViewModel => BatchTranslateDataTemplate,
            _ => null
        };
    }
}