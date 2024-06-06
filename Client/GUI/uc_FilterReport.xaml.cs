using Eudora.Net.Core;
using System.Windows.Controls;

namespace Eudora.Net.GUI
{
    /// <summary>
    /// Interaction logic for uc_FilterReport.xaml
    /// </summary>
    public partial class uc_FilterReport : UserControl
    {
        public uc_FilterReport()
        {
            InitializeComponent();
            datagrid.ItemsSource = EmailFilterReporter.Reports;
        }
    }
}
