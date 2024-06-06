using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Eudora.Net.Data
{
    public class BrowserBookmark : INotifyPropertyChanged
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

        private string _Url = string.Empty;
        public string Url
        {
            get => _Url;
            set => SetField(ref _Url, value, nameof(Url));
        }

        private string _DisplayString = string.Empty;
        public string DisplayString
        {
            get => _DisplayString;
            set => SetField(ref _DisplayString, value, nameof(DisplayString));
        }

        private Uri _IconImage = new("pack://application:,,,/GUI/res/images/new/bookmark.png");
        public Uri IconImage
        {
            get => _IconImage;
            set => SetField(ref _IconImage, value, nameof(IconImage));
        }

        private string _Tooltip = string.Empty;
        public string Tooltip
        {
            get => $@"{DisplayString}{Environment.NewLine}{Url}";
            //set => SetField(ref _Tooltip, value, nameof(Tooltip));
        }

        /////////////////////////////
        #endregion Properties
        ///////////////////////////////////////////////////////////

        public BrowserBookmark()
        { }

        public BrowserBookmark(string url, string displayString, Uri iconImage)
        {
            Url = url;
            DisplayString = displayString;
            IconImage = iconImage;
        }
    }
}
