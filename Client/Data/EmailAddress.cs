using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;


namespace Eudora.Net.Data
{
    public class EmailAddress : INotifyPropertyChanged, IEquatable<EmailAddress>, IComparable<EmailAddress>
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
        #region IEquatable
        /////////////////////////////

        public bool Equals(EmailAddress? other)
        {
            if (other is null) return false;
            return (Name == other._Name && _Address == other._Address);
        }

        /////////////////////////////
        #endregion IEquatable
        ///////////////////////////////////////////////////////////



        ///////////////////////////////////////////////////////////
        #region IComparable
        /////////////////////////////

        public int CompareTo(EmailAddress? other)
        {
            if(other is null) return 1;
            return Address.CompareTo(other._Address);
        }

        /////////////////////////////
        #endregion IComparable
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

        private string _Address = string.Empty;
        public string Address
        {
            get => _Address;
            set => SetField(ref _Address, value, nameof(Address));
        }

        [JsonIgnore]public string DisplayString
        {
            get => String.Format("{0} ({1})", _Name, _Address);
        }

        [JsonIgnore]public bool Empty
        {
            get => (String.IsNullOrEmpty(_Name) && String.IsNullOrEmpty(_Address));
        }

        /////////////////////////////
        #endregion Properties
        ///////////////////////////////////////////////////////////


        public EmailAddress()
        {

        }

        public EmailAddress(string name, string address)
        {
            _Name = name;
            _Address = address;
        }

        public static EmailAddress ParseFromDisplayString(string display)
        {
            EmailAddress address = new();

            string working = display.Replace("<", ";");
            working = working.Replace(">", "");
            var tokens = working.Split(';');
            if(tokens.Length == 2)
            {
                address.Name = tokens[0].Trim();
                address.Address = tokens[1].Trim();
            }

            return address;
        }
    }
}
