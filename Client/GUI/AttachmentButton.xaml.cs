using Eudora.Net.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;


namespace Eudora.Net.GUI
{
    /// <summary>
    /// Interaction logic for AttachmentButton.xaml
    /// </summary>
    public partial class AttachmentButton : Button
    {
        ///////////////////////////////////////////////////////////
        #region Dependency Properties
        /////////////////////////////

        public static readonly DependencyProperty DisplayTextProperty =
            DependencyProperty.Register("DisplayText", typeof(String), typeof(AttachmentButton), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty IconImageProperty =
            DependencyProperty.Register("IconImage", typeof(BitmapSource), typeof(AttachmentButton), new PropertyMetadata(null));

        /////////////////////////////
        #endregion Dependency Properties
        ///////////////////////////////////////////////////////////
        


        ///////////////////////////////////////////////////////////
        #region Properties
        /////////////////////////////

        public String DisplayText
        {
            get { return (String)GetValue(DisplayTextProperty); }
            set { SetValue(DisplayTextProperty, value); }
        }

        public BitmapSource? IconImage
        {
            get { return (BitmapSource)GetValue(IconImageProperty); }
            set { SetValue(IconImageProperty, value); }
        }

        private EmailAttachment _Attachment = new();
        public EmailAttachment Attachment
        {
            get => _Attachment;
            set { if (_Attachment != value) _Attachment = value; }
        }

        /////////////////////////////
        #endregion Properties
        ///////////////////////////////////////////////////////////
        

        public AttachmentButton()
        {
            InitializeComponent();
        }

        public AttachmentButton(EmailAttachment attachment)
        {
            InitializeComponent();

            DataContext = this;
            Attachment = attachment;
            DisplayText = attachment.Name;
            IconImage = IconCache.GetBitmapSourceFromFile(attachment.Path);
        }
    }
}
