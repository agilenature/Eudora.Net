using MailKit.Security;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Eudora.Net.Core;
using SQLite;
using System.Text.Json.Serialization;

namespace Eudora.Net.Data
{
    /// <summary>
    /// The Eudora concept of a user/user id and email account.
    /// </summary>
    public class Personality : INotifyPropertyChanged
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
        private string _PersonalityName = "unnamed";
        private bool _IsDefault = false;
        private string _DefaultSignature = GHelpers.NullSelection;
        private string _DefaultStationery = GHelpers.NullSelection;
        private string _EmailName = string.Empty;
        private string _EmailAddress = "blank@blank.com";
        private string _EmailPassword = string.Empty;
        private MailKit.Security.SecureSocketOptions _SocketOptions_Outgoing = SecureSocketOptions.Auto;
        private string _Server_Outgoing = string.Empty;
        private int _Port_Outgoing = 0;
        private MailKit.Security.SecureSocketOptions _SocketOptions_Incoming = SecureSocketOptions.Auto;
        private string _Server_Incoming = string.Empty;
        private int _Port_Incoming = 0;
        private bool _UsePOP = false;
        private bool _UseImap = true;
        private bool _DeleteMailOnServer = false;

        /////////////////////////////
        #endregion Fields
        ///////////////////////////////////////////////////////////



        ///////////////////////////////////////////////////////////
        #region Properties
        /////////////////////////////

        [SQLite.Ignore, JsonIgnore]
        public bool IsGmail
        {
            get
            {
                return _EmailAddress.Contains("@gmail.com", StringComparison.CurrentCultureIgnoreCase);
            }
        }

        [SQLite.PrimaryKey]
        public Guid Id
        {
            get => _Id;
            set => SetField(ref _Id, value, nameof(Id));
        }

        public string PersonalityName
        {
            get => _PersonalityName;
            set => SetField(ref _PersonalityName, value, nameof(PersonalityName));
        }
        
        public bool IsDefault
        {
            get => _IsDefault;
            set => SetField(ref _IsDefault, value, nameof(IsDefault));
        }
        
        public string DefaultSignature
        {
            get => _DefaultSignature;
            set => SetField(ref _DefaultSignature, value, nameof(DefaultSignature));
        }
        
        public string DefaultStationery
        {
            get => _DefaultStationery;
            set => SetField(ref _DefaultStationery, value, nameof(DefaultStationery));
        }

        // Common mail settings
        public string EmailName
        {
            get => _EmailName;
            set => SetField(ref _EmailName, value, nameof(EmailName));
        }
        
        public string EmailAddress
        {
            get => _EmailAddress;
            set => SetField(ref _EmailAddress, value, nameof(EmailAddress));
        }
        
        public string EmailPassword
        {
            get => _EmailPassword;
            set => SetField(ref _EmailPassword, value, nameof(EmailPassword));
        }

        // Outgoing mail settings
        public SecureSocketOptions SocketOptions_Outgoing
        {
            get => _SocketOptions_Outgoing;
            set => SetField(ref _SocketOptions_Outgoing, value, nameof(SocketOptions_Outgoing));
        }
        
        public string Server_Outgoing
        {
            get => _Server_Outgoing;
            set => SetField(ref _Server_Outgoing, value, nameof(Server_Outgoing));
        }
        
        public int Port_Outgoing
        {
            get => _Port_Outgoing;
            set => SetField(ref _Port_Outgoing, value, nameof(Port_Outgoing));
        }

        // Incoming mail settings
        public SecureSocketOptions SocketOptions_Incoming
        {
            get => _SocketOptions_Incoming;
            set => SetField(ref _SocketOptions_Incoming, value, nameof(SocketOptions_Incoming));
        }
        
        public string Server_Incoming
        {
            get => _Server_Incoming;
            set => SetField(ref _Server_Incoming, value, nameof(Server_Incoming));
        }
        
        public int Port_Incoming
        {
            get => _Port_Incoming;
            set => SetField(ref _Port_Incoming, value, nameof(Port_Incoming));
        }
        
        public bool UsePop
        {
            get => _UsePOP;
            set => SetField(ref _UsePOP, value, nameof(UsePop));
        }
        
        public bool UseImap
        {
            get => _UseImap;
            set => SetField(ref _UseImap, value, nameof(UseImap));
        }
        
        public bool DeleteMailOnServer
        {
            get => _DeleteMailOnServer;
            set => SetField(ref _DeleteMailOnServer, value, nameof(DeleteMailOnServer));
        }
        

        /////////////////////////////
        #endregion Properties
        ///////////////////////////////////////////////////////////




        ///////////////////////////////////////////////////////////
        #region Operations
        /////////////////////////////

        public Personality()
        {
        }

        public Personality(
            string personalityName, 
            Guid id, 
            bool isDefault, 
            string defaultSignature, 
            string defaultStationery, 
            string emailName, 
            string emailAddress, 
            string emailPassword, 
            SecureSocketOptions socketOptions_Outgoing, 
            string server_Outgoing, 
            int port_Outgoing, 
            SecureSocketOptions socketOptions_Incoming, 
            string server_Incoming, 
            int port_Incoming, 
            bool usePop, 
            bool useImap, 
            bool deleteMailOnServer)
        {
            _PersonalityName = personalityName;
            _Id = id;
            _IsDefault = isDefault;
            _DefaultSignature = defaultSignature;
            _DefaultStationery = defaultStationery;
            _EmailName = emailName;
            _EmailAddress = emailAddress;
            _EmailPassword = emailPassword;
            _SocketOptions_Outgoing = socketOptions_Outgoing;
            _Server_Outgoing = server_Outgoing;
            _Port_Outgoing = port_Outgoing;
            _SocketOptions_Incoming = socketOptions_Incoming;
            _Server_Incoming = server_Incoming;
            _Port_Incoming = port_Incoming;
            _UsePOP = usePop;
            _UseImap = useImap;
            _DeleteMailOnServer = deleteMailOnServer;
        }

        public void MakeDefault()
        {
            foreach (Personality p in PersonalityManager.Datastore.Data)
            {
                if (p != this && p.IsDefault)
                {
                    p.IsDefault = false;
                }
            }
            IsDefault = true;
        }

        public EmailAddress ToEmailAddress()
        {
            return new EmailAddress(EmailName, EmailAddress);
        }

        /////////////////////////////
        #endregion Operations
        ///////////////////////////////////////////////////////////
    }


    internal static class PersonalityManager
    {
        public static DatastoreBase<Personality> Datastore;

        static PersonalityManager()
        {
            Datastore = new("Data", "Personalities", "Personalities");
        }

        public static void Startup()
        {
            try
            {
                Datastore.Open();
                Datastore.Load();
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
            }
        }

        public static void Shutdown()
        {
            try
            {
                Datastore.Close();
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
            }
        }

        public static Personality New(string name)
        {
            Personality personality = new() { PersonalityName = name };
            Datastore.Add(personality);
            return personality;
        }

        public static void Add(Personality personality)
        {
            Datastore.Add(personality);
        }

        public static void Remove(Personality personality)
        {
            Datastore.Delete(personality);
        }

        public static bool Contains(string name)
        {
            if (Datastore.Data.Any(x => x.PersonalityName.Equals(name, StringComparison.CurrentCultureIgnoreCase))) return true;
            return false;
        }

        public static Personality? FindPersonality(Guid id)
        {
            Personality? personality = null;

            try
            {
                personality = Datastore.Data.Where(i => i.Id == id).First();
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
            }

            return personality;
        }

        public static Personality? DefaultPersonality()
        {
            return Datastore.Data.Where(p => p.IsDefault == true).FirstOrDefault();
        }
    }
}
