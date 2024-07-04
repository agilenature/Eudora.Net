using Eudora.Net.Core;
using Eudora.Net.Data;
using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using WpfThemer;

namespace Eudora.Net
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        ///////////////////////////////////////////////////////////
        #region Properties
        /////////////////////////////

        public string DatastoreRoot { get; private set; } = string.Empty;
        public string ApplicationName { get; } = "Eudora.Net";

        /////////////////////////////
        #endregion Properties
        ///////////////////////////////////////////////////////////

        public App() : base()
        {
            ThemeManager.SetApplication(this);
            ThemeManager.SetTheme(Eudora.Net.Properties.Settings.Default.UxTheme);
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                tb.SelectAll();
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            // By default, all textboxes behave as "select all" when you click in them
            EventManager.RegisterClassHandler(typeof(TextBox), TextBox.GotMouseCaptureEvent, new RoutedEventHandler(TextBox_GotFocus));
            EventManager.RegisterClassHandler(typeof(TextBox), TextBox.MouseDoubleClickEvent, new RoutedEventHandler(TextBox_GotFocus));

            base.OnStartup(e);

            FrameworkElement.StyleProperty.OverrideMetadata(typeof(Window), new FrameworkPropertyMetadata
            {
                DefaultValue = FindResource(typeof(Window))
            });

            DatastoreRoot = Eudora.Net.Properties.Settings.Default.DataStoreRoot;

            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            if (!Eudora.Net.Properties.Settings.Default.FirstRunOptionsComplete ||
                string.IsNullOrEmpty(DatastoreRoot) ||
                string.IsNullOrWhiteSpace(DatastoreRoot))
            {
                // Default choices; user can override
                string defaultStorage = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    ApplicationName);
                Eudora.Net.Properties.Settings.Default.DataStoreRoot = defaultStorage;
                DatastoreRoot = defaultStorage;
                var dlg = new Eudora.Net.GUI.dlg_FirstRun(defaultStorage);
                var result = dlg.ShowDialog();
                if (result is null || result == false)
                {
                    Application.Current.Shutdown();
                    return;
                }
                DatastoreRoot = dlg.DataRoot;
                Eudora.Net.Properties.Settings.Default.DataStoreRoot = dlg.DataRoot;
                Eudora.Net.Properties.Settings.Default.Save();
            }

            // Get the WebView2 cache going
            InitWebviewOptionsAndQueue();

            // The rest of early startup & init
            SignatureManager.Startup();
            StationeryManager.Startup();
            LabelManager.Startup();
            PersonalityManager.Startup();
            AddressBookManager.Startup();
            PostOffice.Instance.Startup();
            EmailSearchEngine.Startup();
            BrowserSettings.Instance.Startup();
            EudoraStatistics.Startup();

            GUI.MainWindow.Instance = new GUI.MainWindow();
            ShutdownMode = ShutdownMode.OnLastWindowClose;
            MainWindow = GUI.MainWindow.Instance;
            MainWindow.Show();
        }

        private async void InitWebviewOptionsAndQueue()
        {
            Webview2Options.Startup();
            await Webview2Allocator.Startup();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            Eudora.Net.Properties.Settings.Default.Save();
            PostOffice.Instance.Shutdown();
            //IconCache.Save();
            BrowserSettings.Instance.Shutdown();
            EudoraStatistics.Shutdown();
            TempFileManager.Shutdown();
        }
    }

}
