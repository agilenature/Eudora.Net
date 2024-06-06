using Eudora.Net.Core;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace Eudora.Net.GUI
{
    /// <summary>
    /// Interaction logic for uc_EventLogger.xaml
    /// </summary>
    public partial class uc_EventLogger : UserControl
    {
        public uc_EventLogger()
        {
            InitializeComponent();
            Display.DataContext = Logger.Events;
            Display.ItemsSource = Logger.Events;
            ((INotifyCollectionChanged)Display.Items).CollectionChanged += Uc_EventLogger_CollectionChanged;
            Loaded += Uc_EventLogger_Loaded;
        }

        private void Uc_EventLogger_Loaded(object sender, RoutedEventArgs e)
        {
            if(Display.Items.Count == 0)
            {
                return;
            }
            Display.ScrollIntoView(Display.Items.GetItemAt(Display.Items.Count - 1));
        }

        private void Uc_EventLogger_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if(e.Action == NotifyCollectionChangedAction.Add)
            {
                Display.ScrollIntoView(Display.Items.GetItemAt(Display.Items.Count-1));
            }
        }

        private void btn_EventLog_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance?.ShowEventLog(false);
        }

        private void btn_Copy_Click(object sender, RoutedEventArgs e)
        {
            Logger.DumpToFile();
        }
    }
}
