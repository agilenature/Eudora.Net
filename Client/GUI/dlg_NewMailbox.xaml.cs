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
            string mailboxLc = mailbox.ToLower();
            foreach(var mb in PostOffice.Instance.Mailboxes)
            {
                if(mb.Name.ToLower().Equals(mailboxLc, StringComparison.CurrentCultureIgnoreCase))
                {
                    return new ValidationResult(false, "That Mailbox already exists");
                }
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

            BuildImageList();
            lb_Image.ItemsSource = Images;
            lb_Image.SelectedIndex = 0;
            DataContext = this;
            Mouse.OverrideCursor = null;
        }

        private void BuildImageList()
        {
            for (UInt32 index = 0; index < 128; index++)
            {
                string uri = String.Format("pack://application:,,,/GUI/res/images/tb32/tb32_{0}.png", index);
                //Images.Add(new BitmapImage(new Uri(uri)));
                Image img = new Image();
                BitmapImage source = new BitmapImage(new Uri(uri));
                img.Source = source;
                Images.Add(img);
            }
        }

        private void btn_OK_Click(object sender, RoutedEventArgs e)
        {
            var img = lb_Image.SelectedItem as Image;
            if(img == null)
            {
                return;
            }

            if(!MailboxValidationRule.Validate(tb_Mailbox.Text))
            {
                return;
            }

            string name = tb_Mailbox.Text;
            PostOffice.Instance.AddUserMailbox(tb_Mailbox.Text, img.Source.ToString());
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
    }
}
