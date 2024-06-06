using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace Eudora.Net.Data
{
    /// <summary>
    /// Categorized to align with the UX
    /// </summary>
    public class Contact : INotifyPropertyChanged
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

        [JsonIgnore]
        public static readonly string extension = ".con";

        [JsonIgnore]
        public AddressBook? AddressBook
        {
            get { return AddressBookManager.Get(AddressBookName); }
        }

        private string _AddressBookName = string.Empty;
        public string AddressBookName
        {
            get => _AddressBookName;
            set => SetField(ref _AddressBookName, value, nameof(AddressBookName));
        }

        private string _Name = "NameOfContact";
        public string Name
        {
            get => _Name;
            set => SetField(ref _Name, value, nameof(Name)); 
        }

        ///////////////////////////////////////////////////////////
        #region PERSONAL page
        /////////////////////////////

        private string _FirstName = string.Empty;
        public string FirstName
        {
            get => _FirstName;
            set => SetField(ref _FirstName, value, nameof(FirstName));
        }

        private string _LastName = string.Empty;
        public string LastName
        {
            get => _LastName;
            set => SetField(ref _LastName, value, nameof(LastName));
        }

        private string _EmailAddress = string.Empty;
        public string EmailAddress
        {
            get => _EmailAddress;
            set => SetField(ref _EmailAddress, value, nameof(EmailAddress));
        }

        /////////////////////////////
        #endregion PERSONAL page
        ///////////////////////////////////////////////////////////


        ///////////////////////////////////////////////////////////
        #region HOME page
        /////////////////////////////

        private string _StreetAddress_Home = string.Empty;
        public string StreetAddress_Home
        {
            get => _StreetAddress_Home;
            set => SetField(ref _StreetAddress_Home, value, nameof(StreetAddress_Home));
        }

        private string _City_Home = string.Empty;
        public string City_Home
        {
            get => _City_Home;
            set => SetField(ref _City_Home, value, nameof(City_Home));
        }

        private string _State_Home = string.Empty;
        public string State_Home
        {
            get => _State_Home;
            set => SetField(ref _State_Home, value, nameof(State_Home));
        }

        private string _Country_Home = string.Empty;
        public string Country_Home
        {
            get => _Country_Home;
            set => SetField(ref _Country_Home, value, nameof(Country_Home));
        }

        private string _PostalCode_Home = string.Empty;
        public string PostalCode_Home
        {
            get => _PostalCode_Home;
            set => SetField(ref _PostalCode_Home, value, nameof(PostalCode_Home));
        }

        private string _Phone1_Home = string.Empty;
        public string Phone1_Home
        {
            get => _Phone1_Home;
            set => SetField(ref _Phone1_Home, value, nameof(Phone1_Home));
        }

        private string _Phone2_Home = string.Empty;
        public string Phone2_Home
        {
            get => _Phone2_Home;
            set => SetField(ref _Phone2_Home, value, nameof(Phone2_Home));
        }

        private string _Fax_Home = string.Empty;
        public string Fax_Home
        {
            get => _Fax_Home;
            set => SetField(ref _Fax_Home, value, nameof(Fax_Home));
        }
        
        private string _Url_Home = string.Empty;
        public string Url_Home
        {
            get => _Url_Home;
            set => SetField(ref _Url_Home, value, nameof(Url_Home));
        }

        private bool _IsPrimaryInformation_Home = false;
        public bool IsPrimaryInformation_Home
        {
            get => _IsPrimaryInformation_Home;
            set => SetField(ref _IsPrimaryInformation_Home, value, nameof(IsPrimaryInformation_Home));
        }

        /////////////////////////////
        #endregion HOME page
        ///////////////////////////////////////////////////////////


        ///////////////////////////////////////////////////////////
        #region WORK page
        /////////////////////////////

        private string _Title_Work = string.Empty;
        public string Title_Work
        {
            get => _Title_Work;
            set => SetField(ref _Title_Work, value, nameof(Title_Work));
        }

        private string _Organization_Work = string.Empty;
        public string Organization_Work
        {
            get => _Organization_Work;
            set => SetField(ref _Organization_Work, value, nameof(Organization_Work));
        }

        private string _StreetAddress_Work = string.Empty;
        public string StreetAddress_Work
        {
            get => _StreetAddress_Work;
            set => SetField(ref _StreetAddress_Work, value, nameof(StreetAddress_Work));
        }

        private string _City_Work = string.Empty;
        public string City_Work
        {
            get => _City_Work;
            set => SetField(ref _City_Work, value, nameof(City_Work));
        }

        private string _State_Work = string.Empty;
        public string State_Work
        {
            get => _State_Work;
            set => SetField(ref _State_Work, value, nameof(State_Work));
        }

        private string _Country_Work = string.Empty;
        public string Country_Work
        {
            get => _Country_Work;
            set => SetField(ref _Country_Work, value, nameof(Country_Work));
        }

        private string _PostalCode_Work = string.Empty;
        public string PostalCode_Work
        {
            get => _PostalCode_Work;
            set => SetField(ref _PostalCode_Work, value, nameof(PostalCode_Work));
        }

        private string _Phone1_Work = string.Empty;
        public string Phone1_Work
        {
            get => _Phone1_Work;
            set => SetField(ref _Phone1_Work, value, nameof(Phone1_Work));
        }

        private string _Phone2_Work = string.Empty;
        public string Phone2_Work
        {
            get => _Phone2_Work;
            set => SetField(ref _Phone2_Work, value, nameof(Phone2_Work));
        }

        private string _Fax_Work = string.Empty;
        public string Fax_Work
        {
            get => _Fax_Work;
            set => SetField(ref _Fax_Work, value, nameof(Fax_Work));
        }

        private string _Url_Work = string.Empty;
        public string Url_Work
        {
            get => _Url_Work;
            set => SetField(ref _Url_Work, value, nameof(Url_Work));
        }

        private bool _IsPrimaryInformation_Work = false;
        public bool IsPrimaryInformation_Work
        {
            get => _IsPrimaryInformation_Work;
            set => SetField(ref _IsPrimaryInformation_Work, value, nameof(IsPrimaryInformation_Work));
        }

        /////////////////////////////
        #endregion WORK page
        ///////////////////////////////////////////////////////////


        ///////////////////////////////////////////////////////////
        #region OTHER page
        /////////////////////////////

        private string _OtherEmails = string.Empty;
        public string OtherEmails
        {
            get => _OtherEmails;
            set => SetField(ref _OtherEmails, value, nameof(OtherEmails));
        }

        private string _OtherPhones = string.Empty;
        public string OtherPhones
        {
            get => _OtherPhones;
            set => SetField(ref _OtherPhones, value, nameof(OtherPhones));
        }

        private string _OtherUrls = string.Empty;
        public string OtherUrls
        {
            get => _OtherUrls;
            set => SetField(ref _OtherUrls, value, nameof(OtherUrls));
        }

        /////////////////////////////
        #endregion OTHER page
        ///////////////////////////////////////////////////////////


        ///////////////////////////////////////////////////////////
        #region NOTES page
        /////////////////////////////

        private string _Notes = string.Empty;
        public string Notes
        {
            get => _Notes;
            set => SetField(ref _Notes, value, nameof(Notes));
        }

        /////////////////////////////
        #endregion NOTES page
        ///////////////////////////////////////////////////////////
        

        public Contact()
        {
        }
    }
}
