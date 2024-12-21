using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.MvvmDialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Witcher3StringEditor.Core.Interfaces;

namespace Witcher3StringEditor.Dialogs.ViewModels
{
    public partial class RecentDialogViewModel : ObservableObject, IModalDialogViewModel, ICloseable
    {
        public bool? DialogResult => true;

        public event EventHandler? RequestClose;

        public ObservableCollection<IRecentItem> RecentItems { get; } = [];

        [RelayCommand]
        public void ReOpen(object item)
        {

        }
    }
}