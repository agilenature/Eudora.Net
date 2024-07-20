using Eudora.Net.Data;
using System.Windows;

namespace Eudora.Net.GUI
{
    /// <summary>
    /// Interaction logic for dlg_NewAddressBook.xaml
    /// </summary>
    public partial class dlg_NewAddressBook : Window
    {
        public string BookName { get; set; } = string.Empty;

        public dlg_NewAddressBook()
        {
            InitializeComponent();
            DataContext = this;
            tb_BookName.Focus();
        }

        private void Btn_OK_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void btn_OK_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(BookName) || String.IsNullOrWhiteSpace(BookName))
            {
                // TODO: Validation cues
                return;
            }
            var existingBook = AddressBookManager.Get(BookName);
            if(existingBook != null)
            {
                // TODO: Validation cues
                return;
            }
            
            DialogResult = true;
            Close();
        }

        private void btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void tb_BookName_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(e.Key == System.Windows.Input.Key.Enter)
            {
                e.Handled = true;
                btn_OK_Click(sender, e);
            }
        }
    }
}
