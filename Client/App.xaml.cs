using Eudora.Net.Core;
using Eudora.Net.Data;
using Eudora.Net.GUI.theme;
using System.Windows;
using System.Windows.Controls;
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

        public static string ApplicationName { get; } = "Eudora.Net";

        /////////////////////////////
        #endregion Properties
        ///////////////////////////////////////////////////////////

        public App() : base()
        {
            
        }

        /// <summary>
        /// Gives all textboxes the behavior of "select all" when clicked into
        /// </summary>
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                tb.SelectAll();
            }
}
        private void InstallTextboxBehaviors()
        {
            EventManager.RegisterClassHandler(typeof(TextBox), TextBox.GotMouseCaptureEvent, new RoutedEventHandler(TextBox_GotFocus));
            EventManager.RegisterClassHandler(typeof(TextBox), TextBox.MouseDoubleClickEvent, new RoutedEventHandler(TextBox_GotFocus));
        }

        /// <summary>
        /// Initialize the WpfThemer lib, and install the Eudora theme to complement the default themes
        /// </summary>
        private void InstallThemes()
        {
            try
            {
                // The application's library of control symbols for light & dark themes
                SymboLib.Build();

                ThemeManager.SetApplication(this);
                
                // The Eudora theme
                string uri = "/Eudora.Net;component/GUI/theme/ThemeEudora.xaml";
                var theme = new WpfThemer.Theme(WpfThemer.Theme.eThemeType.Light, "Eudora Classic", "Eudora Classic", new ResourceDictionary()
                {
                    Source = new Uri(uri, UriKind.RelativeOrAbsolute)
                });
                ThemeManager.AddExternalTheme(theme);
            }
            catch(Exception ex)
            {
                Logger.LogException(ex);
            }

            try
            {
                ThemeManager.SetTheme(Eudora.Net.Properties.Settings.Default.UxTheme);

                // Set the default styles for all windows, user controls, etc.
                FrameworkElement.StyleProperty.OverrideMetadata(typeof(Window), new FrameworkPropertyMetadata
                {
                    DefaultValue = FindResource(typeof(Window))
                });
                FrameworkElement.StyleProperty.OverrideMetadata(typeof(GUI.ChildWindowBase), new FrameworkPropertyMetadata
                {
                    DefaultValue = FindResource(typeof(GUI.ChildWindowBase))
                });
                FrameworkElement.StyleProperty.OverrideMetadata(typeof(GUI.uc_TabBase), new FrameworkPropertyMetadata
                {
                    DefaultValue = FindResource(typeof(GUI.uc_TabBase))
                });
                FrameworkElement.StyleProperty.OverrideMetadata(typeof(UserControl), new FrameworkPropertyMetadata
                {
                    DefaultValue = FindResource(typeof(UserControl))
                });
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        private void InstallStyles()
        {
            // Unnecessary: MainWindow references CommonUX
            //try
            //{
            //    App.Current.Resources.MergedDictionaries.Add(new ResourceDictionary()
            //    {
            //        Source = new Uri("/Eudora.Net;component/GUI/theme/CommonUX.xaml", UriKind.RelativeOrAbsolute)
            //    });
            //}
            //catch (Exception ex)
            //{
            //    Logger.LogException(ex);
            //}
        }

        /// <summary>
        /// On first run, show the user the welcome & initial options dialog
        /// Save the user's choices to the settings
        /// </summary>
        private bool HandleFirstRun()
        {
            if (!Eudora.Net.Properties.Settings.Default.FirstRunOptionsComplete)
            {
                var dlg = new Eudora.Net.GUI.dlg_FirstRun();
                var result = dlg.ShowDialog();
                if (result is null || result == false)
                {
                    return false;
                }

                Eudora.Net.Properties.Settings.Default.FirstRunOptionsComplete = true;
                Eudora.Net.Properties.Settings.Default.Save();
            }

            return true;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            InstallTextboxBehaviors();

            base.OnStartup(e);
            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            InstallThemes();
            InstallStyles();

            // Inital options upon first run
            if(HandleFirstRun() == false)
            {
                Shutdown();
                return;
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
            BrowserData.Instance.Startup();
            EudoraStatistics.Startup();

            // Load the main window
            GUI.MainWindow.Instance = new();
            MainWindow = GUI.MainWindow.Instance;
            MainWindow.Show();

            // Set the theme
            ThemeManager.SetTheme(Eudora.Net.Properties.Settings.Default.UxTheme);
            
            // Change shutdown mode to close when the main window closes
            ShutdownMode = ShutdownMode.OnMainWindowClose;
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
            BrowserData.Instance.Shutdown();
            EudoraStatistics.Shutdown();
            TempFileManager.Shutdown();
        }
    }
}
