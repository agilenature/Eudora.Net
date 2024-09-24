using Eudora.Net.Core;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace Eudora.Net.Data
{
    public class Mailbox
    {
        ///////////////////////////////////////////////////////////
        #region Fields
        /////////////////////////////

        public static readonly string extension = ".mbx";
        private string DataRoot = string.Empty;
        private object FileLocker = new();
        private object CollectionLocker = new();
        private string _Name = string.Empty;
        private string _ImageSource = string.Empty;
        private int _SortOrder = 999;

        /////////////////////////////
        #endregion Fields
        ///////////////////////////////////////////////////////////


        ///////////////////////////////////////////////////////////
        #region Properties
        /////////////////////////////

        //public virtual ICollection<EmailMessage> Messages { get; private set; } = new ObservableCollection<EmailMessage>();
        [SQLite.Ignore, JsonIgnore]
        public ObservableCollection<EmailMessage> Messages { get; set; } = [];


        public int SortOrder
        {
            get => _SortOrder;
            set { if (_SortOrder != value) { _SortOrder = value; } }
        }

        public string Name
        {
            get => _Name;
            set { if (_Name != value) { _Name = value; InitializeForIO(); } }
        }

        public string ImageSource
        {
            get => _ImageSource;
            set { if (_ImageSource != value) { _ImageSource = value; } }
        }

        /////////////////////////////
        #endregion Properties
        ///////////////////////////////////////////////////////////


        public Mailbox()
        {
        }

        public Mailbox(string name, string imageSource)
        {
            _Name = name;
            _ImageSource = imageSource;
            InitializeForIO();
        }

        public Mailbox(string name, string imageSource, int sortOrder)
        {
            _Name = name;
            _ImageSource = imageSource;
            _SortOrder = sortOrder;
            InitializeForIO();
        }

        private void InitializeForIO()
        {
            DataRoot = Path.Combine(PostOffice.MailboxesPath, Name);
            IoUtil.EnsureFolder(DataRoot);
        }

        public bool Contains(EmailMessage message)
        {
            return Messages.Contains(message);
        }

        public void Delete()
        {
            try
            {
                DirectoryInfo di = new(DataRoot);
                di.Delete(true);
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
            }
        }

        ///////////////////////////////////////////////////////////
        #region File handling
        /////////////////////////////

        private string MakeFilename(string name)
        {
            return string.Format("{0}{1}", name, EmailMessage.extension);
        }

        private string MakeFullPath(string name)
        {
            return Path.Combine(DataRoot, MakeFilename(name));
        }

        private string MakeAttachmentsFolder(string name)
        {
            return Path.Combine(DataRoot, name);
        }

        private void Message_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(EmailMessage.Attachments))
            {
                Debug.WriteLine("Attachments changed");
            }
            if (sender is EmailMessage message)
            {
                Debug.WriteLine("Message saved");
                SaveMessage(message);
            }
        }

        public void Load()
        {
            try
            {
                lock (FileLocker)
                {
                    DirectoryInfo di = new(DataRoot);
                    string searchQuery = string.Format("*{0}", EmailMessage.extension);
                    var files = di.GetFiles(searchQuery);
                    foreach (var file in files)
                    {
                        if (file.DirectoryName == null)
                        {
                            continue;
                        }

                        string raw = File.ReadAllText(file.FullName);
                        var message = JsonSerializer.Deserialize<EmailMessage>(raw);
                        if (message is not null)
                        {
                            message.PropertyChanged += Message_PropertyChanged;
                            message.ConnectCollectionsToChangeEvents();
                            Messages.Add(message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
            }
        }

        public EmailMessage NewMessage(bool draft = false)
        {
            EmailMessage message = new()
            {
                MailboxName = Name,
                Status = draft ? EmailMessage.MessageStatus.Draft : EmailMessage.MessageStatus.Sealed
            };
            message.PropertyChanged += Message_PropertyChanged;
            Messages.Add(message);
            SaveMessage(message);
            return message;
        }

        public void AddMessage(EmailMessage message)
        {
            if (!System.Windows.Application.Current.Dispatcher.CheckAccess())
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() => AddMessage(message)));
                return;
            }
            message.PropertyChanged -= Message_PropertyChanged;
            message.PropertyChanged += Message_PropertyChanged;
            Messages.Add(message);
            SaveMessage(message);
        }

        public void SaveMessage(EmailMessage message)
        {
            try
            {
                lock (FileLocker)
                {
                    string fullPath = MakeFullPath(message.InternalId.ToString());
                    string json = JsonSerializer.Serialize(message, IoUtil.JsonWriterOptions);
                    File.WriteAllText(fullPath, json);
                }
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
            }
        }

        public EmailMessage? LoadMessage(string fullPath)
        {
            EmailMessage? message = null;
            try
            {
                lock (FileLocker)
                {
                    string raw = File.ReadAllText(fullPath);
                    message = JsonSerializer.Deserialize<EmailMessage>(raw);
                    if (message is not null)
                    {
                        message.PropertyChanged += Message_PropertyChanged;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
            }
            return message;
        }

        public void UnloadMessage(EmailMessage message)
        {
            try
            {
                Messages.Remove(message);
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
            }
        }

        public void DeleteMessage(EmailMessage message)
        {
            try
            {
                Messages.Remove(message);

                // message
                string path = MakeFullPath(message.InternalId.ToString());
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                // attachments
                path = MakeAttachmentsFolder(message.InternalId.ToString());
                if (Directory.Exists(path))
                {
                    Directory.Delete(path);
                }
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
            }
        }

        /////////////////////////////
        #endregion File handling
        ///////////////////////////////////////////////////////////
    }
}

