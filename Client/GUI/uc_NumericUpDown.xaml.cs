using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace Eudora.Net.GUI
{
    /// <summary>
    /// Because for some reason, this useful control type still doesn't exist in the WPF library.
    /// </summary>

    public partial class uc_NumericUpDown : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        ///////////////////////////////////////////////////////////
        #region Construction
        /////////////////////////////

        public uc_NumericUpDown()
        {
            InitializeComponent();
            DataContext = this;
        }

        /////////////////////////////
        #endregion Construction
        ///////////////////////////////////////////////////////////



        ///////////////////////////////////////////////////////////
        #region Properties
        /////////////////////////////

        public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(
            name: "Value",
            propertyType: typeof(Double),
            ownerType: typeof(uc_NumericUpDown),
            typeMetadata: new FrameworkPropertyMetadata(defaultValue: 1.0));

        public Double Value
        {
            get => (Double)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        private Double _value_min = 1.0;
        public Double Min
        {
            get { return _value_min; }
            set
            {
                _value_min = value;
                OnPropertyChanged(nameof(Min));
            }
        }

        private Double _value_max = 999.9;
        public Double Max
        {
            get { return _value_max; }
            set
            {
                _value_max = value;
                OnPropertyChanged(nameof(Max));
            }
        }

        /////////////////////////////
        #endregion Properties
        ///////////////////////////////////////////////////////////
        
        
        
        ///////////////////////////////////////////////////////////
        #region Control Logic
        /////////////////////////////

        private Task Increment()
        {
            Double value = Value + 1.0;
            if (value > _value_max)
            {
                value = _value_max;
            }
            Value = value;
            return Task.FromResult(value);
        }

        private Task Decrement()
        {
            Double value = Value - 1.0;
            if (value < _value_min)
            {
                value = _value_min;
            }
            Value = value;
            return Task.FromResult(value);
        }

        private async void btn_Up_Click(object sender, RoutedEventArgs e)
        {
            await Increment();
        }

        private async void btn_Down_Click(object sender, RoutedEventArgs e)
        {
            await Decrement();
        }

        /////////////////////////////
        #endregion Control Logic
        ///////////////////////////////////////////////////////////
    }
}
