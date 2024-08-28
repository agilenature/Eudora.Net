using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using sbux.wpf.Themer;

namespace Eudora.Net.GUI
{
    /// <summary>
    /// Interaction logic for InitialOptions_Theme.xaml
    /// </summary>
    public partial class InitialOptions_Theme : Page
    {
        public InitialOptions_Theme()
        {
            InitializeComponent();

            cb_Themes.ItemsSource = ThemeManager.Themes;
            cb_Themes.SelectedItem = ThemeManager.Themes.FirstOrDefault(t => 
                t.DisplayName.Equals(Eudora.Net.Properties.Settings.Default.UxTheme, StringComparison.CurrentCultureIgnoreCase));
        }

        private void cb_Themes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(cb_Themes.SelectedItem is sbux.wpf.Themer.Theme theme)
            {
                ThemeManager.SetTheme(theme.DisplayName);
                Eudora.Net.Properties.Settings.Default.UxTheme = theme.DisplayName;
                Eudora.Net.Properties.Settings.Default.Save();
            }
        }
    }
}
