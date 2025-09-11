using System.ComponentModel;
using JetBrains.Annotations;

namespace Witcher3StringEditor.Common;

public enum W3Language
{
    [Description("ar")] Ar = 0,
    [Description("pt")] Br = 1,
    [Description("zh-Hans")] Cn = 2,

    [UsedImplicitly]
    [Description("cs")] Cz = 3,
    [Description("de")] [UsedImplicitly] De = 4,
    [Description("en")] [UsedImplicitly] En = 5,
    [Description("es")] [UsedImplicitly] Es = 6,
    [Description("es-MX")] Esmx = 7,
    [Description("fr")] [UsedImplicitly] Fr = 8,
    [Description("hu")] [UsedImplicitly] Hu = 9,
    [Description("it")] [UsedImplicitly] It = 10,

    [UsedImplicitly]
    [Description("jp")] Jp = 11,
    [Description("ko")] Kr = 12,
    [Description("pl")] [UsedImplicitly] Pl = 13,
    [Description("ru")] [UsedImplicitly] Ru = 14,

    [UsedImplicitly]
    [Description("zh-Hant")] Zh = 15,
    [Description("tr")] Tr = 16
}