using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Eudora.Net.Core;

namespace Eudora.Net.Data
{
    public class EmailMessage : IEquatable<EmailMessage>, INotifyPropertyChanged, ICloneable
    {
        public static readonly string extension = ".msg";

        ///////////////////////////////////////////////////////////
        #region Interfaces
        /////////////////////////////

        /// <summary>
        /// INotifyPropertyChanged
        /// In this context, the INotify interface allows a message to autosave itself
        /// when its properties, bound to the UX for display and edit, are changed.
        /// It also facilitates the aforementioned databinding.
        /// Modern .Net is pretty awesome.
        /// </summary>
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

        /// <summary>
        /// IEquatable
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(EmailMessage? other)
        {
            if(other == null)
            {
                return false;
            }
            return other.InternalId == InternalId;
        }

        /// <summary>
        /// IClonable
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return this.MemberwiseClone();
        }

        /////////////////////////////
        #endregion Interfaces
        ///////////////////////////////////////////////////////////



        ///////////////////////////////////////////////////////////
        #region Properties
        /////////////////////////////

        /// <summary>
        /// A unique ID for all messages, independent of server tags or variable field values
        /// </summary>
        private Guid _InternalId = Guid.NewGuid();
        public Guid InternalId
        {
            get => _InternalId;
            set => SetField(ref _InternalId, value, nameof(InternalId));
        }

        /// <summary>
        /// The message ID that comes from the server/service
        /// </summary>
        private string _MessageId = string.Empty;
        public string MessageId
        {
            get => _MessageId;
            set => SetField(ref _MessageId, value, nameof(MessageId));
        }

        /// <summary>
        /// A reference to MessageID for replies
        /// </summary>
        private string _InReplyToId = string.Empty;
        public string InReplyToId
        {
            get => _InReplyToId;
            set => SetField(ref _InReplyToId, value, nameof(InReplyToId));
        }

        /// <summary>
        /// A list of id references
        /// </summary>
        private ObservableCollection<string> _ReferenceIDs = [];
        public ObservableCollection<string> ReferenceIDs
        {
            get => _ReferenceIDs;
            set => SetField(ref _ReferenceIDs, value, nameof(ReferenceIDs));
        }

        /// <summary>
        /// An email's reply-to address is not necessarily the same as the sender address
        /// </summary>
        private EmailAddress _ReplyTo = new();
        public EmailAddress ReplyTo
        {
            get => _ReplyTo;
            set => SetField(ref _ReplyTo, value, nameof(ReplyTo));
        }

        /// <summary>
        /// 
        /// </summary>
        private string _MailboxName = string.Empty;
        public string MailboxName
        {
            get => _MailboxName;
            set => SetField(ref _MailboxName, value, nameof(MailboxName));
        }


        /// <summary>
        /// Eudora's concept of a sender's address/account
        /// </summary>
        private Guid _PersonalityId;
        public Guid PersonalityID
        {
            get => _PersonalityId;
            set => SetField(ref _PersonalityId, value, nameof(PersonalityID));
        }

        /// <summary>
        /// 
        /// </summary>
        private string _LabelName = string.Empty;
        public string LabelName
        {
            get => _LabelName;
            set => SetField(ref _LabelName, value, nameof(LabelName));
        }

        /// <summary>
        /// Edit status of message
        /// </summary>
        public enum MessageStatus
        {
            Draft,
            Sealed,
            Default
        }
        private MessageStatus _Status = MessageStatus.Default;
        public MessageStatus Status
        {
            get => _Status;
            set => SetField(ref _Status, value, nameof(Status));
        }

        /// <summary>
        /// Message is read or unread
        /// </summary>
        public enum eReadStatus
        {
            Read,
            Unread
        }
        private eReadStatus _ReadStatus = eReadStatus.Unread;
        public eReadStatus ReadStatus
        {
            get => _ReadStatus;
            set => SetField(ref _ReadStatus, value, nameof(ReadStatus));
        }

        /// <summary>
        /// Message is sent or not sent
        /// </summary>
        public enum eSendStatus
        {
            Sent,
            Unsent
        }
        private eSendStatus _SendStatus = eSendStatus.Unsent;
        public eSendStatus SendStatus
        {
            get => _SendStatus;
            set => SetField(ref _SendStatus, value, nameof(SendStatus));
        }

        /// <summary>
        /// Where did this message come from
        /// </summary>
        public enum MessageOrigin
        {
            Outgoing,
            IncomingTo,
            IncomingCC,
            IncomingBCC,
            Default
        }
        private MessageOrigin _Origin = MessageOrigin.Default;
        public MessageOrigin Origin
        {
            get => _Origin;
            set => SetField(ref _Origin, value, nameof(Origin));
        }

        /// <summary>
        /// 
        /// </summary>
        private PostOffice.eMailPriority _Priority = PostOffice.eMailPriority.Normal;
        public PostOffice.eMailPriority Priority
        {
            get => _Priority;
            set => SetField(ref _Priority, value, nameof(Priority));
        }

        /// <summary>
        /// Date sent
        /// TODO: Initially date created, will update upon sending
        /// </summary>
        private DateTime _Date = DateTime.Now;
        public DateTime Date
        {
            get => _Date;
            set => SetField(ref _Date, value, nameof(Date));
        }


        private EmailAddress _SenderAddress = new();
        public EmailAddress SenderAddress
        {
            get => _SenderAddress;
            set => SetField(ref _SenderAddress, value, nameof(SenderAddress));
        }

        private ObservableCollection<EmailAddress> _Addresses_From = [];
        public ObservableCollection<EmailAddress> Addresses_From
        {
            get => _Addresses_From;
            set => SetField(ref _Addresses_From, value, nameof(Addresses_From));
        }

        /// <summary>
        /// The top-tier addressees, differentiating from CC's
        /// </summary>
        private ObservableCollection<EmailAddress> _Addresses_To = [];
        public ObservableCollection<EmailAddress> Addresses_To
        {
            get => _Addresses_To;
            set => SetField(ref _Addresses_To, value, nameof(Addresses_To));
        }

        /// <summary>
        /// List of CC addresses
        /// </summary>
        private ObservableCollection<EmailAddress> _Addresses_CC = [];
        public ObservableCollection<EmailAddress> Addresses_CC
        {
            get => _Addresses_CC;
            set => SetField(ref _Addresses_CC, value, nameof(Addresses_CC));
        }

        /// <summary>
        /// List of BCC addresses
        /// </summary>
        private ObservableCollection<EmailAddress> _Addresses_BCC = [];
        public ObservableCollection<EmailAddress> Addresses_BCC
        {
            get => _Addresses_BCC;
            set => SetField(ref _Addresses_BCC, value, nameof(Addresses_BCC));
        }

        /// <summary>
        /// Email subject line
        /// </summary>
        private string _Subject = string.Empty;
        public string Subject
        {
            get => _Subject;
            set => SetField(ref _Subject, value, nameof(Subject));
        }

        /// <summary>
        /// Email body content
        /// </summary>
        private string _Body = string.Empty;
        public string Body
        {
            get => _Body;
            set => SetField(ref _Body, value, nameof(Body));
        }

        /// <summary>
        /// Attchments to the email
        /// </summary>
        private ObservableCollection<EmailAttachment> _Attachments = [];
        public ObservableCollection<EmailAttachment> Attachments
        {
            get => _Attachments;
            set => SetField(ref _Attachments, value, nameof(Attachments));
        }

        private ObservableCollection<EmbeddedImage> _InlineImages = [];
        public ObservableCollection<EmbeddedImage> InlineImages
        {
            get => _InlineImages;
            set => SetField(ref _InlineImages, value, nameof(InlineImages));
        }

        private PostOffice.eOutgoingType _MessageCategory = PostOffice.eOutgoingType.NewThread;
        public PostOffice.eOutgoingType MessageCategory
        {
            get => _MessageCategory;
            set => SetField(ref _MessageCategory, value, nameof(MessageCategory));
        }

        /////////////////////////////
        #endregion Properties
        ///////////////////////////////////////////////////////////



        ///////////////////////////////////////////////////////////
        #region Construction
        /////////////////////////////

        public EmailMessage() 
        {
            ConnectCollectionsToChangeEvents();
        }

        public void ConnectCollectionsToChangeEvents()
        {
            Addresses_To.CollectionChanged += Addresses_To_CollectionChanged;
            Addresses_CC.CollectionChanged += Addresses_CC_CollectionChanged;
            Addresses_BCC.CollectionChanged += Addresses_BCC_CollectionChanged;
            Attachments.CollectionChanged += Attachments_CollectionChanged;
            InlineImages.CollectionChanged += InlineImages_CollectionChanged;
        }

        /////////////////////////////
        #endregion Construction
        ///////////////////////////////////////////////////////////



        ///////////////////////////////////////////////////////////
        #region Operations
        /////////////////////////////        

        /// <summary>
        /// The ObservableCollection Changed event has its own delegate/prototype
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Addresses_BCC_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(Addresses_BCC));
        }

        private void Addresses_CC_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(Addresses_CC));
        }

        private void Addresses_To_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(Addresses_To));
        }

        private void Attachments_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(Attachments));
        }

        private void InlineImages_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(InlineImages));
        }

        /////////////////////////////
        #endregion Operations
        ///////////////////////////////////////////////////////////
    }
}
