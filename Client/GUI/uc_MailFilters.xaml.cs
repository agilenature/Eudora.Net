using Eudora.Net.Core;
using Eudora.Net.Data;
using Eudora.Net.EmailSearch;
using Eudora.Net.ExtensionMethods;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace Eudora.Net.GUI
{
    /// <summary>
    /// Interaction logic for uc_MailFilters.xaml
    /// </summary>
    public partial class uc_MailFilters : SubviewBase
    {
        public EmailFilter Filter
        {
            get { return (EmailFilter)GetValue(FilterProperty); }
            set { SetValue(FilterProperty, value); }
        }

        public static readonly DependencyProperty FilterProperty =
            DependencyProperty.Register("Filter", typeof(EmailFilter), typeof(uc_MailFilters), 
                new PropertyMetadata(EmailFilter.Default(), FilterChangedCallback));

        private static void FilterChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(d is uc_MailFilters uc && e.NewValue is EmailFilter filter)
            {
                Debug.WriteLine("New filter is: " + filter.Name);
                //uc.SettingsPanel.DataContext = filter;
            }
        }

        private void Filter_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Debug.WriteLine("Filter property changed: " + e.PropertyName?.ToString());
            if(sender is EmailFilter filter)
            {
                Filter = filter;
            }
        }

        public uc_MailFilters()
        {
            InitializeComponent();
            Loaded += Uc_MailFilters_Loaded;
        }

        private void Uc_MailFilters_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = this;
            ConnectUX();
           // DataContextChanged += Uc_MailFilters_DataContextChanged;
            dg_Filters.SelectedIndex = 0;
        }

        private void Uc_MailFilters_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            //if(DataContext is EmailFilter filter)
            //{
            //    Filter = filter;
            //    //SettingsPanel.DataContext = filter;
            //    SettingsPanel.IsEnabled = true;
            //}
            //else
            //{
            //    //SettingsPanel.DataContext = null;
            //    SettingsPanel.IsEnabled = false;
            //}
        }

        public override void Activate()
        {
            base.Activate();
        }

        public override void Deactivate()
        {
            base.Deactivate();
        }

        private void ConnectUX() 
        {
            dg_Filters.ItemsSource = FilterManager.Collection;

            //cb_Query.ItemsSource = Core.EmailSearchEngine.QueryKeys;

            //cb_Operand.ItemsSource = Core.EmailSearchEngine.SearchOperands;

            sval_Priority.ItemsSource = Enum.GetValues(typeof(PostOffice.eMailPriority)).Cast<PostOffice.eMailPriority>();
            sval_Priority.SelectedIndex = 2;

            cb_Action.ItemsSource = EmailFilterActions.Actions;            

            apar_LabelParam.ItemsSource = LabelManager.Collection;

            apar_MailboxParam.ItemsSource = PostOffice.Instance.Mailboxes;

            apar_PersonalityParam.ItemsSource = PersonalityManager.Collection;

            apar_PriorityParam.ItemsSource = Enum.GetValues(typeof(PostOffice.eMailPriority)).Cast<PostOffice.eMailPriority>();

            apar_SoundParam.ItemsSource = GSoundPlayer.Instance.Sounds;
        }

        private void HideAllValueUX()
        {
            sval_String.Visibility = Visibility.Hidden;
            sval_Date.Visibility = Visibility.Hidden;
            sval_Priority.Visibility = Visibility.Hidden;
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
            else if (T == typeof(PostOffice.eMailPriority))
            {
                sval_Priority.Visibility = Visibility.Visible;
            }
        }

        private void HideAllParameterUX()
        {
            apar_StringParam.Visibility = Visibility.Hidden;
            apar_LabelParam.Visibility = Visibility.Hidden;
            apar_MailboxParam.Visibility = Visibility.Hidden;
            apar_PersonalityParam.Visibility = Visibility.Hidden;
            apar_PriorityParam.Visibility = Visibility.Hidden;
            apar_SoundParam.Visibility = Visibility.Hidden;
        }

        private void ActivateParameterUX(EmailFilterAction.eFilterActionKey key)
        {
            HideAllParameterUX();

            switch(key)
            {
                case EmailFilterAction.eFilterActionKey.Move:
                case EmailFilterAction.eFilterActionKey.Copy:
                    apar_MailboxParam.Visibility = Visibility.Visible;
                    break;

                case EmailFilterAction.eFilterActionKey.Forward:
                case EmailFilterAction.eFilterActionKey.Reply:
                    apar_PersonalityParam.Visibility= Visibility.Visible;
                    break;

                case EmailFilterAction.eFilterActionKey.Priority:
                    apar_PriorityParam.Visibility= Visibility.Visible;
                    break;

                case EmailFilterAction.eFilterActionKey.Label:
                    apar_LabelParam.Visibility= Visibility.Visible;
                    break;

                case EmailFilterAction.eFilterActionKey.Sound:
                    apar_SoundParam.Visibility = Visibility.Visible;
                    break;

                case EmailFilterAction.eFilterActionKey.Notify:
                    apar_StringParam.Visibility = Visibility.Visible;
                    break;
                
            }
        }

        private void dg_Filters_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dg_Filters.SelectedItem is EmailFilter filter)
            {
                Filter = filter;
                filter.PropertyChanged -= Filter_PropertyChanged;
                filter.PropertyChanged += Filter_PropertyChanged;
                SettingsPanel.IsEnabled = true;
            }
            else
            {
                SettingsPanel.IsEnabled = false;
            }
        }

        private void cb_Query_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cb_Query.SelectedItem is EmailSearch.QueryKey key)
            {
                //Atom.QueryKey = key;
                ActivateValueUX(QueryKey.Lookup[key.Key]);
            }
        }

        private void cb_Operand_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(cb_Operand.SelectedItem is EmailSearch.SearchOperand operand)
            {
                //Atom.Operand = operand;
            }
        }

        private void cb_Action_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cb_Action.SelectedItem is EmailFilterAction action)
            {
                ActivateParameterUX(action.ActionKey);
                //Filter.Action = action;
            }
        }

        private void btn_MoveUp_Click(object sender, RoutedEventArgs e)
        {
            EmailFilter? filter = dg_Filters.SelectedItem as EmailFilter;
            if (filter is null)
            {
                return;
            }

            filter.Priority++;
            dg_Filters.Sort(1, System.ComponentModel.ListSortDirection.Descending);
        }

        private void btn_MoveDown_Click(object sender, RoutedEventArgs e)
        {
            EmailFilter? filter = dg_Filters.SelectedItem as EmailFilter;
            if (filter is null)
            {
                return;
            }
            
            filter.Priority--;
            filter.Priority = Math.Max(0, filter.Priority);
            dg_Filters.Sort(1, System.ComponentModel.ListSortDirection.Descending);
        }

        private void btn_New_Click(object sender, RoutedEventArgs e)
        {
            FilterManager.New();
            dg_Filters.SelectedIndex = dg_Filters.Items.Count - 1;
        }

        private void btn_Remove_Click(object sender, RoutedEventArgs e)
        {
            if (dg_Filters.SelectedItem is not EmailFilter filter) return;

            string prompt = "Are you sure you want to delete this filter?";
            var dlg = new dlg_Confirmation(prompt);
            if(dlg.ShowDialog() is true)
            {
                FilterManager.Remove(filter);
            }
        }        
    }
}
