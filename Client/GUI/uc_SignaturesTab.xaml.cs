using Eudora.Net.Core;
using Eudora.Net.Data;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Eudora.Net.GUI
{
    /// <summary>
    /// Interaction logic for uc_SignaturesTab.xaml
    /// </summary>
    public partial class uc_SignaturesTab : uc_TabBase
    {
        public DelegateCommand EditCommand { get; set; }

        public uc_SignaturesTab()
        {
            EditCommand = new DelegateCommand((o) => btn_Edit_Click(o, null), (o) => listbox.SelectedItem != null);

            InitializeComponent();

            listbox.ItemsSource = SignatureManager.Datastore.Data;
            listbox.SelectedIndex = 0;
            
            EnableButtons(false);

            Loaded += Uc_SignaturesTab_Loaded;
        }

        private void EnableButtons(bool enable)
        {
            btn_Edit.IsEnabled = enable;
            btn_Delete.IsEnabled = enable;
        }

        private void Uc_SignaturesTab_Loaded(object sender, RoutedEventArgs e)
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

            if (listbox.SelectedItem is Signature signature)
            {
                if (MainWindow.Instance?.MDI.FindWindow(typeof(uc_SignatureView)) is not null)
                {
                    MainWindow.Instance?.ShowSignature(signature);
                }
            }
        }

        private void btn_New_Click(object sender, RoutedEventArgs e)
        {
            //var dlg = new dlg_NamePrompt(nameof(Signature));
            //dlg.Owner = MainWindow.Instance;
            //if (dlg.ShowDialog() == false) return;

            var signature = SignatureManager.New(GHelpers.MakeTempName("signature"));
            ListviewSelectLastItem();
            MainWindow.Instance?.ShowSignature(signature);
        }

        private void btn_Edit_Click(object? sender, RoutedEventArgs e)
        {
            if(listbox.SelectedValue is Signature signature)
            {
                MainWindow.Instance?.ShowSignature(signature);
            }
        }

        private void btn_Delete_Click(object sender, RoutedEventArgs e)
        {
            if (listbox.SelectedValue is Signature signature)
            {
                string prompt = "Are you sure you wish to delete this Signature? This cannot be undone.";
                dlg_Confirmation dlg = new dlg_Confirmation(prompt);
                var result = dlg.ShowDialog();
                if (result is null || result is false) return;
                
                SignatureManager.Remove(signature);
                ListviewSelectLastItem();

                var window = MainWindow.Instance?.MDI.FindWindow(typeof(uc_SignatureView), signature);
                if (window != null)
                {
                    window.Close();
                }
            }
        }
    }
}
