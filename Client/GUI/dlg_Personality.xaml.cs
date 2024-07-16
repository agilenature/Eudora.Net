using Eudora.Net.Data;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace Eudora.Net.GUI
{
    /// <summary>
    /// Interaction logic for dlg_NewPersonality.xaml
    /// </summary>
    public partial class dlg_Personality : Window
    {
        public enum DialogMode
        {
            Create,
            Edit
        }
        private DialogMode _Mode = DialogMode.Create;
        public DialogMode Mode
        {
            get
            {
                return _Mode;
            }
            set
            {
                _Mode = value;
                if(_Mode == DialogMode.Create)
                {
                    Title = "Eudora.Net: Create New Personality";
                }
                else
                {
                    Title = "Eudora.Net: Edit Personality";
                }
            }
        }

        public dlg_Personality()
        {
            InitializeComponent();
            
            // Combo box for stationery selection
            cb_Stationery.ItemsSource = StationeryManager.Collection;
            cb_Stationery.DisplayMemberPath = "Name";
            cb_Stationery.SelectedIndex = 0;

            // Combo box for signature selection
            cb_Signature.ItemsSource = SignatureManager.Datastore.Data;
            cb_Signature.DisplayMemberPath = "Name";
            cb_Signature.SelectedIndex = 0;
        }

        /// <summary>
        /// Force SMTP Port TextBox to numeric-only input
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tb_SMTPPort_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void tb_POPPort_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void btn_OK_Click(object sender, RoutedEventArgs e)
        {
            // Validate, save, exit
            Personality? personality = DataContext as Personality;
            if (personality != null)
            {
                PersonalityManager.Add(personality);
            }
            e.Handled = true;
            Close();
        }

        private void btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
            e.Handled = true;
        }

        private void btn_Help_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
        }
    }
}
