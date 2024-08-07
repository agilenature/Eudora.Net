using Eudora.Net.Core;
using SQLite;
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
        #region Fields
        /////////////////////////////

        private Guid _Id = Guid.NewGuid();
        private string _Url = string.Empty;
        private string _DisplayString = string.Empty;
        private Uri _IconImage = new("pack://application:,,,/GUI/res/images/new/bookmark.png");
        private string _Tooltip = string.Empty;

        /////////////////////////////
        #endregion Fields
        ///////////////////////////////////////////////////////////


        ///////////////////////////////////////////////////////////
        #region Properties
        /////////////////////////////

        [SQLite.PrimaryKey]
        public Guid Id
        {
            get => _Id;
            set => SetField(ref _Id, value, nameof(Id));
        }
        
        public string Url
        {
            get => _Url;
            set => SetField(ref _Url, value, nameof(Url));
        }
        
        public string DisplayString
        {
            get => _DisplayString;
            set => SetField(ref _DisplayString, value, nameof(DisplayString));
        }
        
        public Uri IconImage
        {
            get => _IconImage;
            set => SetField(ref _IconImage, value, nameof(IconImage));
        }
        
        public string Tooltip
        {
            get => $@"{DisplayString}{Environment.NewLine}{Url}";
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
