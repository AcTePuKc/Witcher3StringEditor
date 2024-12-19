using CommunityToolkit.Mvvm.ComponentModel;
using HanumanInstitute.MvvmDialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTranslate;
using System.Collections.ObjectModel;

namespace Witcher3StringEditor.Dialogs.ViewModels
{
    internal class TranslateDiaglogViewModel : ObservableObject, IModalDialogViewModel, ICloseable
    {
        public bool? DialogResult => true;

        public event EventHandler? RequestClose;

        public ObservableCollection<Language> Languages { get; set; }

        public TranslateDiaglogViewModel()
        {
            Languages = new(Language.LanguageDictionary.Values.Where(x => x.SupportedServices.HasFlag(TranslationServices.Microsoft)));
        }
    }
}