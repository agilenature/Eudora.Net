using Eudora.Net.ExtensionMethods;
using Microsoft.Web.WebView2.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;


namespace Eudora.Net.Core
{
    /// <summary>
    /// The WebView2 control has a lengthy, multi-step creation and initialization process.
    /// Because this is true, one of two things must happen:
    /// 1. Every UX view that contains a webview must do a dance around the WV2 initialization
    /// 2. The app can have a supply of preallocated and preinitialized WV2 controls
    /// </summary>
    /// 

    internal class WebviewReadyEventArgs : EventArgs
    {
        private WebviewItem _Webview;
        public WebviewItem Webview
        {
            get => _Webview;
            set => _Webview = value;
        }

        public WebviewReadyEventArgs(WebviewItem item) 
        {
            _Webview = item;
        }
    }

    internal class WebviewItem
    {
        private Window Container = new();
        private Grid ContainerGrid = new() { Name = "ContainerGrid" };
        private static readonly Uri InitialContent = new("about:blank");
        public WebView2 Webview = new();

        public event EventHandler? WebviewReady;
        private void RaiseWebviewReadyEvent()
        {
            WebviewReady?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// The WebView2 must have a visible parent control in order for initialization to occur
        /// </summary>
        public WebviewItem()
        {
            Container.MaxWidth = 1;
            Container.MaxHeight = 1;
            Container.ResizeMode = ResizeMode.NoResize;
            Container.WindowStyle = WindowStyle.None;
            Container.ShowInTaskbar = false;
            Container.Content = ContainerGrid;
            ContainerGrid.Children.Add(Webview);
            Container.Show();

            // Hook up the events we need and begin the initialization
            Webview.CoreWebView2InitializationCompleted += Webview_CoreWebView2InitializationCompleted;
            Webview.NavigationCompleted += Webview_NavigationCompleted;
            Webview.EnsureCoreWebView2Async(Webview2Options.Environment);
        }

        /// <summary>
        /// Init step 1 complete. Now navigate to some content to complete the process
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Webview_CoreWebView2InitializationCompleted(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2InitializationCompletedEventArgs e)
        {
            try
            {
                Webview.CoreWebView2.Navigate(InitialContent.OriginalString);
            }
            catch(Exception ex)
            {
                FaultReporter.Error(ex);
            }
        }

        /// <summary>
        /// Now that the WV2 is truly loaded and ready, the webview can be divorced from it
        /// and the container window can be closed. This WebView2 is now ready to be allocated
        /// from the queue and used right away, without trodding on a minefield of exceptions.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Webview_NavigationCompleted(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            ContainerGrid.Children.Remove(Webview);
            Container.Close();
            Webview.NavigationCompleted -= Webview_NavigationCompleted;
            Webview.CoreWebView2InitializationCompleted -= Webview_CoreWebView2InitializationCompleted;
            RaiseWebviewReadyEvent();
        }
    }


    internal static class Webview2Allocator
    {
        private static readonly uint PreallocationCount = 5;
        private static readonly int AllocationThreshold = 2;
        private static Queue<WebviewItem> Webviews = [];
        private static List<WebviewItem> UnreadyWebviews = [];
        private static object Locker = new();

        /// <summary>
        /// This could be done in a static constructor but the initial
        /// allocation has to come early, so we need determinstic control
        /// of when this happens.
        /// </summary>
        public static async Task Startup()
        {
            for (uint i = 0; i < PreallocationCount; i++)
            {
                AllocateNewWebview();
            }
        }

        static void AllocateNewWebview()
        {
            lock (Locker)
            {
                var wv = new WebviewItem();
                wv.WebviewReady += Wv_WebviewReady;
                UnreadyWebviews.Add(wv);
            }
        }

        private static void Wv_WebviewReady(object? sender, EventArgs e)
        {
            lock (Locker)
            {
                if (sender is WebviewItem item)
                {
                    UnreadyWebviews.Remove(item);
                    Webviews.Enqueue(item);
                }
            }
        }

        public static WebView2 Get()
        {
            lock (Locker)
            {
                var wv2 = Webviews.Dequeue();
                if (Webviews.Count <= AllocationThreshold)
                {
                    AllocateNewWebview();
                }
                return wv2.Webview;
            }
        }
    }
}
