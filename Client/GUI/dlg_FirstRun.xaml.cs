using Eudora.Net.Core;
using Eudora.Net.Data;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.IO;
using WpfThemer;
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


        public WpfThemer.Theme CurrentTheme
        {
            get { return (WpfThemer.Theme)GetValue(CurrentThemeProperty); }
            set { SetValue(CurrentThemeProperty, value); }
        }

        public static readonly DependencyProperty CurrentThemeProperty =
            DependencyProperty.Register(
                "CurrentTheme",
                typeof(WpfThemer.Theme),
                typeof(dlg_FirstRun),
                new PropertyMetadata(ThemeManager.ActiveTheme));


        public bool AllowReporting
        {
            get { return (bool)GetValue(AllowReportingProperty); }
            set { SetValue(AllowReportingProperty, value); }
        }

        public static readonly DependencyProperty AllowReportingProperty =
            DependencyProperty.Register(
                "AllowReporting", 
                typeof(bool), 
                typeof(dlg_FirstRun),
                new PropertyMetadata(false));

        private int CurrentPageIndex = 0;

        private static void OnDataStoreChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is dlg_FirstRun dlg)
            {
                Properties.Settings.Default.DataStoreRoot = dlg.DataStore;
                Properties.Settings.Default.Save();
            }
        }

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
