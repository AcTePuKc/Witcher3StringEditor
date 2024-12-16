using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Witcher3StringEditor.Core.Interfaces;

namespace Witcher3StringEditor.Models
{
    public partial class W3ItemModel : ObservableObject, ICloneable
    {
        [ObservableProperty]
        [Display(Name = "ID")]
        private string strId;

        [ObservableProperty]
        [Display(Name = "Key(Hex)")]
        private string keyHex;

        [ObservableProperty]
        [Display(Name = "Key(String)")]
        private string keyName;

        [ObservableProperty]
        private string text;

        public W3ItemModel(IW3Item w3Item)
        {
            StrId = w3Item.StrID;
            KeyHex = w3Item.KeyHex;
            KeyName = w3Item.KeyName;
            Text = w3Item.Text;
        }

        public W3ItemModel()
        {
            StrId = string.Empty;
            KeyHex = string.Empty;
            KeyName = string.Empty;
            Text = string.Empty;
        }

        public Guid Id { get; } = Guid.NewGuid();

        public object Clone() => MemberwiseClone();
    }
}