using SQLite;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Eudora.Net.Data
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
        #region Fields
        /////////////////////////////

        private Guid _Id = Guid.NewGuid();
        private string _Name = string.Empty;
        private string _SearchString = string.Empty;
        private string _ImagePath = string.Empty;

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

        public string Name
        {
            get => _Name;
            set => SetField(ref _Name, value, nameof(Name));
        }
        
        public string SearchString
        {
            get => _SearchString;
            set => SetField(ref _SearchString, value, nameof(SearchString));
        }
        
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

        public BrowserSearchEngine(string name, string searchString, string imagePath)
        {
            Name = name;
            SearchString = searchString;
            ImagePath = imagePath;
        }
    }
}
