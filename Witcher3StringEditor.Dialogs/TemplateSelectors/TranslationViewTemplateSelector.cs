using System.Windows;
using System.Windows.Controls;
using Witcher3StringEditor.Dialogs.ViewModels;

namespace Witcher3StringEditor.Dialogs.TemplateSelectors;

/// <summary>
///     A DataTemplateSelector that selects the appropriate data template based on the type of translation view model
///     Used to dynamically switch between single item translation and batch translation views
/// </summary>
internal class TranslationViewTemplateSelector : DataTemplateSelector
{
    /// <summary>
    ///     Gets or sets the data template for single item translation view
    /// </summary>
    public DataTemplate? TranslateDataTemplate { get; set; }

    /// <summary>
    ///     Gets or sets the data template for batch translation view
    /// </summary>
    public DataTemplate? BatchTranslateDataTemplate { get; set; }

    /// <summary>
    ///     Selects the appropriate data template based on the type of the item
    /// </summary>
    /// <param name="item">The data object for which to select the template</param>
    /// <param name="container">The data-bound object</param>
    /// <returns>The DataTemplate to use for the item, or null if no appropriate template is found</returns>
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