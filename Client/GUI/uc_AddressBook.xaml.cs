using Eudora.Net.Data;
using System.Windows;
using System.Windows.Controls;

namespace Eudora.Net.GUI
{
    /// <summary>
    /// Interaction logic for uc_AddressBook.xaml
    /// </summary>
    public partial class uc_AddressBook : UserControl
    {
        public uc_AddressBook() : base()
        {
            InitializeComponent();
            Loaded += Uc_AddressBook_Loaded;

            // Disable Contact UX until a contact is selected
            DetailsGrid.IsEnabled = false;

            lb_Books.ItemsSource = AddressBookManager.Collection;
        }

        private void Uc_AddressBook_Loaded(object sender, RoutedEventArgs e)
        {
            lb_Books.SelectedIndex = 0;
        }

        /// <summary>
        /// The full AddressBook UX consists of many UserControls.
        /// Upon changing the selected contact in the list, update
        /// DataContext for each tab.
        /// </summary>
        /// <param name="contact"></param>
        private void UpdateDataContext(Contact contact)
        {
            DataContext = contact;
            foreach(var item in tc_Tabs.Items)
            {
                UserControl? tab = item as UserControl;
                if(tab is not null)
                {
                    tab.DataContext = contact;
                }
            }
        }

        private void lb_Books_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(lb_Books.SelectedItem is AddressBook book)
            {
                lb_Contacts.ItemsSource = book.Contacts;
            }
        }

        private void lb_Contacts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(lb_Contacts.SelectedItem is Contact contact)
            {
                UpdateDataContext(contact);
                DetailsGrid.IsEnabled = true;
            }
            else
            {
                DetailsGrid.IsEnabled = false;
            }                
        }        

        /// <summary>
        /// Button: New Address Book
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_NewBook_Click(object sender, RoutedEventArgs e)
        {
            dlg_NewAddressBook dlg = new dlg_NewAddressBook();
            dlg.ShowDialog();
            if(dlg.DialogResult == true)
            {
                AddressBookManager.New(dlg.BookName);
            }
        }

        /// <summary>
        /// Button: Delete Address Book
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_DeleteBook_Click(object sender, RoutedEventArgs e)
        {
            if (lb_Books.SelectedItem is AddressBook book)
            {
                string prompt = "Are you certain you want to delete this Address Book?\r\n" +
                                "Doing so will also delete all contacts within that book.";

                dlg_Confirmation dlg = new dlg_Confirmation(prompt);
                var result = dlg.ShowDialog();
                if (result == true)
                {
                    AddressBookManager.Remove(book);
                }
            }
        }

        /// <summary>
        /// Button: New Contact
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_NewContact_Click(object sender, RoutedEventArgs e)
        {
            if (lb_Books.SelectedItem is AddressBook book)
            {
                book.NewContact();
            }
        }

        /// <summary>
        /// Button: Delete Contact
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_DeleteContact_Click(object sender, RoutedEventArgs e)
        {
            if (lb_Books.SelectedItem is AddressBook book)
            {
                if (lb_Contacts.SelectedItem is Contact contact)
                {
                    string prompt = "Are you certain you want to delete this Contact?";

                    dlg_Confirmation dlg = new dlg_Confirmation(prompt);
                    var result = dlg.ShowDialog();
                    if (result == true)
                    {
                        book.Delete(contact);
                    }
                }
            }
        }
    }
}
