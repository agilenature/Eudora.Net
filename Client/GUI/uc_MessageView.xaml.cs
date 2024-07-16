using Eudora.Net.Core;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Eudora.Net.Data;

namespace Eudora.Net.GUI
{
    /// <summary>
    /// Interaction logic for uc_MessageView.xaml
    /// </summary>
    public partial class uc_MessageView : ChildWindowBase
    {
        //////////////////////////////////////////////////////////////
        #region Construct/Init
        //////////////////////////////////////////////////////////////

        public uc_MessageView()
        {
            InitializeComponent();
            
            // Load the Sender combo with available personalities
            cb_Sender.ItemsSource = PersonalityManager.Datastore.Data;
            DataContextChanged += Uc_MessageView_DataContextChanged;         
        }

        //////////////////////////////////////////////////////////////
        #endregion Construct/Init
        //////////////////////////////////////////////////////////////



        //////////////////////////////////////////////////////////////
        #region Interface
        //////////////////////////////////////////////////////////////

        /// <summary>
        /// 
        /// </summary>
        private void UpdateTitle()
        {
            var message = DataContext as EmailMessage;
            if (message == null)
            {
                return;
            }

            // 1. Construct the new title
            string title = String.Format("{0}, {1}, {2}",
                message.Addresses_To.FirstOrDefault(),
                message.Date.ToString(),
                message.Subject);
                
            // 2. Apply the new title
            Title = title;
            MainWindow.Instance?.UpdateMessageViewTitle(message, title);
        }


        //////////////////////////////////////////////////////////////
        #endregion Interface
        //////////////////////////////////////////////////////////////
        


        //////////////////////////////////////////////////////////////
        #region Internal Utilities
        //////////////////////////////////////////////////////////////

        /// <summary>
        /// The DataContext for this UC class family is a MailMessage
        /// When it is set, we can tell if we're looking at a draft message
        /// or a finished message. The relevant GUI must be enabled or disabled
        /// depending on which it is.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Uc_MessageView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            EmailMessage? message = DataContext as EmailMessage;
            if(message == null)
            {
                return;
            }

            Editor.DataContext = message.Body;
            bool isDraft = (message.Status == EmailMessage.MessageStatus.Draft);
            EnableEdit(isDraft);
        }

        /// <summary>
        /// Edit is enabled by default.
        /// For viewing incoming messages, much of the GUI must be disabled.
        /// </summary>
        /// <param name=""></param>
        /// <param name=""></param>
        private void EnableEdit(bool enable)
        {
            ToolbarRegion.IsEnabled = enable;
            HeaderRegion.IsEnabled = enable;
            Editor.IsEnabled = enable;
        }


        //////////////////////////////////////////////////////////////
        #endregion Internal Utilities
        //////////////////////////////////////////////////////////////
        


        //////////////////////////////////////////////////////////////
        #region Validation
        //////////////////////////////////////////////////////////////

        /// <summary>
        /// Verify that this email address is well-formed
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        private bool IsAddressValid(string address)
        {
            // 1. Verify string is legal, meaning not zero length or otherwise blank
            if(String.IsNullOrEmpty(address))
            {
                return false;
            }
            if(String.IsNullOrWhiteSpace(address))
            {
                return false;
            }

            // 2. Check overall structure comforms to something@something.extension
           


            // All checks passed
            return true;
        }


        /// <summary>
        ///
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private bool ValidateMessage(EmailMessage message)
        {
            // 1. Address checking

            // Check sender
            if(!IsAddressValid(message.SenderAddress.Address))
            {
                return false;
            }

            // Check TO
            foreach(EmailAddress address in message.Addresses_To)
            {
                if(!IsAddressValid(address.Address))
                {
                    return false;
                }
            }

            // Check CC
            foreach (EmailAddress address in message.Addresses_CC)
            {
                if (!IsAddressValid(address.Address))
                {
                    return false;
                }
            }

            // Check BCC
            foreach (EmailAddress address in message.Addresses_BCC)
            {
                if (!IsAddressValid(address.Address))
                {
                    return false;
                }
            }

            // All checks passed
            return true;
        }

        //////////////////////////////////////////////////////////////
        #endregion Validation
        //////////////////////////////////////////////////////////////



        



        //////////////////////////////////////////////////////////////
        #region Events that trigger save
        //////////////////////////////////////////////////////////////

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cb_Sender_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var personality = cb_Sender.SelectedItem as Personality;
            if (personality != null)
            {
                var message = DataContext as EmailMessage;
                if (message != null)
                {
                    message.PersonalityID = personality.Id;
                    //Mailboxes.Save(message.MailboxID);
                }
            }
        }

        /// <summary>
        /// Subject line changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tb_Subject_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateTitle();
        }

        /// <summary>
        /// Body content changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Editor_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tb_To_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateTitle();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tb_CC_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tb_BCC_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        //////////////////////////////////////////////////////////////
        #endregion Events that trigger save
        //////////////////////////////////////////////////////////////




        //////////////////////////////////////////////////////////////
        #region Toolbar event handlers
        //////////////////////////////////////////////////////////////

        /// <summary>
        /// Quoted Printable
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbb_QuotedPrintable_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbb_TextAsAttachment_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbb_WordWrap_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbb_TabsInBody_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbb_KeepCopies_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbb_ReturnReceipt_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbb_InvisibleChars_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbb_MoodWatch_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void tbb_Send_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            EmailMessage? message = DataContext as EmailMessage;
            if (message != null)
            {
                if(ValidateMessage(message))
                {
                    await PostOffice.Instance.SendMessage(message);                    
                }                
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbb_FontSize_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbb_Bold_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbb_Italic_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbb_Underline_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbb_Strikeout_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbb_TextColor_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cb_Priority_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cb_Signature_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cb_Font_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // This looks stupid but it happens on init because
            // the selection is set to 0 before the editor is instantiated
            if(Editor == null)
            {
                return;
            }

            FontFamily? font = cb_Font.SelectedItem as FontFamily;
            if(font != null)
            {
                Editor.FontFamily = font;
                
            }
        }

        

        //////////////////////////////////////////////////////////////
        #endregion Toolbar event handlers
        //////////////////////////////////////////////////////////////
    }
}
