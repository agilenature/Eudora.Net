using Eudora.Net.Core;
using Eudora.Net.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Eudora.Net.GUI
{
    /// <summary>
    /// Interaction logic for uc_PersonalitiesTab.xaml
    /// </summary>
    public partial class uc_PersonalitiesTab : uc_TabBase
    {
        public uc_PersonalitiesTab()
        {
            InitializeComponent();
            listview.ItemsSource = PersonalityManager.Datastore.Data;
            btn_Delete.IsEnabled = false;
        }

        private void ListviewSelectLastItem()
        {
            if (listview.Items.Count > 0)
            {
                listview.SelectedIndex = listview.Items.Count - 1;
            }
        }

        /// <summary>
        /// This is Eudora 7.1 behavior: double-click creates a new email
        /// using the selected personality as sender.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listview_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (listview.SelectedItem is Personality personality)
            {
                MainWindow.Instance?.ShowMailMessage(PostOffice.CreateMessage_Outgoing(personality));
            }
        }

        private void btn_New_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new dlg_NamePrompt(nameof(Personality));
            dlg.Owner = MainWindow.Instance;
            bool? result = dlg.ShowDialog();
            if (result is null || result is false) return;
            var personality = PersonalityManager.New(dlg.ItemName);
            if (personality is null) return;
            MainWindow.Instance?.ShowPersonality(personality);
            ListviewSelectLastItem();
        }

        private void btn_Delete_Click(object sender, RoutedEventArgs e)
        {
            if (listview.SelectedItem is Personality personality)
            {
                string prompt = "Are you sure you wish to delete this Personality? This cannot be undone.";
                dlg_Confirmation dlg = new dlg_Confirmation(prompt);
                if (dlg.ShowDialog() is true)
                {
                    PersonalityManager.Remove(personality);
                    ListviewSelectLastItem();
                }
            }
        }

        private void btn_Edit_Click(object sender, RoutedEventArgs e)
        {
            if (listview.SelectedItem is Personality personality)
            {
                MainWindow.Instance?.ShowPersonality(personality);
            }
        }

        private void listview_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btn_Delete.IsEnabled = false;
            if (listview.SelectedItem is Personality personality)
            {
                btn_Delete.IsEnabled = true;
            }
        }
    }
}
