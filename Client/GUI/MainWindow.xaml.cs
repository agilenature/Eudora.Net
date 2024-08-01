using Eudora.Net.Core;
using mdilib;
using System.Windows;
using Eudora.Net.Data;
using System.Windows.Controls;
using Eudora.Net.ExtensionMethods;
using WpfThemer;

namespace Eudora.Net.GUI
{
    /// <summary>
    /// The application's Main Window
    /// </summary>
    public partial class MainWindow : Window
    {
        ///////////////////////////////////////////////////////////
        #region Properties
        /////////////////////////////

        public static MainWindow? Instance { get; set; } = null;

        /////////////////////////////
        #endregion Properties
        ///////////////////////////////////////////////////////////



        ///////////////////////////////////////////////////////////
        #region Construction / MainWindow Ops
        /////////////////////////////

        public MainWindow()
        {
            InitializeComponent();

            MDI.ActiveChildChangedEvent += MDI_ActiveChildChangedEvent;
            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
            PostOffice.Instance.Mailboxes.CollectionChanged += Mailboxes_CollectionChanged;
            PopulateMailboxMenu();
            HandleUxStateOnWindowChange();
        }

        private void PostStartupChecks()
        {
            // 1. Prompt user to create a Personality if none exist
            if(PersonalityManager.Datastore.Data.Count == 0)
            {
                var dlg = new dlg_PromptCreatePersonality();
                dlg.Owner = this;
                if(dlg.ShowDialog() is true)
                {
                    ShowPersonalitiesTab();
                    Personality p = PersonalityManager.New("default");
                    ShowPersonality(p);
                }
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            PostStartupChecks();
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
        }

        private void Mailboxes_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            PopulateMailboxMenu();
        }

        private void PopulateMailboxMenu()
        {
            Menu_Mailboxes.Items.Clear();
            
            foreach(var mailbox in PostOffice.Instance.Mailboxes)
            {
                MenuItem item = new()
                {
                    Header = mailbox.Name,
                    Icon = mailbox.ImageSource,
                    Tag = mailbox.Name
                };
                item.Click += MailboxMenuItem_Click;
                Menu_Mailboxes.Items.Add(item);
            }

            Menu_Mailboxes.Items.Add(new Separator());

            MenuItem newMailboxItem = new()
            {
                Header = "New"
            };
            newMailboxItem.Click += Menu_Mailbox_New_Click;
            Menu_Mailboxes.Items.Add(newMailboxItem);
        }

        public void PopulateMailTransferMenu(Mailbox? currentMailbox)
        {
            Menu_Transfer.Items.Clear();
            if (currentMailbox is null)
            {
                return;
            }

            foreach(var mailbox in PostOffice.Instance.Mailboxes)
            {
                if(mailbox != currentMailbox)
                {
                    MenuItem item = new()
                    {
                        Header = mailbox.Name,
                        Icon = mailbox.ImageSource,
                        Tag = mailbox.Name
                    };
                    Menu_Transfer.Items.Add(item);
                }
            }
        }

        private void MDI_ActiveChildChangedEvent(MdiChild newWindow, MdiChild oldWindow)
        {
            HandleUxStateOnWindowChange();

            if (oldWindow is not null && oldWindow.Content is ChildWindowBase windowO)
            {
                windowO.MdiDeactivated();
            }

            if (newWindow is not null && newWindow.Content is ChildWindowBase windowN)
            {
                windowN.MdiActivated();
            }
        }

        /// <summary>
        /// Because Eudora is built with the old UX thinking of "big main menu, big main toolbar"
        /// we must keep track of which menus and buttons should be enabled or disabled as the
        /// active view changes. In the modern world, the relevant UX would be part of
        /// the child view it relates to (thus it would come, go, enable, and disable automatically)
        /// </summary>
        private void HandleUxStateOnWindowChange()
        {
            // A sanity measure: begin by disabling everything

            MainMenu.DisableAllItems();
            MainToolBar.DisableAllControls();

            // Enable the main menu items that are always active

            // File
            Menu_File.IsEnabled = true;
            Menu_File_Exit.IsEnabled = true;

            // Mailboxes
            Menu_Mailboxes.IsEnabled = true;
            Menu_Mailboxes.EnableAllSubitems();

            // Tools
            Menu_Tools.IsEnabled = true;
            Menu_Tools.EnableAllSubitems();

            // Help
            Menu_Help.IsEnabled = true;
            Menu_Help.EnableAllSubitems();

            Menu_Transfer.IsEnabled = false;
            Menu_Transfer.EnableAllSubitems();


            // Now selectively enable based on active view type

            // No view is active
            if (MDI.ActiveMdiChild is null)
            {
                /* Main Menu */

                /* Main Toolbar */

                btn_Inbox.IsEnabled = true;
                btn_Drafts.IsEnabled = true;
                btn_CheckMail.IsEnabled = true;
                btn_NewMsg.IsEnabled = true;
                btn_FindMsg.IsEnabled = true;
                btn_AddressBook.IsEnabled = true;
                btn_Browser.IsEnabled = true;
                btn_Settings.IsEnabled = true;
            }
            // There is an active view
            else
            {
                MdiChild? activeWindow = MDI.ActiveMdiChild;
                Type t = activeWindow.Content.GetType();

                if(t == typeof(uc_AddressBook))
                {
                    /* Main Menu */

                    /* Main Toolbar */

                    btn_Inbox.IsEnabled = true;
                    btn_Drafts.IsEnabled = true;
                    btn_CheckMail.IsEnabled = true;
                    btn_NewMsg.IsEnabled = true;
                    btn_FindMsg.IsEnabled = true;
                    btn_AddressBook.IsEnabled = true;
                    btn_Browser.IsEnabled = true;
                    btn_Settings.IsEnabled = true;
                }
                else if(t == typeof(uc_BrowserView))
                {
                    /* Main Menu */

                    /* Main Toolbar */

                    btn_Inbox.IsEnabled = true;
                    btn_Drafts.IsEnabled = true;
                    btn_CheckMail.IsEnabled = true;
                    btn_NewMsg.IsEnabled = true;
                    btn_FindMsg.IsEnabled = true;
                    btn_AddressBook.IsEnabled = true;
                    btn_Browser.IsEnabled = true;
                    btn_Settings.IsEnabled = true;
                }
                else if(t == typeof(uc_CombinedMailboxView))
                {
                    /* Main Menu */


                    /* Main Toolbar */

                    btn_Inbox.IsEnabled = true;
                    btn_Drafts.IsEnabled = true;
                    btn_CheckMail.IsEnabled = true;
                    btn_NewMsg.IsEnabled = true;
                    btn_FindMsg.IsEnabled = true;
                    btn_AddressBook.IsEnabled = true;
                    btn_Browser.IsEnabled = true;
                    btn_Settings.IsEnabled = true;
                }
                else if(t == typeof(uc_EmailView))
                {
                    /* Main Menu */

                    // File
                    Menu_File.IsEnabled = true;
                    Menu_File_Print.IsEnabled = true;
                    Menu_File_Exit.IsEnabled = true;

                    // Edit
                    uc_EmailView? view = MDI.ActiveMdiChild.Content as uc_EmailView;
                    if(view is not null && view.IsEditable())
                    {
                        Menu_Edit.IsEnabled = true;
                        Menu_Edit.EnableAllSubitems();
                    }



                    /* Main Toolbar */

                    btn_Inbox.IsEnabled = true;
                    btn_Drafts.IsEnabled = true;
                    btn_CheckMail.IsEnabled = true;
                    btn_NewMsg.IsEnabled = true;
                    btn_FindMsg.IsEnabled = true;
                    btn_AddressBook.IsEnabled = true;
                    btn_Browser.IsEnabled = true;
                    btn_Settings.IsEnabled = true;
                }
                else if(t == typeof(uc_EudoraStatisticsView))
                {
                    /* Main Menu */


                    /* Main Toolbar */

                    btn_Inbox.IsEnabled = true;
                    btn_Drafts.IsEnabled = true;
                    btn_CheckMail.IsEnabled = true;
                    btn_NewMsg.IsEnabled = true;
                    btn_FindMsg.IsEnabled = true;
                    btn_AddressBook.IsEnabled = true;
                    btn_Browser.IsEnabled = true;
                    btn_Settings.IsEnabled = true;
                }
                else if(t == typeof(uc_FindMessagesView))
                {
                    /* Main Menu */

                    /* Main Toolbar */

                    btn_Inbox.IsEnabled = true;
                    btn_Drafts.IsEnabled = true;
                    btn_CheckMail.IsEnabled = true;
                    btn_NewMsg.IsEnabled = true;
                    btn_FindMsg.IsEnabled = true;
                    btn_AddressBook.IsEnabled = true;
                    btn_Browser.IsEnabled = true;
                    btn_Settings.IsEnabled = true;
                }
                else if(t == typeof(uc_OptionsView))
                {
                    /* Main Menu */


                    /* Main Toolbar */

                    btn_Inbox.IsEnabled = true;
                    btn_Drafts.IsEnabled = true;
                    btn_CheckMail.IsEnabled = true;
                    btn_NewMsg.IsEnabled = true;
                    btn_FindMsg.IsEnabled = true;
                    btn_AddressBook.IsEnabled = true;
                    btn_Browser.IsEnabled = true;
                    btn_Settings.IsEnabled = true;
                }
                else if(t == typeof(uc_PersonalityView))
                {
                    /* Main Menu */


                    /* Main Toolbar */

                    btn_Inbox.IsEnabled = true;
                    btn_Drafts.IsEnabled = true;
                    btn_CheckMail.IsEnabled = true;
                    btn_NewMsg.IsEnabled = true;
                    btn_FindMsg.IsEnabled = true;
                    btn_AddressBook.IsEnabled = true;
                    btn_Browser.IsEnabled = true;
                    btn_Settings.IsEnabled = true;
                }
                else if(t == typeof(uc_ServicesView))
                {
                    /* Main Menu */


                    /* Main Toolbar */

                    btn_Inbox.IsEnabled = true;
                    btn_Drafts.IsEnabled = true;
                    btn_CheckMail.IsEnabled = true;
                    btn_NewMsg.IsEnabled = true;
                    btn_FindMsg.IsEnabled = true;
                    btn_AddressBook.IsEnabled = true;
                    btn_Browser.IsEnabled = true;
                    btn_Settings.IsEnabled = true;
                }
                else if(t == typeof(uc_SignatureView))
                {
                    /* Main Menu */

                    // Edit
                    Menu_Edit.IsEnabled = true;
                    Menu_Edit.EnableAllSubitems();


                    /* Main Toolbar */

                    btn_Inbox.IsEnabled = true;
                    btn_Drafts.IsEnabled = true;
                    btn_CheckMail.IsEnabled = true;
                    btn_NewMsg.IsEnabled = true;
                    btn_FindMsg.IsEnabled = true;
                    btn_AddressBook.IsEnabled = true;
                    btn_Browser.IsEnabled = true;
                    btn_Settings.IsEnabled = true;
                }
                else if(t == typeof(uc_StationeryView))
                {
                    /* Main Menu */

                    // Edit
                    Menu_Edit.IsEnabled = true;
                    Menu_Edit.EnableAllSubitems();


                    /* Main Toolbar */
                    btn_Inbox.IsEnabled = true;
                    btn_Drafts.IsEnabled = true;
                    btn_CheckMail.IsEnabled = true;
                    btn_NewMsg.IsEnabled = true;
                    btn_FindMsg.IsEnabled = true;
                    btn_AddressBook.IsEnabled = true;
                    btn_Browser.IsEnabled = true;
                    btn_Settings.IsEnabled = true;
                }
            }
        }

        /////////////////////////////
        #endregion Construction / MainWindow Ops
        ///////////////////////////////////////////////////////////



        ///////////////////////////////////////////////////////////
        #region View Management
        /////////////////////////////

        public void EnableUX(bool enable)
        {
            IsEnabled = enable;
        }

        public void FocusWindow(MdiChild window)
        {
            MDI.SetActiveWindow(window);
        }

        public void ShowEventLog(bool show)
        {
            if(show)
            {
                EventLog.Visibility = Visibility.Visible;
                LoggerPanel.MinHeight = 128;
                LoggerPanel.Height = new GridLength(Eudora.Net.Properties.Settings.Default.MainViewRowBHeight, GridUnitType.Pixel);
            }
            else
            {
                EventLog.Visibility = Visibility.Collapsed;
                LoggerPanel.MinHeight = 0;
                LoggerPanel.Height = new GridLength(0, GridUnitType.Pixel);
            }
        }

        public void ShowOptionsView()
        {
            var existingView = MDI.FindWindow(typeof(uc_OptionsView));
            if(existingView is not null)
            {
                FocusWindow(existingView);
                return;
            }

            _ = new uc_OptionsView();
        }

        public void ActivateOptionsTab(int index)
        {
            ShowOptionsView();
            var optionsWindow = MDI.FindWindow(typeof(uc_OptionsView));
            if (optionsWindow is not null)
            {
                var options = optionsWindow.Content as uc_OptionsView;
                options?.ActivateTab(index);
            }
        }

        public void ShowMessageSearchView()
        {
            var existingView = MDI.FindWindow(typeof(uc_FindMessagesView));
            if(existingView is not null)
            {
                FocusWindow(existingView);
                return;
            }

            _ = new uc_FindMessagesView();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        public void ShowMailbox(string name)
        {
            // If this mailbox is active already, focus it
            var mailboxWindow = MDI.FindWindow(name);
            if (mailboxWindow is not null)
            {
                FocusWindow(mailboxWindow);
                return;
            }

            // Mailbox view not active; create it
            Mailbox? mailbox = PostOffice.Instance.GetMailboxByName(name);
            if (mailbox is null)
            {
                return;
            }
            
            if(mailbox.Equals(PostOffice.Inbox))
            {
                _ = new uc_MailboxInbox();
            }
            else if(mailbox.Equals(PostOffice.Drafts))
            {
                _ = new uc_MailboxDrafts();
            }
            else if (mailbox.Equals(PostOffice.Sent))
            {
                _ = new uc_MailboxSent();
            }
            else if (mailbox.Equals(PostOffice.Trash))
            {
                _ = new uc_MailboxTrash();
            }
            else
            {
                _ = new uc_CombinedMailboxView(mailbox, true, true);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public void ShowMailMessage(EmailMessage message)
        {
            // If this message is being viewed already, focus it.
            var window = MDI.FindWindow(typeof(uc_EmailView), message);
            if (window is not null)
            {
                FocusWindow(window);
                return;
            }

            // A view for this message does not already exist. Create it
            //var view = new uc_EmailView(message);
            //FocusWindow(view.Window);
            _ = new uc_EmailView(message);
        }

        public void CloseMailMessage(EmailMessage message)
        {
            var window = MDI.FindWindow(typeof(uc_EmailView), message);
            window?.Close();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="personality"></param>
        public void ShowPersonality(Personality personality)
        {
            var window = MDI.FindWindow(typeof(uc_PersonalityView));
            if(window != null)
            {
                window.DataContext = personality;
                FocusWindow(window);
                return;
            }

            _ = new uc_PersonalityView() { DataContext = personality };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="signature"></param>
        public void ShowSignature(Signature signature)
        {
            var window = MDI.FindWindow(typeof(uc_SignatureView));
            if (window != null)
            {
                window.DataContext = signature;
                FocusWindow(window);
                return;
            }

            _ = new uc_SignatureView(signature) { DataContext = signature };
        }

        public void ShowStationery(Stationery stationery)
        {
            var window = MDI.FindWindow(typeof(uc_StationeryView));
            if (window != null)
            {
                window.DataContext = stationery;
                FocusWindow(window);
                return;
            }

            _ = new uc_StationeryView(stationery) { DataContext = stationery };
        }

        public void ShowEudoraStatisticsView()
        {
            var window = MDI.FindWindow(typeof(uc_EudoraStatisticsView));
            if(window is not null)
            {
                FocusWindow(window);
                return;
            }

            _ = new uc_EudoraStatisticsView();
        }

        public void ShowBrowserView()
        {
            var window = MDI.FindWindow(typeof(uc_BrowserView));
            if(window is not null)
            {
                FocusWindow(window);
                return;
            }

            _ = new uc_BrowserView();
        }

        private void ActivateServicesView()
        {
            MdiChild? view = MDI.FindWindow(typeof(uc_ServicesView));
            if (view is null)
            {
                ShowServicesView();
            }
            else
            {
                FocusWindow(view);
            }
        }

        private void ActivateServicesViewTab(uc_ServicesView.eSubview tab)
        {
            ActivateServicesView();
            MdiChild? window = MDI.FindWindow(typeof(uc_ServicesView));
            if (window is not null)
            {
                var view = window.Content as uc_ServicesView;
                view?.SetActiveSubview(tab);
            }
        }

        public void ShowServicesView()
        {
            _ = new uc_ServicesView();
        }

        public void ShowAddressBookView()
        {
            ActivateServicesViewTab(uc_ServicesView.eSubview.AddressBook);
        }

        public void ShowFiltersView()
        {
            ActivateServicesViewTab(uc_ServicesView.eSubview.Filters);
        }

        public void ShowFilterReportView()
        {
            ActivateServicesViewTab(uc_ServicesView.eSubview.FilterReport);
        }

        public void ShowLinkHistoryView()
        {
            ActivateServicesViewTab(uc_ServicesView.eSubview.LinkHistory);
        }

        public void ShowMailboxesTab()
        {
            tabControl_Left.SelectedIndex = 0;
        }

        public void ShowFileBrowserTab()
        {
            tabControl_Left.SelectedIndex = 1;
        }

        public void ShowSignaturesTab()
        {
            tabControl_Left.SelectedIndex = 2;
        }

        public void ShowStationeryTab()
        {
            tabControl_Left.SelectedIndex = 3;
        }

        public void ShowPersonalitiesTab()
        {
            tabControl_Left.SelectedIndex = 4;
        }

        

        public void ShowHelp()
        {
            
        }

        public void UpdateMessageViewTitle(EmailMessage message, string title)
        {
            var window = MDI.FindWindow(typeof(uc_EmailView), message);
            if(window is not null)
            {
                window.Title = title;
            }
        }

        public void SetServicesTabTitle(string title)
        {
            var window = MDI.FindWindow(typeof(uc_ServicesView));
            if(window is not null)
            {
                window.Title = title;
            }
        }


        /////////////////////////////
        #endregion View Management
        ///////////////////////////////////////////////////////////



        ///////////////////////////////////////////////////////////
        #region Main Toolbar
        /////////////////////////////

        private void btn_Inbox_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            ShowMailbox("Inbox");
        }

        private void btn_Drafts_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            ShowMailbox("Drafts");
        }

        private void btn_CheckMail_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            PostOffice.Instance.CheckMail();
        }

        private void btn_NewMsg_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            var defaultPersonality = PersonalityManager.DefaultPersonality();
            if (defaultPersonality is not null)
            {
                ShowMailMessage(PostOffice.CreateMessage_Outgoing(defaultPersonality));
            }
            else
            {
                // prompt to create an account
            }
        }

        private void btn_FindMsg_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            ShowMessageSearchView();
        }

        private void btn_AddressBook_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            ShowAddressBookView();
        }

        private void btn_Browser_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            ShowBrowserView();
        }

        private void btn_Help_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            ShowHelp();
        }

        private void btn_Settings_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            ShowOptionsView();
        }

        /////////////////////////////
        #endregion Main Toolbar
        ///////////////////////////////////////////////////////////



        ///////////////////////////////////////////////////////////
        #region Main Menu
        /////////////////////////////


        //
        // MENU: FILE
        private void Menu_File_Print_Click(object sender, RoutedEventArgs e)
        {
            //e.Handled = true;
        }


        private void Menu_File_Exit_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            // TODO: App Exit/Clieanup must occur first
            Application.Current.Shutdown();
        }

        //
        // MENU: MAILBOX
        private void MailboxMenuItem_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            if (e.Source is MenuItem item)
            {
                ShowMailbox((string)item.Tag);
                e.Handled = true;
            }
        }

        private void Menu_Mailbox_New_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            dlg_NewMailbox dlg = new dlg_NewMailbox();
            dlg.Owner = this;
            dlg.ShowDialog();
        }

        //
        // MENU: MESSAGE
        private void Menu_Message_New_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            var defaultPersonality = PersonalityManager.DefaultPersonality();
            if (defaultPersonality is not null)
            {
                ShowMailMessage(PostOffice.CreateMessage_Outgoing(defaultPersonality));
            }
            else
            {
                // prompt to create an account
            }
        }

        //
        // MENU: TOOLS
        private void Menu_Tools_Filters_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            ShowFiltersView();
        }

        private void Menu_Tools_Report_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            ShowFilterReportView();
        }

        private void Menu_Tools_Mailboxes_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            ShowMailboxesTab();
        }

        private void Menu_Tools_FileBrowser_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            ShowFileBrowserTab();
        }

        private void Menu_Tools_Stationery_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            ShowStationeryTab();
        }

        private void Menu_Tools_Signatures_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            ShowSignaturesTab();
        }

        private void Menu_Tools_Personalities_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            ShowPersonalitiesTab();
        }

        private void Menu_Tools_AddressBook_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            ShowAddressBookView();
        }

        private void Menu_Tools_LinkHistory_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            ShowLinkHistoryView();
        }

        private void Menu_Tools_Statistics_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            ShowEudoraStatisticsView();
        }

        private void Menu_Tools_Options_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            ShowOptionsView();
        }

        private void Menu_Tools_EventLog_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            if (EventLog.Visibility == Visibility.Visible)
            {
                ShowEventLog(false);
            }
            else
            {
                ShowEventLog(true);
            }
        }

        private void Menu_Tools_Import_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            var dlg = new dlg_ImportWizard();
            dlg.Owner = this;
            dlg.ShowDialog();
        }

        //
        // MENU: WINDOW
        private void Menu_Window_Cascade_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            //MDI.MdiLayout = MdiLayout.Cascade;
        }

        private void Menu_Window_TileHorizontal_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            //MDI.MdiLayout = MdiLayout.TileHorizontal;
        }

        private void Menu_Window_TileVertical_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            //MDI.MdiLayout = MdiLayout.TileVertical;
        }

        //
        // MENU: HELP
        private void Menu_Help_Topics_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void Menu_Help_About_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            var dlg = new dlg_HelpAbout();
            dlg.Owner = this;
            dlg.ShowDialog();
        }

        private void Menu_Tools_Browser_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            ShowBrowserView();
        }

        private void Menu_Help_Feedback_Click(object sender, RoutedEventArgs e)
        {
            dlg_SubmitFeedback dlg = new();
            if(dlg.ShowDialog() == false)
            {
                return;
            }

            IssueReporter.ReportFeedback(dlg.Feedback);
        }

        private void Menu_Help_Contents_Click(object sender, RoutedEventArgs e)
        {

        }

        







        /////////////////////////////
        #endregion Main Menu
        //////////////////////////////////////////////////////////

    }
}