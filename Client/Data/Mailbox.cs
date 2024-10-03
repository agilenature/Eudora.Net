using Eudora.Net.Core;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace Eudora.Net.Data
{
    public class Mailbox : INotifyPropertyChanged
    {
        ///////////////////////////////////////////////////////////
        #region Fields

        // INotifyPropertyChanged isn't used in this class but its presence
        // is necessary for the unified database format.
        // See: Data/DatastoreBase.cs
#pragma warning disable 0067
        public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore 0067


        private Guid _Id = Guid.NewGuid();
        private string _Name = string.Empty;
        private string _ImageSource = string.Empty;
        private int _SortOrder = 999;
        private DatastoreConversion<EmailMessage, EmailMessage_DB, DbConverter_EmailMessage> Datastore;

        #endregion Fields
        ///////////////////////////////////////////////////////////


        ///////////////////////////////////////////////////////////
        #region Properties

        [SQLite.Ignore]
        public SortableObservableCollection<EmailMessage> Messages
        {
            get => Datastore.Data;
        }

        [SQLite.PrimaryKey]
        public Guid Id
        {
            get => _Id;
            set => _Id = value;
        }

        public int SortOrder
        {
            get => _SortOrder;
            set { if (_SortOrder != value) { _SortOrder = value; } }
        }

        public string Name
        {
            get => _Name;
            set { if (_Name != value) { _Name = value; } }
        }

        public string ImageSource
        {
            get => _ImageSource;
            set { if (_ImageSource != value) { _ImageSource = value; } }
        }

        #endregion Properties
        ///////////////////////////////////////////////////////////



        ///////////////////////////////////////////////////////////
        #region Mailbox Interface

        public Mailbox()
        {
            CommonCtor();
        }

        public Mailbox(string name, string imageSource)
        {
            _Name = name;
            _ImageSource = imageSource;
            CommonCtor();
        }

        public Mailbox(string name, string imageSource, int sortOrder)
        {
            _Name = name;
            _ImageSource = imageSource;
            _SortOrder = sortOrder;
            CommonCtor();
        }

        public void Close()
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

        public bool Contains(EmailMessage message)
        {
            return Messages.Contains(message);
        }

        public EmailMessage NewMessage(bool draft = false)
        {
            EmailMessage message = new()
            {
                MailboxName = Name,
                Status = draft ? EmailEnums.MessageStatus.Draft : EmailEnums.MessageStatus.Sealed
            };
            Messages.Add(message);
            return message;
        }

        public void AddMessage(EmailMessage message)
        {
            if (!System.Windows.Application.Current.Dispatcher.CheckAccess())
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() => AddMessage(message)));
                return;
            }
            Messages.Add(message);
        }

        public void DeleteMessage(EmailMessage message)
        {
            if (!System.Windows.Application.Current.Dispatcher.CheckAccess())
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() => DeleteMessage(message)));
                return;
            }
            Messages.Remove(message);
        }

        public async Task TransferMessage(EmailMessage message, Mailbox mailboxDest)
        {
            EmailMessage? msgClone = message.Clone() as EmailMessage;
            if(msgClone is null)
            {
                Logger.Error($"TransferMessage(): Message failed to clone.");
                return;
            }
            msgClone.MailboxName = Name;
            mailboxDest.AddMessage(msgClone);
            DeleteMessage(message);
        }


        #endregion Mailbox Interface
        ///////////////////////////////////////////////////////////
        


        ///////////////////////////////////////////////////////////
        #region Mailbox Internal

        private void CommonCtor()
        {
            try
            {
                Datastore = new("Data", "Mailboxes", _Name);
                Datastore.Open();
                Datastore.Load();
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
            }
        }


        #endregion Mailbox Internal
        ///////////////////////////////////////////////////////////
    }
}

