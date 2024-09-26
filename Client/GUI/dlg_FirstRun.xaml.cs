using Eudora.Net.Core;
using Eudora.Net.Data;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.IO;
using sbux.wpf.Themer;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace Eudora.Net.GUI
{
    /// <summary>
    /// Interaction logic for dlg_FirstRun.xaml
    /// </summary>
    public partial class dlg_FirstRun : Window
    {
        private static readonly List<Uri> Pages =
        [
            new Uri("InitialOptions_Landing.xaml", UriKind.Relative),
            new Uri("InitialOptions_Security.xaml", UriKind.Relative),
            new Uri("InitialOptions_Theme.xaml", UriKind.Relative),
            new Uri("InitialOptions_Storage.xaml", UriKind.Relative),
            //new Uri("InitialOptions_Import.xaml", UriKind.Relative),
            new Uri("InitialOptions_Reporting.xaml", UriKind.Relative)
        ];


        public string DataStore
        {
            get { return (string)GetValue(DataStoreProperty); }
            set { SetValue(DataStoreProperty, value); }
        }

        public static readonly DependencyProperty DataStoreProperty =
            DependencyProperty.Register(
                "DataStore", 
                typeof(string), 
                typeof(dlg_FirstRun), 
                new PropertyMetadata(string.Empty, OnDataStoreChanged));

        private static void OnDataStoreChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is dlg_FirstRun dlg)
            {
                Properties.Settings.Default.DataStoreRoot = dlg.DataStore;
                Properties.Settings.Default.Save();
            }
        }


        public sbux.wpf.Themer.Theme CurrentTheme
        {
            get { return (sbux.wpf.Themer.Theme)GetValue(CurrentThemeProperty); }
            set { SetValue(CurrentThemeProperty, value); }
        }

        public static readonly DependencyProperty CurrentThemeProperty =
            DependencyProperty.Register(
                "CurrentTheme",
                typeof(sbux.wpf.Themer.Theme),
                typeof(dlg_FirstRun),
                new PropertyMetadata(ThemeManager.ActiveTheme));


        public bool EnableReporting
        {
            get { return (bool)GetValue(EnableReportingProperty); }
            set { SetValue(EnableReportingProperty, value); }
        }

        public static readonly DependencyProperty EnableReportingProperty =
            DependencyProperty.Register(
                "EnableReporting", 
                typeof(bool), 
                typeof(dlg_FirstRun),
                new PropertyMetadata(false, OnEnableReportingChanged));

        private static void OnEnableReportingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is dlg_FirstRun dlg)
            {
                Properties.Settings.Default.EnableErrorReporting = dlg.EnableReporting;
                Properties.Settings.Default.Save();
            }
        }

        private int CurrentPageIndex = 0;

        

        

        public dlg_FirstRun()
        {
            DataContext = this;
            InitializeComponent();

            DataStore = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    App.ApplicationName);

            ContentFrame.LoadCompleted += ContentFrame_LoadCompleted;
        }

        private void ContentFrame_LoadCompleted(object sender, NavigationEventArgs e)
        {
            if(e.Content is Page page)
            {
                page.DataContext = this;

                if(page is InitialOptions_Landing landing)
                {
                    btn_Back.IsEnabled = false;
                    btn_Back.Visibility = Visibility.Collapsed;
                }
                else
                {
                    btn_Back.IsEnabled = true;
                    btn_Back.Visibility = Visibility.Visible;
                }
            }
        }

        private void NavigateToPage(int index)
        {
            if (index < 0 || index >= Pages.Count)
            {
                return;
            }

            string navstring = $"pack://application:,,,/GUI/{Pages[CurrentPageIndex]}";
            ContentFrame.Navigate(new Uri(navstring));
        }

        private void btn_Next_Click(object sender, RoutedEventArgs e)
        {
            CurrentPageIndex++;
            if (CurrentPageIndex >= Pages.Count)
            {
                CurrentPageIndex = Pages.Count - 1;
                DialogResult = true;
                Close();
            }
            else
            {
                NavigateToPage(CurrentPageIndex);
            }
        }

        private void btn_Back_Click(object sender, RoutedEventArgs e)
        {
            CurrentPageIndex--;
            if (CurrentPageIndex < 0)
            {
                CurrentPageIndex = 0;
            }
            else
            {
                NavigateToPage(CurrentPageIndex);
            }
        }

        private void ContentFrame_Navigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            var ta = new ThicknessAnimation();
            ta.Duration = TimeSpan.FromSeconds(1.0);
            ta.DecelerationRatio = 0.7;
            ta.To = new Thickness(0, 0, 0, 0);
            if (e.NavigationMode == NavigationMode.New)
            {
                ta.From = new Thickness(500, 0, 0, 0);
            }
            else if (e.NavigationMode == NavigationMode.Back)
            {
                ta.From = new Thickness(0, 0, 500, 0);
            }
            (e.Content as Page)?.BeginAnimation(MarginProperty, ta);
        }
    }
}
