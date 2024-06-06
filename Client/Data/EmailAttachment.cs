using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Eudora.Net.Data
{
    public class EmailAttachment : INotifyPropertyChanged
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

        private string _Name = string.Empty;
        public string Name
        {
            get => _Name;
            set => SetField(ref _Name, value, nameof(Name));
        }

        private string _Path = string.Empty;
        public string Path
        {
            get => _Path;
            set => SetField(ref _Path, value, nameof(Path));
        }

        private object? _Content = null;
        public object? Content
        {
            get => _Content;
            set => SetField(ref _Content, value, nameof(Content));
        }

        /////////////////////////////
        #endregion Properties
        ///////////////////////////////////////////////////////////
        

        public EmailAttachment()
        {

        }

        public EmailAttachment(string name, string path)
        {
            Name = name;
            Path = path;
        }

        public EmailAttachment(string name, object? content, string? path)
        {
            Name = name;
            Content = content;
            if(path is not null)
            {
                Path = path;
            }
        }
    }
}
