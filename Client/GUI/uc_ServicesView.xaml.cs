using System.Windows.Controls;

namespace Eudora.Net.GUI
{
    /// <summary>
    /// Interaction logic for uc_ServicesView.xaml
    /// </summary>
    public partial class uc_ServicesView : ChildWindowBase
    {
        public enum eSubview
        {
            AddressBook,
            DirectoryServices,
            Filters,
            FilterReport,
            LinkHistory
        }

        public uc_ServicesView()
        {
            InitializeComponent();
        }

        public void SetActiveSubview(eSubview subview)
        {
            switch (subview)
            {
                case eSubview.AddressBook:
                    tc_ServicesView.SelectedIndex = 0;
                    break;
                //case eSubview.DirectoryServices:
                //    tc_ServicesView.SelectedIndex = 1;
                //    break;
                case eSubview.Filters:
                    tc_ServicesView.SelectedIndex = 1;
                    break;
                case eSubview.FilterReport:
                    tc_ServicesView.SelectedIndex = 2;
                    break;
                case eSubview.LinkHistory:
                    tc_ServicesView.SelectedIndex = 3;
                    break;
            }
        }

        private void tc_ServicesView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // TODO: This shouldn't be necessary but the auto-binding isn't right
            string TabTitle = "Services Tab";
            switch (tc_ServicesView.SelectedIndex)
            {
                case 0:
                    TabTitle = "Address Book";
                    break;
                case 1:
                    TabTitle = "Directory Services";
                    break;
                case 2:
                    TabTitle = "Filters";
                    break;
                case 3:
                    TabTitle = "Filter Report";
                    break;
                case 4:
                    TabTitle = "Link History";
                    break;
                default:
                    break;
            }

            MainWindow.Instance?.SetServicesTabTitle(TabTitle);
        }
    }
}
