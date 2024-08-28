using Eudora.Net.Core;
using Eudora.Net.Data;
using Microsoft.Win32;
using System.Windows;
using sbux.wpf.Themer;

namespace Eudora.Net.GUI
{
    /// <summary>
    /// Interaction logic for uc_OptionsView.xaml
    /// </summary>
    public partial class uc_OptionsView : ChildWindowBase
    {
        public uc_OptionsView()
        {
            InitializeComponent();
            Loaded += Uc_OptionsView_Loaded;

            cb_ThemeSelect.ItemsSource = ThemeManager.Themes;
            cb_ThemeSelect.DisplayMemberPath = "DisplayName";
            var theme = ThemeManager.Themes.Where(x => x.DisplayName == Properties.Settings.Default.UxTheme).FirstOrDefault();
            cb_ThemeSelect.SelectedItem = theme ?? ThemeManager.Themes.First();
        }

        private void Uc_OptionsView_Loaded(object sender, RoutedEventArgs e)
        {
            cb_SearchEngine.ItemsSource = BrowserData.SearchEngines.Data;
            if (BrowserData.Instance.ActiveSearchEngine is null)
            {
                cb_SearchEngine.SelectedIndex = 0;
                return;
            }
            cb_SearchEngine.SelectedIndex = BrowserData.SearchEngines.Data.IndexOf(BrowserData.Instance.ActiveSearchEngine);
        }

        private void Cb_SearchEngine_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (cb_SearchEngine.SelectedItem is BrowserSearchEngine bse)
            {
                BrowserData.Instance.ActiveSearchEngine = bse;
            }
        }

        public void ActivateTab(int index)
        {
            SettingsTabs.SelectedIndex = index;
        }

        private void btn_StorageRoot_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFolderDialog();
            dlg.Multiselect = false;
            dlg.Title = "Eudora.Net: Choose Storage Root Folder";
            dlg.ValidateNames = true;
            dlg.InitialDirectory = Properties.Settings.Default.DataStoreRoot;
            var result = dlg.ShowDialog();
            if (result is null || result == false) return;

            string folder = dlg.SafeFolderName;
            tb_Folder.Text = folder;
        }

        private void btn_OK_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btn_Help_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btn_Cancel_Click(object sender, RoutedEventArgs e)
        {

        }

        private void cb_ThemeSelect_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if(cb_ThemeSelect.SelectedItem is Theme theme)
            {
                ThemeManager.SetTheme(theme.DisplayName);
                Properties.Settings.Default.UxTheme = theme.DisplayName;
                Properties.Settings.Default.Save();
            }
        }

        private void sld_Frequency_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Parallel.Invoke(() => PostOffice.UpdateTimerFrequency());
            Parallel.Invoke(async () => Properties.Settings.Default.Save());
        }
    }
}
