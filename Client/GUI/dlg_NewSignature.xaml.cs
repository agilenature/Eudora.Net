using Eudora.Net.Core;
using Eudora.Net.Data;
using System.Windows;

namespace Eudora.Net.GUI
{
    /// <summary>
    /// Interaction logic for dlg_NewSignature.xaml
    /// </summary>
    public partial class dlg_NewSignature : Window
    {
        private string _SignatureName = string.Empty;
        public string SignatureName
        {
            get => _SignatureName;
            set => _SignatureName = value;
        }

        private string _ErrorText = string.Empty;
        public string ErrorText
        {
            get => _ErrorText;
            set => _ErrorText = value;
        }


        public dlg_NewSignature()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void ShowErrorPanel(string error)
        {
            ErrorText = error;
            ErrorPanel.Visibility = Visibility.Visible;
        }

        private void btn_OK_Click(object sender, RoutedEventArgs e)
        {
            ErrorPanel.Visibility = Visibility.Hidden;

            if (String.IsNullOrEmpty(SignatureName) || String.IsNullOrWhiteSpace(SignatureName))
            {
                ShowErrorPanel("Name cannot be empty");
                return;
            }

            if (!IoUtil.IsValidFilename(SignatureName))
            {
                ShowErrorPanel("Name must be legal as a filename");
                return;
            }

            if (SignatureManager.Contains(SignatureName))
            {
                ShowErrorPanel("That name is already in use");
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
    }
}
