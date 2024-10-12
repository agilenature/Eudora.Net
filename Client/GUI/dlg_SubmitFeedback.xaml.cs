using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Eudora.Net.GUI
{
    public class FeedbackValidation : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string? feedback = value as string;
            if (feedback is null) 
                return new ValidationResult(false, "Text cannot be empty");
            if(string.IsNullOrEmpty(feedback) || string.IsNullOrWhiteSpace(feedback)) 
                return new ValidationResult(false, "Text cannot be empty");

            return ValidationResult.ValidResult;
        }
    }


    /// <summary>
    /// Interaction logic for dlg_SubmitFeedback.xaml
    /// </summary>
    public partial class dlg_SubmitFeedback : Window
    {
        public static readonly DependencyProperty FeedbackProperty =
            DependencyProperty.Register(
                "Feedback",
                typeof(string),
                typeof(dlg_SubmitFeedback),
                new PropertyMetadata(string.Empty));

        public string Feedback
        {
            get { return (string)GetValue(FeedbackProperty); }
            set { SetValue(FeedbackProperty, value); }
        }

        



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
            var result = new FeedbackValidation().Validate(tb_Feedback.Text, CultureInfo.CurrentCulture);
            if (result == ValidationResult.ValidResult)
            {
                DialogResult = true;
                Close();
            }
            else
            {
                tb_Feedback.BorderBrush = new SolidColorBrush(Colors.Red);
            }
        }
    }
}
