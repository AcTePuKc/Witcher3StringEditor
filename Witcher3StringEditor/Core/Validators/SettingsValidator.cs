using FluentValidation;
using System.IO;
using Witcher3StringEditor.Dialogs.Models;

namespace Witcher3StringEditor.Core.Validators
{
    public partial class SettingsValidator : AbstractValidator<Settings>
    {
        public SettingsValidator()
        {
            RuleFor(x => x.W3StringsPath).NotEmpty().Must(x => File.Exists(x) && Path.GetFileName(x) == "w3strings.exe");
            RuleFor(x => x.GameExePath).NotEmpty().Must(x => File.Exists(x) && Path.GetFileName(x) == "witcher3.exe");
        }
    }
}