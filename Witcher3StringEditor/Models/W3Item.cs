using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Witcher3StringEditor.Core.Interfaces;

namespace Witcher3StringEditor.Models;

public partial class W3Item : ObservableObject, IW3Item
{
    public Guid Id { get; }

    [ObservableProperty]
    [Display(Name = "Key(Hex)")]
    private string keyHex;

    [ObservableProperty]
    [Display(Name = "Key(String)")]
    private string keyName;

    [ObservableProperty]
    [Display(Name = "ID")]
    private string strId;

    [ObservableProperty] private string text;

    public W3Item(string strId, string keyHex, string keyName, string text)
    {
        Id = Guid.NewGuid();
        StrId = strId;
        KeyHex = keyHex;
        KeyName = keyName;
        Text = text;
    }

    public object Clone() => MemberwiseClone();
}