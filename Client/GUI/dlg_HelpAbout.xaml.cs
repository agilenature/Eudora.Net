using System.Windows;

namespace Eudora.Net.GUI
{
    /// <summary>
    /// Interaction logic for dlg_HelpAbout.xaml
    /// </summary>
    public partial class dlg_HelpAbout : Window
    {
        public dlg_HelpAbout()
        {
            Owner = Application.Current.MainWindow;
            InitializeComponent();
        }

        private void btn_Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
