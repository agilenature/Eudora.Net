using System.Windows.Controls;
using System.Windows.Media;
using Eudora.Net.Data;

namespace Eudora.Net.GUI
{
    /// <summary>
    /// Editor / Viewer component for plain-text emails
    /// </summary>
    public partial class uc_Email_PlainText : UserControl
    {
        public EmailMessage Message { get; set; } = new EmailMessage();

        public uc_Email_PlainText()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void cb_FontFamily_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(Editor == null)
            {
                return;
            }
            Editor.FontFamily = cb_FontFamily.SelectedValue as FontFamily;
        }
    }
}
