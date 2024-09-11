using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;

namespace Eudora.Net.GUI
{
    /// <summary>
    /// Interaction logic for uc_Options_Datastore.xaml
    /// </summary>
    public partial class uc_Options_Datastore : UserControl
    {
        public uc_Options_Datastore()
        {
            InitializeComponent();
        }

        private void btn_StorageRoot_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFolderDialog
            {
                Multiselect = false,
                Title = "Eudora.Net: Choose Storage Root Folder",
                ValidateNames = true,
                InitialDirectory = Properties.Settings.Default.DataStoreRoot
            };
            var result = dlg.ShowDialog();
            if (result is null || result == false) return;

            string folder = dlg.SafeFolderName;
            tb_Folder.Text = folder;
        }
    }
}
