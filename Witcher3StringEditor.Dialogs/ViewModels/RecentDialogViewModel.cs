using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using HanumanInstitute.MvvmDialogs;
using System.Collections.ObjectModel;
using Witcher3StringEditor.Core;
using Witcher3StringEditor.Core.Interfaces;
using Witcher3StringEditor.Dialogs.Models;
using Witcher3StringEditor.Dialogs.Recipients;

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
        public static void Open(IRecentItem item)
        {
            item.OpenedTime = DateTime.Now;
            WeakReferenceMessenger.Default.Send(new RecentFileOpenedMessage(item.FilePath));
        }

        [RelayCommand]
        public void Closing()
        {
            RecentManger.Instance.Update(RecentItems);
        }
    }
}