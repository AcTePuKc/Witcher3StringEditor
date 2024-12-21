using CommunityToolkit.Mvvm.ComponentModel;
using HanumanInstitute.MvvmDialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Witcher3StringEditor.Dialogs.ViewModels
{
    public partial class RecentDialogViewModel : ObservableObject, IModalDialogViewModel, ICloseable
    {
        public bool? DialogResult => true;

        public event EventHandler? RequestClose;
    }
}