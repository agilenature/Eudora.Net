using Eudora.Net.Core;
using System.Windows;
using System.Windows.Controls;

namespace Eudora.Net.GUI
{
    /// <summary>
    /// Interaction logic for uc_BrowserBookmarksBar.xaml
    /// </summary>
    public partial class uc_BrowserBookmarksBar : UserControl
    {
        public uc_BrowserView Browser
        {
            get { return (uc_BrowserView)GetValue(BrowserProperty); }
            set { SetValue(BrowserProperty, value); }
        }

        public static readonly DependencyProperty BrowserProperty =
            DependencyProperty.Register("Browser", typeof(uc_BrowserView), typeof(uc_BrowserBookmarksBar), new PropertyMetadata(null));


        public uc_BrowserBookmarksBar()
        {
            InitializeComponent();            
        }

        private void BookmarkButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is BookmarkButton button && Browser is not null)
                {
                    Browser.Navigate(button.Bookmark);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }
    }
}
