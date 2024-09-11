using Eudora.Net.Core;
using System.Windows;
using System.Windows.Controls;

namespace Eudora.Net.GUI
{
    /// <summary>
    /// Interaction logic for uc_Options_PostOffice.xaml
    /// </summary>
    public partial class uc_Options_PostOffice : UserControl
    {
        public uc_Options_PostOffice()
        {
            InitializeComponent();
        }

        private void sld_Frequency_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Parallel.Invoke(() => PostOffice.UpdateTimerFrequency());
            Parallel.Invoke(async () => Properties.Settings.Default.Save());
        }
    }
}
