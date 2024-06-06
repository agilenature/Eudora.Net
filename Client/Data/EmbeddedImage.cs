using Eudora.Net.Core;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Text;

namespace Eudora.Net.Data
{
    public class EmbeddedImage : INotifyPropertyChanged
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
        #region Properties
        /////////////////////////////

        private string _Source = string.Empty;
        public string Source
        {
            get => _Source;
            set => SetField(ref _Source, value, nameof(Source));
        }

        private string _HTMLSource = string.Empty;
        public string HTMLSource
        {
            get => _HTMLSource;
            set => SetField(ref _HTMLSource, value, nameof(HTMLSource));
        }

        private string _CIDSource = string.Empty;
        public string CIDSource
        {
            get => _CIDSource;
            set => SetField(ref _CIDSource, value, nameof(CIDSource));
        }

        private string _Alt = string.Empty;
        public string Alt
        {
            get => _Alt;
            set => SetField(ref _Alt, value, nameof(Alt));
        }        

        private string _Base64String = string.Empty;
        public string Base64String
        {
            get => _Base64String;
            set => SetField(ref _Base64String, value, nameof(Base64String));
        }

        private byte[] _Base64Bytes = [];
        public byte[] Base64Bytes
        {
            get => _Base64Bytes;
            set => SetField(ref _Base64Bytes, value, nameof(Base64Bytes));
        }

        /////////////////////////////
        #endregion Properties
        ///////////////////////////////////////////////////////////

        public EmbeddedImage()
        {
        }
    }
}
