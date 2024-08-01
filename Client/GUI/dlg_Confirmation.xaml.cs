using System.Windows;

namespace Eudora.Net.GUI
{
    /// <summary>
    /// Interaction logic for dlg_Confirmation.xaml
    /// </summary>
    public partial class dlg_Confirmation : Window
    {
        public string Message { get; set; } = string.Empty;

        public dlg_Confirmation(string message)
        {
            Owner = Application.Current.MainWindow;
            InitializeComponent();
            DataContext = this;
            Message = message;
        }

        private void btn_Confirm_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            Close();
        }

        private void btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            Close();
        }

        private void btn_NCA_Close_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            Close();
        }
    }
}
