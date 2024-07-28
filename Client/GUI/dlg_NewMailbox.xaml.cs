using Eudora.Net.Core;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;


namespace Eudora.Net.GUI
{
    /// <summary>
    /// Validation for the mailbox name entered by user.
    /// 0. Must not be blank
    /// 1. Must not already exist
    /// 2. Must be a legal filename
    /// 3. Must be a filename and not a new path (i.e, no folders in name)
    /// </summary>
    public class MailboxValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            // Not null or blank
            string? mailbox = value as string;
            if(mailbox == null || string.IsNullOrEmpty(mailbox) || string.IsNullOrWhiteSpace(mailbox))
            {
                return new ValidationResult(false, "Name cannot be blank");
            }

            // Must not exist already
            //if (PostOffice.Instance.Mailboxes.Contains(m => m.Name.Equals(mailbox, StringComparison.CurrentCultureIgnoreCase)))
            if(PostOffice.Instance.Mailboxes.Any(m => m.Name.Equals(mailbox, StringComparison.CurrentCultureIgnoreCase)))
            {
                return new ValidationResult(false, "That Mailbox already exists");
            }

            // Must be a legal filename
            if(mailbox.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) != -1)
            {
                return new ValidationResult(false, "Must be a legal filename");
            }

            // Filename only, no folders
            

            return ValidationResult.ValidResult;            
        }

        public static bool Validate(string text)
        {
            MailboxValidationRule rule = new MailboxValidationRule();
            ValidationResult result = rule.Validate(text, CultureInfo.CurrentCulture);
            return result.IsValid;
        }
    }



    /// <summary>
    /// Interaction logic for dlg_NewMailbox.xaml
    /// </summary>
    public partial class dlg_NewMailbox : Window
    {
        public string MailboxName
        {
            get { return (string)GetValue(MailboxNameProperty); }
            set { SetValue(MailboxNameProperty, value); }
        }
        public static readonly DependencyProperty MailboxNameProperty =
            DependencyProperty.Register("MailboxName", typeof(string), typeof(dlg_NewMailbox), new PropertyMetadata(string.Empty));

        public bool ValidationPassed
        {
            get { return (bool)GetValue(ValidationPassedProperty); }
            set { SetValue(ValidationPassedProperty, value); }
        }

        public static readonly DependencyProperty ValidationPassedProperty =
            DependencyProperty.Register("ValidationPassed", typeof(bool), typeof(dlg_NewMailbox), new PropertyMetadata(false));


        private ObservableCollection<Image> Images = [];

        public dlg_NewMailbox()
        {
            Mouse.OverrideCursor = Cursors.Wait;

            InitializeComponent();
            DataContext = this;
            Mouse.OverrideCursor = null;
            tb_Mailbox.Focus();
        }

        private void btn_OK_Click(object sender, RoutedEventArgs e)
        {
            if (!MailboxValidationRule.Validate(tb_Mailbox.Text))
            {
                return;
            }

            string name = tb_Mailbox.Text;
            PostOffice.Instance.AddUserMailbox(tb_Mailbox.Text, "pack://application:,,,/GUI/res/images/tb32/tb32_51.png");
            Close();
        }

        private void btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void tb_Mailbox_LostFocus(object sender, RoutedEventArgs e)
        {
            ValidationPassed = MailboxValidationRule.Validate(tb_Mailbox.Text);
        }

        private void tb_Mailbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidationPassed = MailboxValidationRule.Validate(tb_Mailbox.Text);
        }

        private void tb_Mailbox_KeyUp(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                btn_OK_Click(sender, null);
            }
        }
    }
}
