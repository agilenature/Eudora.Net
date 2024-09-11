using Eudora.Net.Data;
using System.Windows.Controls;

namespace Eudora.Net.GUI
{
    /// <summary>
    /// Interaction logic for uc_Options_Browser.xaml
    /// </summary>
    public partial class uc_Options_Browser : UserControl
    {
        public uc_Options_Browser()
        {
            InitializeComponent();

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
    }
}
