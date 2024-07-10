﻿using Eudora.Net.Core;
using Eudora.Net.Data;
using Eudora.Net.ExtensionMethods;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Eudora.Net.GUI
{
    /// <summary>
    /// Interaction logic for uc_CombinedMailboxView.xaml
    /// </summary>
    public partial class uc_CombinedMailboxView : ChildWindowBase
    {
        ///////////////////////////////////////////////////////////
        #region DependencyProperties
        /////////////////////////////        

        public static readonly DependencyProperty MailboxProperty =
            DependencyProperty.Register("Mailbox", typeof(Mailbox), typeof(uc_CombinedMailboxView), new PropertyMetadata(null, MailboxChangedCallback));

        private static void MailboxChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(d is uc_CombinedMailboxView view && view.Mailbox is not null)
            {
                view.Title = view.Mailbox.Name;
                if(view.Mailbox.Name.Equals("trash", StringComparison.CurrentCultureIgnoreCase))
                {
                    view.IsTrashbox = true;
                }
            }
        }

        public static readonly DependencyProperty IsInboxProperty =
            DependencyProperty.Register("IsInbox", typeof(bool), typeof(uc_CombinedMailboxView), new PropertyMetadata(false, IsInboxChangedCallback));

        private static void IsInboxChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is uc_CombinedMailboxView view && e.NewValue is not null)
            {
                view.Column_ReadStatus.Visibility = view.IsInbox ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public static readonly DependencyProperty IsOutboxProperty =
            DependencyProperty.Register("IsOutbox", typeof(bool), typeof(uc_CombinedMailboxView), new PropertyMetadata(false, IsOutboxChangedCallback));

        private static void IsOutboxChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is uc_CombinedMailboxView view && e.NewValue is not null)
            {
                view.Column_SendStatus.Visibility = view.IsOutbox ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public static readonly DependencyProperty IsTrashboxProperty =
            DependencyProperty.Register("IsTrashbox", typeof(bool), typeof(uc_CombinedMailboxView), new PropertyMetadata(false));

        /////////////////////////////
        #endregion DependencyProperties
        ///////////////////////////////////////////////////////////



        ///////////////////////////////////////////////////////////
        #region Properties
        /////////////////////////////

        private Mailbox Mailbox
        {
            get { return (Mailbox)GetValue(MailboxProperty); }
            set { SetValue(MailboxProperty, value); }
        }

        private bool IsInbox
        {
            get { return (bool)GetValue(IsInboxProperty); }
            set { SetValue(IsInboxProperty, value); }
        }

        private bool IsOutbox
        {
            get { return (bool)GetValue(IsOutboxProperty); }
            set { SetValue(IsOutboxProperty, value); }
        }

        private bool IsTrashbox
        {
            get { return (bool)GetValue(IsTrashboxProperty); }
            set { SetValue(IsTrashboxProperty, value); }
        }

        private bool _IsPrintEnabled = false;
        private bool IsPrintEnabled
        {
            get => _IsPrintEnabled;
            set => _IsPrintEnabled = value;
        }

        private void Menu_File_Print_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            preview.Print();
        }

        /////////////////////////////
        #endregion Properties
        ///////////////////////////////////////////////////////////

        public uc_CombinedMailboxView(Mailbox mailbox, bool isInbox, bool isOutbox)
        {
            InitializeComponent();

            IsInbox = isInbox;
            Column_ReadStatus.Visibility = IsInbox ? Visibility.Visible : Visibility.Collapsed;

            IsOutbox = isOutbox;
            Column_SendStatus.Visibility = IsOutbox ? Visibility.Visible : Visibility.Collapsed;

            Mailbox = mailbox;
            DataContext = Mailbox;
            datagrid.ItemsSource = mailbox.Messages;

            var wnd = MainWindow.Instance;
            if (wnd is not null)
            {
                wnd.PopulateMailTransferMenu(Mailbox);
                foreach (MenuItem item in wnd.Menu_Transfer.Items)
                {
                    item.Click += Menu_Transfer_Click;
                }
            }
        }

        public override void MdiActivated()
        {
            base.MdiActivated();
            UpdateMainWndUX(true);
        }

        public override void MdiDeactivated()
        {
            base.MdiDeactivated();
            UpdateMainWndUX(false);
        }

        private void UpdateMainWndUX(bool enable)
        {
            var wnd = MainWindow.Instance;
            if (wnd is null) return;

            if (enable)
            {
                // Message menu
                wnd.Menu_Message.IsEnabled = true;
                wnd.Menu_Message.EnableAllSubitems();
                wnd.Menu_Message_Reply.Click += Btn_Reply_Click;
                wnd.Menu_Message_ReplyAll.Click += Btn_ReplyAll_Click;
                wnd.Menu_Message_Forward.Click += Btn_Forward_Click;

                // MainUX buttons
                wnd.btn_Delete.IsEnabled = true;
                wnd.btn_Delete.Click += Btn_Delete_Click;
                wnd.btn_Next.IsEnabled = true;
                wnd.btn_Next.Click += Btn_Next_Click;
                wnd.btn_Prev.IsEnabled = true;
                wnd.btn_Prev.Click += Btn_Prev_Click;
                wnd.btn_Reply.IsEnabled = true;
                wnd.btn_Reply.Click += Btn_Reply_Click;
                wnd.btn_ReplyAll.IsEnabled = true;
                wnd.btn_ReplyAll.Click += Btn_ReplyAll_Click;
                wnd.btn_Forward.IsEnabled = true;
                wnd.btn_Forward.Click += Btn_Forward_Click;

                // Mail Transfer menu
                wnd.PopulateMailTransferMenu(Mailbox);
                foreach (MenuItem item in wnd.Menu_Transfer.Items)
                {
                    item.Click += Menu_Transfer_Click;
                }

                // Print support
                wnd.Menu_File_Print.IsEnabled = true;
                wnd.Menu_File_Print.Click += Menu_File_Print_Click;
            }
            else
            {
                // Message menu
                wnd.Menu_Message.IsEnabled = false;
                wnd.Menu_Message.DisableAllSubitems();
                wnd.Menu_Message_Reply.Click -= Btn_Reply_Click;
                wnd.Menu_Message_ReplyAll.Click -= Btn_ReplyAll_Click;
                wnd.Menu_Message_Forward.Click -= Btn_Forward_Click;

                // MainUX buttons
                wnd.btn_Delete.IsEnabled = false;
                wnd.btn_Delete.Click -= Btn_Delete_Click;
                wnd.btn_Next.IsEnabled = false;
                wnd.btn_Next.Click -= Btn_Next_Click;
                wnd.btn_Prev.IsEnabled = false;
                wnd.btn_Prev.Click -= Btn_Prev_Click;
                wnd.btn_Reply.IsEnabled = false;
                wnd.btn_Reply.Click -= Btn_Reply_Click;
                wnd.btn_ReplyAll.IsEnabled = false;
                wnd.btn_ReplyAll.Click -= Btn_ReplyAll_Click;
                wnd.btn_Forward.IsEnabled = false;
                wnd.btn_Forward.Click -= Btn_Forward_Click;
                //wnd.Menu_File_Print.Click -= Menu_File_Print_Click;

                // Mail Transfer menu
                wnd.PopulateMailTransferMenu(null);
                foreach (MenuItem item in wnd.Menu_Transfer.Items)
                {
                    item.Click -= Menu_Transfer_Click;
                }

                // Print support
                wnd.Menu_File_Print.IsEnabled = false;
                wnd.Menu_File_Print.Click -= Menu_File_Print_Click;
            }
        }

        private void datagrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;
            if (datagrid.SelectedItem is EmailMessage message)
            {
                preview.DataContext = message;
                message.ReadStatus = EmailMessage.eReadStatus.Read;
                IsPrintEnabled = true;
                UpdateMainWndUX(true);
            }
            else
            {
                preview.DataContext = null;
                IsPrintEnabled = false;
                UpdateMainWndUX(false);
            }
        }

        private void datagrid_Sorting(object sender, DataGridSortingEventArgs e)
        {
            e.Handled = true;
            string colSortMember = e.Column.SortMemberPath;
            ListSortDirection? sortDirection = e.Column.SortDirection;
            if(sortDirection is null || sortDirection == ListSortDirection.Descending)
            {
                sortDirection = ListSortDirection.Ascending;
            }
            else
            {
                sortDirection = ListSortDirection.Descending;
            }
            datagrid.Sort(e.Column.DisplayIndex, (ListSortDirection)sortDirection);
        }

        private void DataGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGridRow row && row.DataContext is EmailMessage message)
            {
                e.Handled = true;
                //message.ReadStatus = EmailMessage.eReadStatus.Read;
                MainWindow.Instance?.ShowMailMessage(message);
            }
        }

        private void SelectNearest(int index)
        {
            if (datagrid.Items.Count > 0)
            {
                datagrid.SelectedIndex = 0;
            }
        }

        private void DeleteSelectedMessages()
        {
            List<EmailMessage>? selectedItems = datagrid.SelectedItems.Cast<EmailMessage>().ToList();
            if (selectedItems is null) return;
            foreach (var row in selectedItems)
            {
                if (row is EmailMessage message)
                {
                    if (preview.DataContext == message)
                    {
                        preview.DataContext = null;
                    }

                    var window = MainWindow.Instance?.MDI.FindWindow(message);
                    window?.Close();

                    if (IsTrashbox)
                    {
                        Mailbox.DeleteMessage(message);
                    }
                    else
                    {
                        PostOffice.Instance.MoveMessage(message, "Trash");
                    }
                }
            }
        }

        private void DataGridRow_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                e.Handled = true;
                DeleteSelectedMessages();
                SelectNearest(0);
            }
            else if(e.Key == Key.Enter)
            {
                if (datagrid.SelectedItem is EmailMessage message)
                {
                    e.Handled = true;
                    MainWindow.Instance?.ShowMailMessage(message);
                }
            }
        }

        private void Btn_Forward_Click(object sender, RoutedEventArgs e)
        {
            if (datagrid.SelectedItem is EmailMessage messageIn)
            {
                e.Handled = true;
                var messageOut = PostOffice.Instance.CreateMessage_Forward(messageIn);
                MainWindow.Instance?.ShowMailMessage(messageOut);
            }
        }

        private void Btn_ReplyAll_Click(object sender, RoutedEventArgs e)
        {
            if (datagrid.SelectedItem is EmailMessage messageIn)
            {
                e.Handled = true;
                var messageOut = PostOffice.Instance.CreateMessage_ReplyAll(messageIn);
                MainWindow.Instance?.ShowMailMessage(messageOut);
            }
        }

        private void Btn_Reply_Click(object sender, RoutedEventArgs e)
        {
            if (datagrid.SelectedItem is EmailMessage messageIn)
            {
                e.Handled = true;
                var messageOut = PostOffice.Instance.CreateMessage_Reply(messageIn);
                MainWindow.Instance?.ShowMailMessage(messageOut);
            }
        }

        private void Btn_Delete_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            DeleteSelectedMessages();
            SelectNearest(0);
        }

        private void Btn_Prev_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            PreviousMessage();
        }

        private void Btn_Next_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            NextMessage();
        }

        private void Menu_Transfer_Click(object sender, RoutedEventArgs e)
        {
            if (datagrid.SelectedItem is EmailMessage message && e.Source is MenuItem item)
            {
                e.Handled = true;
                PostOffice.Instance.MoveMessage(message, (string)item.Tag);

                if (preview.DataContext == message)
                {
                    preview.DataContext = null;
                }

                var window = MainWindow.Instance?.MDI.FindWindow(message);
                window?.Close();
            }
        }

        public void NextMessage()
        {
            if (datagrid.SelectedIndex < datagrid.Items.Count)
            {
                datagrid.SelectedIndex++;
                datagrid.ScrollIntoView(datagrid.SelectedItem);
            }
        }

        public void PreviousMessage()
        {
            if (datagrid.SelectedIndex > 0)
            {
                datagrid.SelectedIndex--;
                datagrid.ScrollIntoView(datagrid.SelectedItem);
            }
        }

        private void cmenu_Open_Click(object sender, RoutedEventArgs e)
        {
            if (datagrid.SelectedItem is EmailMessage message)
            {
                e.Handled = true;
                MainWindow.Instance?.ShowMailMessage(message);
            }
        }

        private void cmenu_PriHi_Click(object sender, RoutedEventArgs e)
        {
            if (datagrid.SelectedItem is EmailMessage message)
            {
                e.Handled = true;
                message.Priority = PostOffice.eMailPriority.High;
            }
        }

        private void cmenu_PriNml_Click(object sender, RoutedEventArgs e)
        {
            if (datagrid.SelectedItem is EmailMessage message)
            {
                e.Handled = true;
                message.Priority = PostOffice.eMailPriority.Normal;
            }
        }

        private void cmenu_PriLow_Click(object sender, RoutedEventArgs e)
        {
            if (datagrid.SelectedItem is EmailMessage message)
            {
                e.Handled = true;
                message.Priority = PostOffice.eMailPriority.Low;
            }
        }

        private void cmenu_Label1_Click(object sender, RoutedEventArgs e)
        {
            if (datagrid.SelectedItem is EmailMessage message)
            {
                e.Handled = true;
                message.LabelName = LabelManager.Collection[0].Name;
            }
        }

        private void cmenu_Label2_Click(object sender, RoutedEventArgs e)
        {
            if (datagrid.SelectedItem is EmailMessage message)
            {
                e.Handled = true;
                message.LabelName = LabelManager.Collection[1].Name;
            }
        }

        private void cmenu_Label3_Click(object sender, RoutedEventArgs e)
        {
            if (datagrid.SelectedItem is EmailMessage message)
            {
                e.Handled = true;
                message.LabelName = LabelManager.Collection[2].Name;
            }
        }

        private void cmenu_Label4_Click(object sender, RoutedEventArgs e)
        {
            if (datagrid.SelectedItem is EmailMessage message)
            {
                e.Handled = true;
                message.LabelName = LabelManager.Collection[3].Name;
            }
        }

        private void cmenu_Label5_Click(object sender, RoutedEventArgs e)
        {
            if (datagrid.SelectedItem is EmailMessage message)
            {
                e.Handled = true;
                message.LabelName = LabelManager.Collection[4].Name;
            }
        }

        private void cmenu_Label6_Click(object sender, RoutedEventArgs e)
        {
            if (datagrid.SelectedItem is EmailMessage message)
            {
                e.Handled = true;
                message.LabelName = LabelManager.Collection[5].Name;
            }
        }

        private void cmenu_Label7_Click(object sender, RoutedEventArgs e)
        {
            if (datagrid.SelectedItem is EmailMessage message)
            {
                e.Handled = true;
                message.LabelName = LabelManager.Collection[6].Name;
            }
        }

        private void cmenu_Reply_Click(object sender, RoutedEventArgs e)
        {
            Btn_Reply_Click(sender, e);
        }

        private void cmenu_ReplyAll_Click(object sender, RoutedEventArgs e)
        {
            Btn_ReplyAll_Click(sender, e);
        }

        private void cmenu_Forward_Click(object sender, RoutedEventArgs e)
        {
            Btn_Forward_Click(sender, e);
        }

        private void cmenu_MoveTrash_Click(object sender, RoutedEventArgs e)
        {
            Btn_Delete_Click(sender, e);
        }
    }
}
