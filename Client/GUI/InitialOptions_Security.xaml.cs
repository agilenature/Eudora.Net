using System.Windows;
using System.Windows.Controls;
using Eudora.Net.Core;

namespace Eudora.Net.GUI
{
    /// <summary>
    /// Interaction logic for InitialOptions_Security.xaml
    /// </summary>
    public partial class InitialOptions_Security : Page
    {
        public InitialOptions_Security()
        {
            InitializeComponent();
            RetrieveExistingCredential();
        }

        private void btn_GenKey_Click(object sender, RoutedEventArgs e)
        {
            string key = GCrypto.GenerateMasterKey();
            GCrypto.SetMasterKey(key);
            tb_Key.Text = key;
        }

        private void RetrieveExistingCredential()
        {
            var key = GCrypto.GetMasterKey();
            if(key is not null && !string.IsNullOrWhiteSpace(key))
            {
                tb_Key.Text = key;
                tb_Key.IsEnabled = false;
                btn_GenKey.IsEnabled = false;
                tbx_Instructions.IsEnabled = false;
            }
        }
    }
}
