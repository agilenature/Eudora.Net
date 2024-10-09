using Eudora.Net.Core;
using Eudora.Net.Data;
using System.Windows;
using System.Windows.Controls;

namespace Eudora.Net.GUI
{
    /// <summary>
    /// Interaction logic for uc_PersonalityView.xaml
    /// NOTE: This window has no choice but to do some manual databinding and manipulation. 
    /// For example, while the checkbox for "Default Personality" is easily bound to the field, 
    /// it's also true that only one Personality can be the default.
    /// </summary>
    public partial class uc_PersonalityView : ChildWindowBase
    {
        public uc_PersonalityView()
        {
            InitializeComponent();
            BuildComboBoxes();
            DataContextChanged += Uc_PersonalityView_DataContextChanged;
        }

        private void BuildComboBoxes()
        {
            cb_Signature.Items.Clear();
            cb_Signature.Items.Add(GHelpers.NullSelection);
            foreach(var sig in SignatureManager.Datastore.Data)
            {
                cb_Signature.Items.Add(sig.Name);
            }            

            cb_Stationery.Items.Clear();
            cb_Stationery.Items.Add(GHelpers.NullSelection);
            foreach (var sta in StationeryManager.Datastore.Data)
            {
                cb_Stationery.Items.Add(sta.Name);
            }
        }


        

        /// <summary>
        /// Databinding that's tricky to do in Xaml
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Uc_PersonalityView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if(DataContext is Personality personality)
            {
                Title = personality.PersonalityName;
                
                if(personality.IsDefault)
                {
                    cbx_Default.IsChecked = true;
                    cbx_Default.IsEnabled = false;
                }
                else if(PersonalityManager.Datastore.Data.Count == 1)
                {
                    personality.MakeDefault();
                    cbx_Default.IsChecked = true;
                    cbx_Default.IsEnabled = false;
                }
                else
                {
                    cbx_Default.IsEnabled = true;
                }

                // SSL flags, outgoing mail
                switch(personality.SocketOptions_Outgoing)
                {
                    case MailKit.Security.SecureSocketOptions.None:
                        rb_out_none.IsChecked = true;
                        break;
                    case MailKit.Security.SecureSocketOptions.Auto:
                        rb_out_auto.IsChecked = true;
                        break;
                    case MailKit.Security.SecureSocketOptions.SslOnConnect:
                        rb_out_ssl.IsChecked = true;
                        break;
                    case MailKit.Security.SecureSocketOptions.StartTls:
                        rb_out_tls.IsChecked = true;
                        break;
                    case MailKit.Security.SecureSocketOptions.StartTlsWhenAvailable:
                        rb_out_trytls.IsChecked = true;
                        break;
                }

                // SSL flags, incoming mail
                switch (personality.SocketOptions_Incoming)
                {
                    case MailKit.Security.SecureSocketOptions.None:
                        rb_in_none.IsChecked = true;
                        break;
                    case MailKit.Security.SecureSocketOptions.Auto:
                        rb_in_auto.IsChecked = true;
                        break;
                    case MailKit.Security.SecureSocketOptions.SslOnConnect:
                        rb_in_ssl.IsChecked = true;
                        break;
                    case MailKit.Security.SecureSocketOptions.StartTls:
                        rb_in_tls.IsChecked = true;
                        break;
                    case MailKit.Security.SecureSocketOptions.StartTlsWhenAvailable:
                        rb_in_trytls.IsChecked = true;
                        break;
                }

                // Hide or show UX elements based on account type
                AdjustUXForEmailAddress(personality.EmailAddress);
            }
        }

        /// <summary>
        /// A little helper to transmute the tag (string) into a MailKit flag
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        private MailKit.Security.SecureSocketOptions SslFlagFromString(string tag)
        {
            MailKit.Security.SecureSocketOptions sslFlag = MailKit.Security.SecureSocketOptions.Auto;
            switch (tag)
            {
                case "none":
                    sslFlag = MailKit.Security.SecureSocketOptions.None;
                    break;
                case "auto":
                    sslFlag = MailKit.Security.SecureSocketOptions.Auto;
                    break;
                case "ssl":
                    sslFlag = MailKit.Security.SecureSocketOptions.SslOnConnect;
                    break;
                case "tls":
                    sslFlag = MailKit.Security.SecureSocketOptions.StartTls;
                    break;
                case "trytls":
                    sslFlag = MailKit.Security.SecureSocketOptions.StartTlsWhenAvailable;
                    break;
            }
            return sslFlag;
        }

        /// <summary>
        /// The click-handling and databinding of both SSL radiobutton groups really just comes down to
        /// tag and group name. Thus in the xaml, all those radiobuttons from both groups have the same click handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RadioGroup_SSL_Click(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton button)
            {
                var tag = button.Tag.ToString();
                if (!String.IsNullOrEmpty(tag))
                {
                    if (DataContext is Personality personality)
                    {
                        if(button.GroupName == "grp_SSL_Out")
                        {
                            personality.SocketOptions_Outgoing = SslFlagFromString(tag);
                        }
                        else
                        {
                            personality.SocketOptions_Incoming = SslFlagFromString(tag);
                        }                        
                    }
                }
            }
        }

        private void cb_Signature_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cb_Signature.SelectedItem is Signature signature)
            {
                ((Personality)DataContext).DefaultSignature = signature.Name;
            }
        }

        private void cb_Stationery_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cb_Stationery.SelectedItem is Stationery stationery)
            {
                ((Personality)DataContext).DefaultStationery = stationery.Name;
            }
        }

        private void cbx_Default_Checked(object sender, RoutedEventArgs e)
        {
            if(cbx_Default.IsChecked == true)
            {
                ((Personality)DataContext).MakeDefault();
            }
        }

        private void tb_Address_TextChanged(object sender, TextChangedEventArgs e)
        {
            AdjustUXForEmailAddress(tb_Address.Text);
        }

        private void AdjustUXForEmailAddress(string address)
        {
            if (address.Contains("@gmail.com", StringComparison.CurrentCultureIgnoreCase))
            {
                tblk_Password.IsEnabled = false;
                tb_Password.IsEnabled = false;
                TabOutgoing.IsEnabled = false;
                TabIncoming.IsEnabled = false;
                tblk_Oauth.Visibility = Visibility.Visible;
            }
            else
            {
                tblk_Password.IsEnabled = true;
                tb_Password.IsEnabled = true;
                TabOutgoing.IsEnabled = true;
                TabIncoming.IsEnabled = true;
                tblk_Oauth.Visibility = Visibility.Collapsed;
            }
        }
    }
}
