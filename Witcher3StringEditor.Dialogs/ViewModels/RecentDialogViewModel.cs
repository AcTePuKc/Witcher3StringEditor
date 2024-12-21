using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.MvvmDialogs;
using System.Collections.ObjectModel;
using Witcher3StringEditor.Core;
using Witcher3StringEditor.Core.Interfaces;
using Witcher3StringEditor.Dialogs.Models;

namespace Witcher3StringEditor.Dialogs.ViewModels
{
    public partial class RecentDialogViewModel : ObservableObject, IModalDialogViewModel, ICloseable
    {
        public bool? DialogResult => true;

        public event EventHandler? RequestClose;

        public ObservableCollection<IRecentItem> RecentItems { get; } = [];

        public RecentDialogViewModel()
        {
            var items = RecentManger.Instance.GetRecentItems();
            foreach (var item in items)
                RecentItems.Add(new RecentItem(item.FilePath, item.OpenedTime, item.IsPin));
        }

        [RelayCommand]
        public void ReOpen(IRecentItem item)
        {
            item.OpenedTime = DateTime.Now;
        }

        [RelayCommand]
        public void Closing()
        {
            RecentManger.Instance.Update(RecentItems);
        }
    }
}