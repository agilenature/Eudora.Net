using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Eudora.Net.Core
{
    public class BrowserSearchEngine : INotifyPropertyChanged
    {
        ///////////////////////////////////////////////////////////
        #region INotifyPropertyChanged
        /////////////////////////////

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SetField<TField>(ref TField field, TField value, string propertyName)
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
        #region Properties
        /////////////////////////////

        private string _DisplayString = string.Empty;
        public string DisplayString
        {
            get => _DisplayString;
            set => SetField(ref _DisplayString, value, nameof(DisplayString));
        }

        private string _SearchString = string.Empty;
        public string SearchString
        {
            get => _SearchString;
            set => SetField(ref _SearchString, value, nameof(SearchString));
        }

        private string _ImagePath = string.Empty;
        public string ImagePath
        {
            get => _ImagePath;
            set => SetField(ref _ImagePath, value, nameof(ImagePath));
        }

        /////////////////////////////
        #endregion Properties
        ///////////////////////////////////////////////////////////
        
        public BrowserSearchEngine()
        { }

        public BrowserSearchEngine(string displayString, string searchString, string imagePath)
        {
            _DisplayString = displayString;
            _SearchString = searchString;
            _ImagePath = imagePath;
        }
    }
}
