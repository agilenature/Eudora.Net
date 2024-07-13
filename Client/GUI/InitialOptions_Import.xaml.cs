using Eudora.Net.Core;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;


namespace Eudora.Net.GUI
{
    /// <summary>
    /// Interaction logic for InitialOptions_Import.xaml
    /// </summary>
    public partial class InitialOptions_Import : Page
    {
        private ImportWizard TheWiz = new();
        private string _StatusText = string.Empty;
        private bool _Active = false;

        private string StatusText
        {
            get => _StatusText;
            set => _StatusText = value;
        }

        private bool Active
        {
            get => _Active;
            set => _Active = value;
        }

        private void MarkStepActive(CheckBox cb)
        {
            cb.FontWeight = FontWeights.Bold;
        }

        private void MarkStepCompleted(CheckBox cb)
        {
            cb.Foreground = System.Windows.Media.Brushes.Green;
            cb.IsChecked = true;
        }

        private void MarkStepFailed(CheckBox cb)
        {
            cb.Foreground = System.Windows.Media.Brushes.Red;
            cb.IsChecked = false;
        }

        private void ResetSteps()
        {
            cb_FindData.IsChecked = false;
            cb_FindData.FontWeight = FontWeights.Normal;
            cb_FindData.Foreground = System.Windows.SystemColors.ControlTextBrush;

            //cb_Accounts.IsChecked = false;
            //cb_Accounts.FontWeight = FontWeights.Normal;
            //cb_Accounts.Foreground = System.Windows.SystemColors.ControlTextBrush;

            cb_Mailboxes.IsChecked = false;
            cb_Mailboxes.FontWeight = FontWeights.Normal;
            cb_Mailboxes.Foreground = System.Windows.SystemColors.ControlTextBrush;

            cb_AddressBooks.IsChecked = false;
            cb_AddressBooks.FontWeight = FontWeights.Normal;
            cb_AddressBooks.Foreground = System.Windows.SystemColors.ControlTextBrush;
        }

        private void AddStatusText(string text)
        {
            StatusText += text + "\n";
            tb_Status.Text = StatusText;
        }

        private void ClearStatusText()
        {
            StatusText = string.Empty;
            tb_Status.Text = StatusText;
        }

        private void EnableStart(bool enable)
        {
            btn_Import.IsEnabled = enable;
        }

        private void RunImportProcess()
        {
            EnableStart(false);
            ClearStatusText();

            AddStatusText("Starting import process...");
            LocateData();
            //ImportAccounts();
            ImportMailboxes();
            ImportAddressBooks();

            AddStatusText("Import process complete");
            Active = false;
            EnableStart(true);
        }

        private void LocateData()
        {
            MarkStepActive(cb_FindData);
            if (TheWiz.LocateEudoraData())
            {
                AddStatusText("Eudora data found at " + TheWiz.EudoraDataPath);
                MarkStepCompleted(cb_FindData);
            }
            else
            {
                AddStatusText("Eudora data not found. Please specify the folder");
                OpenFolderDialog ofd = new();
                ofd.Multiselect = false;
                ofd.Title = "Select the folder containing Eudora data";
                ofd.ValidateNames = true;
                if (ofd.ShowDialog() == true)
                {
                    TheWiz.EudoraDataPath = ofd.FolderName;
                    MarkStepCompleted(cb_FindData);
                }
                else
                {
                    AddStatusText("Import process cancelled");
                    MarkStepFailed(cb_FindData);
                    Active = false;
                    return;
                }
            }
        }

        private void ImportMailboxes()
        {
            AddStatusText("Importing mailboxes...");
            MarkStepActive(cb_Mailboxes);

            if (TheWiz.ImportMailboxes())
            {
                AddStatusText("Mailboxes imported successfully");
                MarkStepCompleted(cb_Mailboxes);
            }
            else
            {
                AddStatusText("Mailbox import failed");
                MarkStepFailed(cb_Mailboxes);
            }

        }

        private void ImportAddressBooks()
        {
            AddStatusText("Importing address books...");
            MarkStepActive(cb_AddressBooks);

            if (TheWiz.ImportContacts())
            {
                AddStatusText("Address books imported successfully");
                MarkStepCompleted(cb_AddressBooks);
            }
            else
            {
                AddStatusText("Address book import failed");
                MarkStepFailed(cb_AddressBooks);
            }
        }

        public InitialOptions_Import()
        {
            InitializeComponent();
        }

        private void btn_Import_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            PostOffice.Instance.Startup();
            RunImportProcess();
        }
    }
}
