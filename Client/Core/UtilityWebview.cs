using Eudora.Net.Data;
using Microsoft.Web.WebView2.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eudora.Net.Core
{
    /// <summary>
    /// This is not rendered. It exists to conduct operations on HTML content
    /// that will be applied to a live view, using the browser's capabilities
    /// (Javascript and DOM).
    /// Example: This gives us a place to load Stationery and Signatures, as
    /// well as the means to extract the transferrable content, so it can be
    /// applied to an email message.
    /// </summary>
    public static class UtilityWebview
    {
        static WebView2 Webview;

        static UtilityWebview()
        {
            Webview = Webview2Allocator.Get();
            Webview.NavigationCompleted += Webview_NavigationCompleted;
        }

        private static void Webview_NavigationCompleted(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            
        }

        private static void LoadSignature(Signature signature)
        {
            string path = TempFileManager.CreateTempFileFromStringContent(signature.Content);
            Webview.CoreWebView2.Navigate(path);
        }

        private static void LoadStationery(Stationery stationery)
        {
            string path = TempFileManager.CreateTempFileFromStringContent(stationery.Content);
            Webview.CoreWebView2.Navigate(path);
        }
    }
}
