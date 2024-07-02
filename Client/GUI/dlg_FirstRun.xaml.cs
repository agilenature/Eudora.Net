using Eudora.Net.Core;
using Eudora.Net.Data;
using Microsoft.Win32;
using System.Windows;

namespace Eudora.Net.GUI
{
    /// <summary>
    /// Interaction logic for dlg_FirstRun.xaml
    /// </summary>
    public partial class dlg_FirstRun : Window
    {
        public string DataRoot
        {
            get { return (string)GetValue(DataRootProperty); }
            set { SetValue(DataRootProperty, value); }
        }
        public static readonly DependencyProperty DataRootProperty =
            DependencyProperty.Register("DataRoot", typeof(string), typeof(dlg_FirstRun), 
                new PropertyMetadata(string.Empty));



        public dlg_FirstRun(string initialDataRoot)
        {
            DataContext = this;
            InitializeComponent();
            DataRoot = initialDataRoot;
        }

        private void btn_OK_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            ClearError();
            if(!DataValidation.IsValidPath(tb_StorageRoot.Text))
            {
                ShowError();
                return;
            }
            Properties.Settings.Default.FirstRunOptionsComplete = true;
            DialogResult = true;
            Close();
        }

        private void btn_BrowseStorageRoot_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFolderDialog();
            dlg.Multiselect = false;
            dlg.Title = "Eudora.Net: Choose Storage Root Folder";
            dlg.ValidateNames = true;
            dlg.InitialDirectory = Properties.Settings.Default.DataStoreRoot;
            var result = dlg.ShowDialog();
            if (result is null || result == false) return;

            DataRoot = dlg.FolderName;
        }

        private void ShowError()
        {
            tb_StorageRoot.BorderBrush = GHelpers.ErrorBrush;
            tb_Error.Text =
                "Error\n\n" +
                "The selected folder is invalid";
        }

        private void ClearError()
        {
            tb_StorageRoot.BorderBrush = SystemColors.ActiveBorderBrush;
            tb_Error.Text = string.Empty;
        }
    }
}
