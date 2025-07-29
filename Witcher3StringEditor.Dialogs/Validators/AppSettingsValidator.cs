using System.IO;
using FluentValidation;
using Witcher3StringEditor.Interfaces;

namespace Witcher3StringEditor.Dialogs.Validators;

public class AppSettingsValidator : AbstractValidator<IAppSettings>
{
    public AppSettingsValidator()
    {
        RuleFor(x => x.GameExePath).NotEmpty()
            .Must(x => File.Exists(x) && Path.GetFileName(x) == "witcher3.exe");
        RuleFor(x => x.W3StringsPath).NotEmpty()
            .Must(x => File.Exists(x) && Path.GetFileName(x) == "w3strings.exe");
    }
}