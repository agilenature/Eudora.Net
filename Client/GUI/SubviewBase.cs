using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace Eudora.Net.GUI
{
    public class SubviewBase : UserControl, INotifyPropertyChanged
    {
        ///////////////////////////////////////////////////////////
        #region INotifyPropertyChanged
        /////////////////////////////

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void SetField<TField>(ref TField field, TField value, string propertyName)
        {
            if (EqualityComparer<TField>.Default.Equals(field, value))
            {
                return;
            }

            field = value;
            OnPropertyChanged(propertyName);
        }

        /////////////////////////////
        #endregion INotifyPropertyChanged
        ///////////////////////////////////////////////////////////


        ///////////////////////////////////////////////////////////
        #region Dependency Properties
        /////////////////////////////

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(SubviewBase), new PropertyMetadata(string.Empty));

        /////////////////////////////
        #endregion Dependency Properties
        ///////////////////////////////////////////////////////////


        ///////////////////////////////////////////////////////////
        #region Operations
        /////////////////////////////

        public virtual void Activate()
        {

        }

        public virtual void Deactivate()
        {

        }

        /////////////////////////////
        #endregion Operations
        ///////////////////////////////////////////////////////////

    }
}
