using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Eudora.Net.Data
{
    internal class HtmlElement : INotifyPropertyChanged
    {
        ///////////////////////////////////////////////////////////
        #region INotifyPropertyChanged
        /////////////////////////////

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // A one-shot property setter which only changes the field and invokes ProperyChanged
        // if the new value is actually new.
        // Returns true if the backing field changes, false otherwise.
        protected bool SetField<TField>(ref TField field, TField value, string propertyName)
        {
            if (EqualityComparer<TField>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        /////////////////////////////
        #endregion INotifyPropertyChanged
        ///////////////////////////////////////////////////////////


        ///////////////////////////////////////////////////////////
        #region Fields
        /////////////////////////////

        private string _Tag = string.Empty;
        private string _Id = string.Empty;
        private StyleAttribute _Style = new();
        private string _InnerHTML = string.Empty;
        private string _OuterHTML = string.Empty;

        /////////////////////////////
        #endregion Fields
        ///////////////////////////////////////////////////////////


        ///////////////////////////////////////////////////////////
        #region Properties
        /////////////////////////////

        public string Tag
        {
            get => _Tag;
            set => _Tag = value;
        }

        public string Id
        {
            get => _Id;
            set => _Id = value;
        }

        public StyleAttribute Style
        {
            get => _Style;
            set => _Style = value;
        }

        public string InnerHTML
        {
            get => _InnerHTML;
            set => _InnerHTML = value;
        }

        public string OuterHTML
        {
            get => _OuterHTML;
            set => _OuterHTML = value;
        }

        /////////////////////////////
        #endregion Properties
        ///////////////////////////////////////////////////////////

    }
}
