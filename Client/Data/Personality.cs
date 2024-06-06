using MailKit.Security;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Eudora.Net.Core;

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
        #region Properties
        /////////////////////////////

        public static readonly string extension = ".per";

        private string _PersonalityName = "unnamed";
        public string PersonalityName
        {
            get => _PersonalityName;
            set => SetField(ref _PersonalityName, value, nameof(PersonalityName));
        }

        private Guid _Id = Guid.NewGuid();
        public Guid Id
        {
            get => _Id;
            set => SetField(ref _Id, value, nameof(Id));
        }

        private bool _IsDefault = false;
        public bool IsDefault
        {
            get => _IsDefault;
            set => SetField(ref _IsDefault, value, nameof(IsDefault));
        }

        private string _DefaultSignature = GHelpers.NullSelection;
        public string DefaultSignature
        {
            get => _DefaultSignature;
            set => SetField(ref _DefaultSignature, value, nameof(DefaultSignature));
        }

        private string _DefaultStationery = GHelpers.NullSelection;
        public string DefaultStationery
        {
            get => _DefaultStationery;
            set => SetField(ref _DefaultStationery, value, nameof(DefaultStationery));
        }

        // Common mail settings
        private string _EmailName = string.Empty;
        public string EmailName
        {
            get => _EmailName;
            set => SetField(ref _EmailName, value, nameof(EmailName));
        }

        private string _EmailAddress = "blank@blank.com";
        public string EmailAddress
        {
            get => _EmailAddress;
            set => SetField(ref _EmailAddress, value, nameof(EmailAddress));
        }

        private string _EmailPassword = string.Empty;
        public string EmailPassword
        {
            get => _EmailPassword;
            set => SetField(ref _EmailPassword, value, nameof(EmailPassword));
        }

        // Outgoing mail settings
        private MailKit.Security.SecureSocketOptions _SocketOptions_Outgoing = SecureSocketOptions.Auto;
        public SecureSocketOptions SocketOptions_Outgoing
        {
            get => _SocketOptions_Outgoing;
            set => SetField(ref _SocketOptions_Outgoing, value, nameof(SocketOptions_Outgoing));
        }

        private string _Server_Outgoing = string.Empty;
        public string Server_Outgoing
        {
            get => _Server_Outgoing;
            set => SetField(ref _Server_Outgoing, value, nameof(Server_Outgoing));
        }

        private int _Port_Outgoing = 0;
        public int Port_Outgoing
        {
            get => _Port_Outgoing;
            set => SetField(ref _Port_Outgoing, value, nameof(Port_Outgoing));
        }

        // Incoming mail settings
        private MailKit.Security.SecureSocketOptions _SocketOptions_Incoming = SecureSocketOptions.Auto;
        public SecureSocketOptions SocketOptions_Incoming
        {
            get => _SocketOptions_Incoming;
            set => SetField(ref _SocketOptions_Incoming, value, nameof(SocketOptions_Incoming));
        }

        private string _Server_Incoming = string.Empty;
        public string Server_Incoming
        {
            get => _Server_Incoming;
            set => SetField(ref _Server_Incoming, value, nameof(Server_Incoming));
        }

        private int _Port_Incoming = 0;
        public int Port_Incoming
        {
            get => _Port_Incoming;
            set => SetField(ref _Port_Incoming, value, nameof(Port_Incoming));
        }

        private bool _UsePOP = false;
        public bool UsePop
        {
            get => _UsePOP;
            set => SetField(ref _UsePOP, value, nameof(UsePop));
        }

        private bool _UseImap = true;
        public bool UseImap
        {
            get => _UseImap;
            set => SetField(ref _UseImap, value, nameof(UseImap));
        }

        private bool _DeleteMailOnServer = false;
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
            foreach (Personality p in PersonalityManager.Collection)
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


    public static class PersonalityManager
    {
        private static readonly string DataRoot = string.Empty;
        private static object Locker;

        public static ObservableCollection<Personality> Collection { get; private set; } = [];

        static PersonalityManager()
        {
            Locker = new();
            DataRoot = Path.Combine(Eudora.Net.Properties.Settings.Default.DataStoreRoot, "Personalities");
            IoUtil.EnsureFolder(DataRoot);
        }

        public static void Startup()
        {
            Load();
        }

        public static void Shutdown()
        {

        }

        private static void Personality_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is Personality personality)
            {
                Save(personality);
            }
        }

        private static string MakeFilename(string name)
        {
            return string.Format("{0}{1}", name, Personality.extension);
        }

        private static string MakeFullPath(string name)
        {
            return Path.Combine(DataRoot, MakeFilename(name));
        }

        public static Personality New(string name)
        {
            Personality personality = new() { PersonalityName = name };
            personality.PropertyChanged += Personality_PropertyChanged;
            Collection.Add(personality);
            Save(personality);
            return personality;
        }

        public static void Add(Personality personality)
        {
            Collection.Add(personality);
            Save(personality);
        }

        public static void Remove(Personality personality)
        {
            Collection.Remove(personality);

            try
            {
                string fullPath = MakeFullPath(personality.PersonalityName);
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        public static bool Contains(string name)
        {
            if (Collection.Any(x => x.PersonalityName.Equals(name, StringComparison.CurrentCultureIgnoreCase))) return true;
            return false;
        }

        private static void Load()
        {
            try
            {
                lock (Locker)
                {
                    DirectoryInfo di = new(DataRoot);
                    string searchQuery = string.Format("*{0}", Personality.extension);
                    var files = di.GetFiles(searchQuery);
                    foreach (var file in files)
                    {
                        if (file.DirectoryName == null)
                        {
                            continue;
                        }

                        string raw = File.ReadAllText(file.FullName);
                        var personality = JsonSerializer.Deserialize<Personality>(raw);
                        if (personality is not null)
                        {
                            personality.PropertyChanged += Personality_PropertyChanged;
                            Collection.Add(personality);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }
      
        public static void Save(Personality personality)
        {
            try
            {
                lock(Locker)
                {
                    string fullPath = MakeFullPath(personality.PersonalityName);
                    string json = JsonSerializer.Serialize<Personality>(personality, IoUtil.JsonWriterOptions);
                    File.WriteAllText(fullPath, json);
                }
            }
            catch(Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        public static Personality? FindPersonality(Guid id)
        {
            Personality? personality = null;

            try
            {
                personality = Collection.Where(i => i.Id == id).First();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }

            return personality;
        }

        public static Personality? DefaultPersonality()
        {
            return Collection.Where(p => p.IsDefault == true).FirstOrDefault();
        }
    }
}
