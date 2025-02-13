using FluentValidation;
using Witcher3StringEditor.Interfaces;

namespace Witcher3StringEditor.Dialogs.Validators;

public class ModelSettingsValidator : AbstractValidator<IModelSettings>
{
    private static readonly Lazy<ModelSettingsValidator> LazyInstance
    = new(static () => new ModelSettingsValidator());

    public static ModelSettingsValidator Instance => LazyInstance.Value;

    private ModelSettingsValidator()
    {
        RuleFor(x => x.EndPoint).Must(x => Uri.TryCreate(x, UriKind.Absolute, out _));
        RuleFor(x => x.ModelId).NotEmpty();
        RuleFor(x=>x.ApiKey).NotEmpty();
        RuleFor(x=>x.Prompts).NotEmpty();
    }
}