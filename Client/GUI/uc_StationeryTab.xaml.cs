using Eudora.Net.Data;
using System.Windows;
using System.Windows.Controls;

namespace Eudora.Net.GUI
{
    /// <summary>
    /// Interaction logic for uc_StationeryTab.xaml
    /// </summary>
    public partial class uc_StationeryTab : uc_TabBase
    {
        public uc_StationeryTab()
        {
            InitializeComponent();
            listbox.ItemsSource = StationeryManager.Datastore.Data;
            listbox.SelectedIndex = 0;
            EnableButtons(false);

            Loaded += Uc_StationeryTab_Loaded;
        }

        private void EnableButtons(bool enable)
        {
            btn_Edit.IsEnabled = enable;
            btn_Delete.IsEnabled = enable;
        }

        private void Uc_StationeryTab_Loaded(object sender, RoutedEventArgs e)
        {
            if (listbox.Items.Count > 0)
            {
                listbox.SelectedItem = listbox.Items[0];
                EnableButtons(true);
            }
        }

        private void ListviewSelectLastItem()
        {
            if (listbox.Items.Count > 0)
            {
                listbox.SelectedIndex = listbox.Items.Count - 1;
                EnableButtons(true);
            }
        }

        private void listbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EnableButtons(listbox.SelectedItem != null);
        }

        private void btn_New_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new dlg_NamePrompt(nameof(Stationery));
            dlg.Owner = MainWindow.Instance;
            if (dlg.ShowDialog() == false) return;

            var stationery = StationeryManager.New(dlg.ItemName);
            ListviewSelectLastItem();
            MainWindow.Instance?.ShowStationery(stationery);
        }

        private void btn_Edit_Click(object sender, RoutedEventArgs e)
        {
            if (listbox.SelectedItem is Stationery stationery)
            {
                MainWindow.Instance?.ShowStationery(stationery);
            }
        }

        private void btn_Delete_Click(object sender, RoutedEventArgs e)
        {
            if (listbox.SelectedItem is Stationery stationery)
            {
                string prompt = "Are you sure you wish to delete this Stationery? This cannot be undone.";
                dlg_Confirmation dlg = new dlg_Confirmation(prompt);
                var result = dlg.ShowDialog();
                if (result is null || result is false) return;

                StationeryManager.Remove(stationery);
                ListviewSelectLastItem();

                var window = MainWindow.Instance?.MDI.FindWindow(typeof(uc_StationeryView), stationery);
                if (window != null)
                {
                    window.Close();
                }
            }
        }
    }
}
