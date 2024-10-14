using Eudora.Net.Core;
using Microsoft.Web.WebView2.Wpf;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Eudora.Net.GUI
{
    /// <summary>
    /// Interaction logic for HelpViewer.xaml
    /// </summary>
    public partial class HelpViewer : Window
    {
        ///////////////////////////////////////////////////////////
        #region Fields

        private readonly string uri_Introduction = "Eudora.Net.Documents.HelpOverview.pdf";

        #endregion Fields
        ///////////////////////////////////////////////////////////

        ///////////////////////////////////////////////////////////
        #region Properties

        public WebView2 Webview { get; private set; }

        #endregion Properties
        ///////////////////////////////////////////////////////////


        ///////////////////////////////////////////////////////////
        #region Interface

        public HelpViewer()
        {
            MainWindow.Instance?.EnableHelpMenu(false);
            InitializeComponent();
            Loaded += HelpViewer_Loaded;
        }

        public void LoadPage(string resourceName)
        {
            //string content = IoUtil.LoadResourceString(resourceName);
            Stream? content = IoUtil.LoadResourceStream(resourceName); 
            string path = TempFileManager.CreatePdfFromStream(content);
            Webview.CoreWebView2.Navigate(path);
        }

        #endregion Interface
        ///////////////////////////////////////////////////////////


        ///////////////////////////////////////////////////////////
        #region Internal

        private void HelpViewer_Loaded(object sender, RoutedEventArgs e)
        {
            var wv = Webview2Allocator.Get();
            if (wv is not null)
            {
                Webview = wv;
                ContentPane.Children.Add(Webview);
            }
        }

        private void btn_toc_introduction_Click(object sender, RoutedEventArgs e)
        {
            LoadPage(uri_Introduction);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MainWindow.Instance?.EnableHelpMenu(true);
        }

        #endregion Internal
        ///////////////////////////////////////////////////////////

    }
}
