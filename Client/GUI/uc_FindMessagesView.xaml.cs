using Eudora.Net.Core;
using Eudora.Net.Data;
using Eudora.Net.EmailSearch;
using Eudora.Net.ExtensionMethods;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;


namespace Eudora.Net.GUI
{
    /// <summary>
    /// Interaction logic for uc_FindMessagesView.xaml
    /// </summary>
    public partial class uc_FindMessagesView : ChildWindowBase
    {
        ///////////////////////////////////////////////////////////
        #region Properties
        /////////////////////////////

        private static EmailSearch.EmailSearchAtom Atom = new();
        public static ObservableCollection<EmailMessage> Results { get; set; } = [];

        /////////////////////////////
        #endregion Properties
        ///////////////////////////////////////////////////////////

        public uc_FindMessagesView()
        {
            InitializeComponent();
            Title = "Find Messages";
            ConnectUX();
            //lb_Mailboxes.ItemsSource = SearchMechanism.Mailboxes;
        }


        private void ConnectUX()
        {
            // results panel
            lv_Results.ItemsSource = Results;

            // Search panel
            SearchPanel.DataContext = Atom;

            // Mailboxes panel
            lb_Mailboxes.DataContext = Atom;
            lb_Mailboxes.ItemsSource = Atom.MailboxList;

            // fields
            cb_Field.ItemsSource = EmailSearchEngine.QueryKeys;
            cb_Field.DisplayMemberPath = "DisplayName";
            cb_Field.SelectedIndex = 0;

            // condition
            cb_Operand.ItemsSource = EmailSearchEngine.SearchOperands;
            cb_Operand.DisplayMemberPath = "DisplayName";
            cb_Operand.SelectedIndex = 0;

            // data-entry controls
            sval_Priority.ItemsSource = Enum.GetValues(typeof(PostOffice.eMailPriority)).Cast<PostOffice.eMailPriority>();
            sval_Priority.SelectedIndex = 2;
        }

        private void HideAllValueUX()
        {
            sval_String.Visibility = Visibility.Hidden;
            sval_Date.Visibility = Visibility.Hidden;
            sval_Priority.Visibility = Visibility.Hidden;
        }

        private void ActivateUX(bool activate)
        {
            SearchPanel.IsEnabled = activate;
            lb_Mailboxes.IsEnabled = activate;
        }

        private void ActivateValueUX(Type T)
        {
            HideAllValueUX();

            if (T == typeof(string))
            {
                sval_String.Visibility = Visibility.Visible;
            }
            else if (T == typeof(DateTime))
            {
                sval_Date.Visibility = Visibility.Visible;
            }
            else if (T == typeof(Guid))
            {

            }
            else if (T == typeof(EmailMessage.eReadStatus))
            {

            }
            else if (T == typeof(EmailMessage.eSendStatus))
            {

            }
            else if (T == typeof(EmailMessage.MessageOrigin))
            {

            }
            else if (T == typeof(EmailMessage.MessageStatus))
            {

            }
            else if (T == typeof(PostOffice.eMailPriority))
            {
                sval_Priority.Visibility = Visibility.Visible;
            }
        }

        private async void btn_Search_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ActivateUX(false);

                // Execute the search on the thread pool
                var result = await Task.Run(() => EmailSearchEngine.ExecuteSearch(Atom));


                // Note: This may look unnecessary, but it goes right around the problem
                // of a collectionview having its bound collection changed in another thread.
                Results.Clear();
                Results.AddRangeUnique(result);
                lv_Results.ItemsSource = Results;

                ActivateUX(true);
            }
            catch(Exception ex)
            {
                Logger.Exception(ex);
            }
        }

        private void cb_Field_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(cb_Field.SelectedItem is EmailSearch.QueryKey key)
            {
                Atom.QueryKey = key;
                ActivateValueUX(QueryKey.Lookup[key.Key]);
            }
        }

        private void cb_Operand_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cb_Operand.SelectedItem is EmailSearch.SearchOperand operand)
            {
                Atom.Operand = operand;
            }
        }

        private void sval_Date_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sval_Date.SelectedDate is DateTime dt)
            {
                Atom.DateTimeValue = dt;
            }
        }
    }
}
