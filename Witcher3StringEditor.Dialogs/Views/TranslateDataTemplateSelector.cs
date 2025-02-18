using System.Windows;
using System.Windows.Controls;
using Witcher3StringEditor.Dialogs.ViewModels;

namespace Witcher3StringEditor.Dialogs.Views;

public class TranslateDataTemplateSelector : DataTemplateSelector
{
    public DataTemplate? TranslateDataTemplate { get; set; }

    public DataTemplate? BatchTranslateDataTemplate { get; set; }

    public override DataTemplate? SelectTemplate(object item, DependencyObject container)
    {
        if (item is TranslateViewModel) return TranslateDataTemplate;
        if (item is BatchTranslateViewModel) return BatchTranslateDataTemplate;
        return null;
    }
}