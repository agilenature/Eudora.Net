using Eudora.Net.Core;
using Eudora.Net.Data;


namespace Eudora.Net.GUI
{
    /// <summary>
    /// Interaction logic for uc_MailboxesTab.xaml
    /// </summary>
    public partial class uc_MailboxesTab : uc_TabBase
    {
        ///////////////////////////////////////////////////////////
        #region Fields
        /////////////////////////////

        private bool _IsDeleteEnabled = false;
        public bool IsDeleteEnabled
        {
            get => _IsDeleteEnabled;
            set
            {
                _IsDeleteEnabled = value;
                btn_Delete.IsEnabled = value;
            }
        }

        /////////////////////////////
        #endregion Fields
        ///////////////////////////////////////////////////////////

        public uc_MailboxesTab()
        {
            InitializeComponent();

            Listview.ItemsSource = PostOffice.Mailboxes;
            IsDeleteEnabled = false;
        }

        public void SetSelectedMailbox(string mailboxName)
        {
            foreach (Mailbox mailbox in Listview.Items)
            {
                if (mailbox.Name.Equals(mailboxName, StringComparison.CurrentCultureIgnoreCase))
                {
                    Listview.SelectedItem = mailbox;
                    break;
                }
            }
        }

        private bool IsSelectionDeletable()
        {
            bool result = false;
            Mailbox? mailbox = Listview.SelectedItem as Mailbox;
            if (mailbox != null)
            {
                string selectedBox = mailbox.Name.ToLower();
                if (!selectedBox.Equals("inbox", StringComparison.CurrentCultureIgnoreCase) &&
                    !selectedBox.Equals("drafts", StringComparison.CurrentCultureIgnoreCase) &&
                    !selectedBox.Equals("sent", StringComparison.CurrentCultureIgnoreCase) &&
                    !selectedBox.Equals("trash", StringComparison.CurrentCultureIgnoreCase))
                {
                    result = true;
                }
            }
            return result;
        }

        private void Listview_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            e.Handled = true;

            Mailbox? mailbox = Listview.SelectedItem as Mailbox;
            if (mailbox == null)
            {
                return;
            }

            MainWindow.Instance?.ShowMailbox(mailbox.Name);
        }

        private void Listview_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            e.Handled = true;

            Mailbox? mailbox = Listview.SelectedItem as Mailbox;
            if (mailbox == null)
            {
                return;
            }

            IsDeleteEnabled = IsSelectionDeletable();
            MainWindow.Instance?.ShowMailbox(mailbox.Name);
        }

        private void btn_New_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            e.Handled = true;
            dlg_NewMailbox dlg = new();
            dlg.Owner = MainWindow.Instance;
            dlg.ShowDialog();
        }

        private void btn_Delete_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            e.Handled = true;

            Mailbox? mailbox = Listview.SelectedItem as Mailbox;
            if (mailbox == null) return;
            if (!IsSelectionDeletable()) return; // extra safety

            string prompt = "Are you sure you wish to delete this mailbox? This cannot be undone.";
            var dlg = new dlg_Confirmation(prompt);
            if (dlg.ShowDialog() == false) return;

            PostOffice.RemoveMailbox(mailbox);
        }
    }
}
