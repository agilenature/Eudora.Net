using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Eudora.Net.Data
{
    internal class HtmlBody : INotifyPropertyChanged
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

        private StyleAttribute _Style = new();
        private string _InnerHTML = string.Empty;
        private string _OuterHTML = string.Empty;

        /////////////////////////////
        #endregion Fields
        ///////////////////////////////////////////////////////////


        ///////////////////////////////////////////////////////////
        #region Properties
        /////////////////////////////

        public StyleAttribute Style
        {
            get => _Style;
            set => SetField(ref _Style, value, nameof(Style));
        }

        public string InnerHTML
        {
            get => _InnerHTML;
            set => SetField(ref _InnerHTML, value, nameof(InnerHTML));
        }

        public string OuterHTML
        {
            get => _OuterHTML;
            set
            {
                bool changed = SetField(ref _OuterHTML, value, nameof(OuterHTML));
                if (changed)
                {
                    UpdateStyle();
                }
            }
        }

        /////////////////////////////
        #endregion Properties
        ///////////////////////////////////////////////////////////
        
        public HtmlBody()
        {
            _Style.PropertyChanged += _Style_PropertyChanged;
        }

        private void _Style_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(Style));
        }

        private void UpdateStyle()
        {
            const string startToken = "style=\"";
            int startPos = _OuterHTML.IndexOf(startToken);
            string sub1 = _OuterHTML.Substring(startPos);
            int endPos = _OuterHTML.IndexOf("\"", startPos + startToken.Length);

            string styleAttribute = _OuterHTML.Substring(startPos, endPos - startPos);
            Style.Value = styleAttribute;
        }
    }
}
