using sbux.wpf.Themer;
using System.Windows.Controls;

namespace Eudora.Net.GUI
{
    /// <summary>
    /// Interaction logic for uc_Options_Ux.xaml
    /// </summary>
    public partial class uc_Options_Ux : UserControl
    {
        public uc_Options_Ux()
        {
            InitializeComponent();

            cb_ThemeSelect.ItemsSource = ThemeManager.Themes;
            cb_ThemeSelect.DisplayMemberPath = "DisplayName";
            var theme = ThemeManager.Themes.Where(x => x.DisplayName == Properties.Settings.Default.UxTheme).FirstOrDefault();
            cb_ThemeSelect.SelectedItem = theme ?? ThemeManager.Themes.First();
        }

        private void cb_ThemeSelect_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (cb_ThemeSelect.SelectedItem is Theme theme)
            {
                ThemeManager.SetTheme(theme.DisplayName);
                Properties.Settings.Default.UxTheme = theme.DisplayName;
                Properties.Settings.Default.Save();
            }
        }
    }
}
