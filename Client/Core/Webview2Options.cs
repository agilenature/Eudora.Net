using Microsoft.Web.WebView2.Core;
using System.IO;

namespace Eudora.Net.Core
{
    /// <summary>
    /// This prevents the various Chromium/Edge webviews in the app
    /// from using the system UserData folders as the datastore root.
    /// </summary>
    internal static class Webview2Options
    {
        public static CoreWebView2EnvironmentOptions? Options;
        public static CoreWebView2Environment? Environment;
        static string DataRoot = string.Empty;

        /// <summary>
        /// For determinstic control of when this occurs.
        /// A static constructor is sometimes the best way, but not here.
        /// Because the app has a queue of preallocated and initialized WV2,
        /// the creation of the WV2 environment has to come first, and early
        /// </summary>
        public static void Startup()
        {
            DataRoot = Path.Combine(
                Eudora.Net.Properties.Settings.Default.DataStoreRoot,
                @"WV2");
            IoUtil.EnsureFilePath(DataRoot);

            Options = new()
            {
                EnableTrackingPrevention = false
            };

            Environment = Init().Result;
        }

        static async Task<CoreWebView2Environment> Init()
        {
            return await CoreWebView2Environment.CreateAsync(null, DataRoot, Options);
        }
        
    }
}
