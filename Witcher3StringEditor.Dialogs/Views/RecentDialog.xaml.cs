using CommunityToolkit.Mvvm.Messaging;
using System.Windows;
using Witcher3StringEditor.Dialogs.Recipients;

namespace Witcher3StringEditor.Dialogs.Views
{
    /// <summary>
    /// RecentDialog.xaml 的交互逻辑
    /// </summary>
    public partial class RecentDialog : Window
    {
        private readonly RecentFileIsPinRecipient recipient;

        public RecentDialog()
        {
            InitializeComponent();
            recipient = new RecentFileIsPinRecipient();
            WeakReferenceMessenger.Default.Register<RecentFileIsPinRecipient, RecentFileIsPinMessage>(recipient, (r, m) =>
            {
                sfDataGrid.View.Refresh();
            });
        }
    }
}
