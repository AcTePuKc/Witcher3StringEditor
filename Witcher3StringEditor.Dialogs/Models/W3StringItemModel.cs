using CommunityToolkit.Mvvm.ComponentModel;
using Witcher3StringEditor.Common.Abstractions;

namespace Witcher3StringEditor.Dialogs.Models;

public partial class W3StringItemModel : ObservableObject, ITrackableW3StringItem
{
    [ObservableProperty] private string keyHex = string.Empty;

    [ObservableProperty] private string keyName = string.Empty;

    [ObservableProperty] private string oldText = string.Empty;

    [ObservableProperty] private string strId = string.Empty;

    [ObservableProperty] private string text = string.Empty;

    public W3StringItemModel(IW3StringItem iw3StringItem)
    {
        StrId = iw3StringItem.StrId;
        KeyHex = iw3StringItem.KeyHex;
        KeyName = iw3StringItem.KeyName;
        OldText = iw3StringItem.OldText;
        Text = iw3StringItem.Text;
    }

    public W3StringItemModel()
    {
    }

    public Guid TrackingId { get; } = Guid.NewGuid();

    public object Clone()
    {
        return MemberwiseClone();
    }

    // ReSharper disable once UnusedParameterInPartialMethod
    partial void OnTextChanging(string value)
    {
        if (string.IsNullOrWhiteSpace(OldText)) OldText = Text;
    }
}