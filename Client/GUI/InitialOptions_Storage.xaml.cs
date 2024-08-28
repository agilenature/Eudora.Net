using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;

namespace Eudora.Net.GUI
{
    /// <summary>
    /// Interaction logic for InitialOptions_Storage.xaml
    /// </summary>
    public partial class InitialOptions_Storage : Page
    {
        public InitialOptions_Storage()
        {
            InitializeComponent();
        }

        private void btn_SetDatastore_Click(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog ofd = new();
            ofd.Multiselect = false;
            ofd.Title = "Select the folder to serve as the data store";
            ofd.ValidateNames = true;
            
            if (ofd.ShowDialog() == true)
            {
                tb_Datastore.Text = ofd.FolderName;
                Eudora.Net.Properties.Settings.Default.DataStoreRoot = ofd.FolderName;
                Eudora.Net.Properties.Settings.Default.Save();
            }
        }
    }
}
