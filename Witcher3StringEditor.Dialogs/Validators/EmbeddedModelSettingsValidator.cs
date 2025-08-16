using FluentValidation;
using Witcher3StringEditor.Interfaces;

namespace Witcher3StringEditor.Dialogs.Validators;

public class EmbeddedModelSettingsValidator : AbstractValidator<IEmbeddedModelSettings>
{
    public EmbeddedModelSettingsValidator()
    {
        RuleFor(x => x.EndPoint).Must(x => Uri.TryCreate(x, UriKind.Absolute, out _));
        RuleFor(x => x.ModelId).NotEmpty();
        RuleFor(x => x.ApiKey).NotEmpty();
        RuleFor(x => x.Dimensions).GreaterThan(0);
    }
}