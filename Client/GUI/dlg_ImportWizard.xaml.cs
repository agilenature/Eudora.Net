﻿using Eudora.Net.Core;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;

namespace Eudora.Net.GUI
{
    /// <summary>
    /// Interaction logic for dlg_ImportWizard.xaml
    /// </summary>
    public partial class dlg_ImportWizard : Window
    {
        private ImportWizard TheWiz = new();
        private string _StatusText = string.Empty;
        private bool _Active = false;
        
        public string StatusText
        {
            get => _StatusText;
            set => _StatusText = value;
        }

        private bool Active
        { 
            get => _Active;
            set => _Active = value;
        }

        public dlg_ImportWizard()
        {
            Owner = Application.Current.MainWindow;
            DataContext = this;
            InitializeComponent();            
        }

        private void btn_Start_Click(object sender, RoutedEventArgs e)
        {
            Active = true;
            RunImportProcess();
        }

        private void btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            Active = false;

            // TODO: Add code to cancel the import process
            Close();
        }

        private void EnableStart(bool enable)
        {
            btn_OK.IsEnabled = enable;
        }

        private void EnableCancel(bool enable)
        {
            btn_Cancel.IsEnabled = enable;
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

        private void RunImportProcess()
        {
            EnableStart(false);
            ClearStatusText();

            AddStatusText("Starting import process...");
            
            if (LocateData())
            {
                ImportMailboxes();
                ImportAddressBooks();
            }

            AddStatusText("Import process complete");
            Active = false;
            EnableStart(true);
        }

        private bool LocateData()
        {
            MarkStepActive(cb_FindData);
            if (TheWiz.LocateEudoraData())
            {
                AddStatusText("Eudora data found at " + TheWiz.EudoraDataPath);
                MarkStepCompleted(cb_FindData);
                return true;
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
                    return true;
                }
                else
                {
                    AddStatusText("Import process cancelled");
                    MarkStepFailed(cb_FindData);
                    Active = false;
                    return false;
                }
            }
        }

        private void ImportAccounts()
        {
            //AddStatusText("Importing accounts...");
            //MarkStepActive(cb_Accounts);
            
            //if (TheWiz.ImportAccounts())
            //{
            //    AddStatusText("Accounts imported successfully");
            //    MarkStepCompleted(cb_Accounts);
            //}
            //else
            //{
            //    AddStatusText("Account import failed");
            //    MarkStepFailed(cb_Accounts);
            //}            
        }

        private void ImportMailboxes()
        {
            AddStatusText("Importing mailboxes...");
            MarkStepActive(cb_Mailboxes);
            
            if(TheWiz.ImportMailboxes())
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

            if(TheWiz.ImportContacts())
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

        private void btn_NCA_Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
