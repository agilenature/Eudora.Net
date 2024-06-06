using Eudora.Net.Core;
using Eudora.Net.Data;
using Microsoft.Web.WebView2.Wpf;
using System.Windows;
using System.Windows.Controls;

namespace Eudora.Net.GUI
{
    /// <summary>
    /// Interaction logic for uc_MessagePreview.xaml
    /// </summary>
    public partial class uc_MessagePreview : SubviewBase
    {
        private WebView2 Webview;

        public uc_MessagePreview()
        {
            InitializeComponent();
            Webview = Webview2Allocator.Get();
            MainGrid.Children.Add(Webview);
            Grid.SetRow(Webview, 1);
            DataContextChanged += Uc_MessagePreview_DataContextChanged;
        }

        public override void Activate()
        {
            base.Activate();
            Visibility = Visibility.Visible;
        }

        public override void Deactivate()
        {
            base.Deactivate();
            Visibility = Visibility.Hidden;
        }

        private void Uc_MessagePreview_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var message = DataContext as EmailMessage;
            if (message is not null)
            {
                AttachmentsBar.DataContext = message.Attachments;

                try
                {
                    string tempPath = TempFileManager.CreateTempFileFromStringContent(message.Body);
                    if (string.IsNullOrEmpty(tempPath))
                    {
                        Logger.NewEvent(LogEvent.EventCategory.Warning, "Failed to create temp file from message body");
                        return;
                    }
                    Webview.CoreWebView2.Navigate(tempPath);
                }
                catch (Exception ex)
                {
                    Logger.LogException(ex);
                }
            }
            else
            {
                Webview.CoreWebView2.Navigate("about:blank");
            }
        }

        public void Print()
        {
            Webview.CoreWebView2.ShowPrintUI(Microsoft.Web.WebView2.Core.CoreWebView2PrintDialogKind.System);
        }
    }
}
