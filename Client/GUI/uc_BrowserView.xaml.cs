using Eudora.Net.Core;
using Eudora.Net.Data;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;



namespace Eudora.Net.GUI
{
    /// <summary>
    /// The browser window
    /// </summary>
    public partial class uc_BrowserView : ChildWindowBase
    {
        ///////////////////////////////////////////////////////////
        #region Properties
        /////////////////////////////

        public ExclusiveCollection SearchEngines { get; set; } = [];
        public BrowserBookmark ActiveEditBookmark { get; set; }
        

        /////////////////////////////
        #endregion Properties
        ///////////////////////////////////////////////////////////



        public uc_BrowserView()
        {
            SearchEngines.Add(new(new(), "Google", string.Empty));
            SearchEngines.Add(new(new(), "Bing", string.Empty));
            SearchEngines.Add(new(new(), "DuckDuckGo", string.Empty));

            InitializeComponent();
            Browser.EnsureCoreWebView2Async(Webview2Options.Environment);

            DataContext = this;
            datagrid.DataContext = BrowserData.Bookmarks;
            datagrid.SelectedIndex = -1;
        }

        public override void MdiActivated()
        {
            base.MdiActivated();
            if (Browser is null) return;
            Browser.Visibility = System.Windows.Visibility.Visible;
        }

        public override void MdiDeactivated()
        {
            base.MdiDeactivated();
            Browser.Visibility = System.Windows.Visibility.Hidden;
        }

        public void Navigate(string url)
        {
            try
            {
                Browser.CoreWebView2.Navigate(url);
            }
            catch(Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        private void BrowserView_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
        }

        // When this fires, the browser control is truly loaded and ready
        private void Browser_CoreWebView2InitializationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2InitializationCompletedEventArgs e)
        {
            // Bind bookmarks to the Bookmarks bar
            BookmarksBar.Browser = this;
            BookmarksBar.Toolbar.ItemsSource = BrowserData.Bookmarks.Data;

            // Bind browser events to be handled
            Browser.CoreWebView2.ProcessFailed += CoreWebView2_ProcessFailed;
            Browser.CoreWebView2.BasicAuthenticationRequested += CoreWebView2_BasicAuthenticationRequested;
            Browser.CoreWebView2.ClientCertificateRequested += CoreWebView2_ClientCertificateRequested;
            Browser.CoreWebView2.ContentLoading += CoreWebView2_ContentLoading;
            Browser.CoreWebView2.DownloadStarting += CoreWebView2_DownloadStarting;
            Browser.CoreWebView2.NavigationCompleted += CoreWebView2_NavigationCompleted;
            Browser.CoreWebView2.NavigationStarting += CoreWebView2_NavigationStarting;
            Browser.CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;
            Browser.CoreWebView2.PermissionRequested += CoreWebView2_PermissionRequested;
            Browser.CoreWebView2.ServerCertificateErrorDetected += CoreWebView2_ServerCertificateErrorDetected;
            Browser.CoreWebView2.StatusBarTextChanged += CoreWebView2_StatusBarTextChanged;           

            // Finally, enable the browser toolbar
            //BrowserToolbar.IsEnabled = true;
        }

        private void CoreWebView2_StatusBarTextChanged(object? sender, object e)
        {
            StatusBarText.Text = Browser.CoreWebView2.StatusBarText;
        }

        private void CoreWebView2_ServerCertificateErrorDetected(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2ServerCertificateErrorDetectedEventArgs e)
        {
            Debug.WriteLine("ServerCertificateErrorDetected");
        }

        private void CoreWebView2_PermissionRequested(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2PermissionRequestedEventArgs e)
        {
            Debug.WriteLine("PermissionRequested");
        }

        private void CoreWebView2_NewWindowRequested(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2NewWindowRequestedEventArgs e)
        {
            Debug.WriteLine("NewWindowRequested");
        }

        private void CoreWebView2_NavigationStarting(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs e)
        {
            Debug.WriteLine("NavigationStarting");
        }

        private void CoreWebView2_NavigationCompleted(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            Debug.WriteLine("NavigationCompleted");
        }

        private void CoreWebView2_DownloadStarting(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2DownloadStartingEventArgs e)
        {
            Debug.WriteLine("DownloadStarting");
        }

        private void CoreWebView2_ContentLoading(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2ContentLoadingEventArgs e)
        {
            Debug.WriteLine("ContentLoading");
        }

        private void CoreWebView2_ClientCertificateRequested(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2ClientCertificateRequestedEventArgs e)
        {
            Debug.WriteLine("ClientCertificateRequested");
        }

        private void CoreWebView2_BasicAuthenticationRequested(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2BasicAuthenticationRequestedEventArgs e)
        {
            Debug.WriteLine("BasicAuthenticationRequested");
        }

        private void CoreWebView2_ProcessFailed(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2ProcessFailedEventArgs e)
        {
            Debug.WriteLine("ProcessFailed");
        }

        private void tb_Address_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key is not Key.Enter) return;
            e.Handled = true;
            string result = PrepareNavigationString(tb_Address.Text);
            if (string.IsNullOrEmpty(result)) return;
            try
            {
                Browser.CoreWebView2.Navigate(result);
            }
            catch (ArgumentException ex)
            {
                Debug.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private bool ProtocolIsValid(string uri)
        {
            try
            {
                string[] tokens = uri.Split(" ");
                if (tokens.Count() < 2) return false;
                return Uri.CheckSchemeName(uri);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                return false;
            }
        }

        private bool SuffixIsValid(string uri)
        {
            string[] tokens = uri.Split(" ");
            if (tokens.Count() < 2) return false;
            return Uri.CheckHostName(uri) == UriHostNameType.Dns;
        }

        private string CreateSearchEngineQuery(string query)
        {
            string result = string.Empty;

            return result;
        }

        private string PrepareNavigationString(string rawText)
        {
            string uri = string.Empty;
            if (string.IsNullOrEmpty(rawText)) return uri;

            // 1. Clean extrneous whitespace
            uri = rawText.Trim();

            // 2. Determine if this is meant to be a partial URL or a search engine query
            bool hasProtocol = ProtocolIsValid(uri);
            bool hasHost = SuffixIsValid(uri);

            // If neither have been specified, we'll turn it into a query for the default search engine
            if (!hasProtocol && !hasHost)
            {
                BrowserSearchEngine? searchEngine = BrowserData.Instance.ActiveSearchEngine;

                uri = uri.Replace(" ", "+");
                uri = string.Format("{0}{1}",
                                    searchEngine?.SearchString,
                                    uri);
                return uri;
            }

            if (hasHost && !hasProtocol)
            {
                uri = string.Format("https://{0}", uri);
                return uri;
            }

            if (hasProtocol && !hasHost)
            {

            }

            return uri;
        }

        private void btn_Back_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Browser.GoBack();
        }

        private void btn_Forward_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Browser.GoForward();
        }

        private void btn_Refresh_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Browser.Reload();
        }

        private void btn_Bookmark_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var bookmark = new BrowserBookmark()
            {
                Url = Browser.Source.AbsoluteUri,
                DisplayString = Browser.CoreWebView2.DocumentTitle
            };
            BrowserData.Bookmarks.Add(bookmark);
        }

        private void btn_Settings_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            MainWindow.Instance?.ActivateOptionsTab(1);
        }

        private void btn_ManageBookmarks_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            Browser.Visibility = System.Windows.Visibility.Hidden;
            BookmarksManager.Visibility = System.Windows.Visibility.Visible;
        }

        private void btn_ManageBookmarks_Unchecked(object sender, System.Windows.RoutedEventArgs e)
        {
            Browser.Visibility = System.Windows.Visibility.Visible;
            BookmarksManager.Visibility = System.Windows.Visibility.Hidden;
        }

        private void btn_DeleteBookmark_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            e.Handled = true;
            var bookmark = ((FrameworkElement)sender).DataContext as BrowserBookmark;
            if(bookmark is not null)
            {
                BrowserData.Bookmarks.Remove(bookmark);
            }
        }

        private void datagrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if(datagrid.SelectedItem is not BrowserBookmark bookmark) return;
            ActiveEditBookmark = bookmark;
            tb_BMTitle.DataContext = bookmark;
            tb_BMURL.DataContext = bookmark;
        }
    }
}
