using Eudora.Net.Data;
using System.Windows;
using System.Windows.Controls;

namespace Eudora.Net.GUI
{
    /// <summary>
    /// Interaction logic for uc_AddressBook_Personal.xaml
    /// </summary>
    public partial class uc_AddressBook_Personal : UserControl
    {
        public uc_AddressBook_Personal()
        {
            InitializeComponent();
        }

        private void btn_SwitchNames_Click(object sender, RoutedEventArgs e)
        {
            Contact? contact = DataContext as Contact;
            if(contact == null)
            {
                return;
            }

            string temp = contact.FirstName;
            contact.FirstName = contact.LastName;
            contact.LastName = temp;
        }
    }
}
