using System.Windows;

namespace Eudora.Net.GUI
{
    /// <summary>
    /// Interaction logic for dlg_PromptCreatePersonality.xaml
    /// </summary>
    public partial class dlg_PromptCreatePersonality : Window
    {
        public dlg_PromptCreatePersonality()
        {
            Owner = Application.Current.MainWindow;
            InitializeComponent();
            DataContext = this;
        }

        private void btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void btn_OK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void btn_NCA_Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
