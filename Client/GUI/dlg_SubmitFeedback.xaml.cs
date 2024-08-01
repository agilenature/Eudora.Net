using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Eudora.Net.GUI
{
    /// <summary>
    /// Interaction logic for dlg_SubmitFeedback.xaml
    /// </summary>
    public partial class dlg_SubmitFeedback : Window
    {


        public string Feedback
        {
            get { return (string)GetValue(FeedbackProperty); }
            set { SetValue(FeedbackProperty, value); }
        }

        public static readonly DependencyProperty FeedbackProperty =
            DependencyProperty.Register(
                "Feedback", 
                typeof(string), 
                typeof(dlg_SubmitFeedback), 
                new PropertyMetadata(string.Empty));



        public dlg_SubmitFeedback()
        {
            Owner = Application.Current.MainWindow;
            InitializeComponent();
            DataContext = this;
            tb_Feedback.Focus();
        }

        private void btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void btn_OK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
