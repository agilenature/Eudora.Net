using Eudora.Net.Core;
using Eudora.Net.Data;
using Microsoft.Win32;
using System.Windows;


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
        }

        private void Uc_OptionsView_Loaded(object sender, RoutedEventArgs e)
        {
            cb_SearchEngine.ItemsSource = BrowserSettings.SearchEngines;
            if (BrowserSettings.Instance.ActiveSearchEngine is null)
            {
                cb_SearchEngine.SelectedIndex = 0;
                return;
            }
            cb_SearchEngine.SelectedIndex = BrowserSettings.SearchEngines.IndexOf(BrowserSettings.Instance.ActiveSearchEngine);
        }

        private void Cb_SearchEngine_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (cb_SearchEngine.SelectedItem is BrowserSearchEngine bse)
            {
                BrowserSettings.Instance.ActiveSearchEngine = bse;
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
    }
}
