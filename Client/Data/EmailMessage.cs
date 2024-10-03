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

        // INotifyPropertyChanged

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


        // IEquatable

        public bool Equals(EmailMessage? other)
        {
            if(other == null)
            {
                return false;
            }
            
            if( other.InternalId == InternalId )
            {
                return true;
            }

            if( other.MessageId == MessageId )
            {
                return true;
            }

            if (other.SenderAddress == SenderAddress &&
                other.Subject == Subject &&
                other.Date == Date &&
                other.Body == Body)
            {
                return true;
            }

            return false;
        }


        // ICloneable

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        #endregion Interfaces
        ///////////////////////////////////////////////////////////



        ///////////////////////////////////////////////////////////
        #region Fields

        private Guid _InternalId = Guid.NewGuid();
        private string _MessageId = string.Empty;
        private string _InReplyToId = string.Empty;
        private ObservableCollection<string> _ReferenceIDs = [];
        private EmailAddress _ReplyTo = new();
        private string _MailboxName = string.Empty;
        private Guid _PersonalityId;
        private string _LabelName = string.Empty;
        private EmailEnums.MessageStatus _Status = EmailEnums.MessageStatus.Default;
        private EmailEnums.eReadStatus _ReadStatus = EmailEnums.eReadStatus.Unread;
        private EmailEnums.eSendStatus _SendStatus = EmailEnums.eSendStatus.Unsent;
        private EmailEnums.MessageOrigin _Origin = EmailEnums.MessageOrigin.Default;
        private PostOffice.eMailPriority _Priority = PostOffice.eMailPriority.Normal;
        private DateTime _Date = DateTime.Now;
        private EmailAddress _SenderAddress = new();
        private ObservableCollection<EmailAddress> _Addresses_From = [];
        private ObservableCollection<EmailAddress> _Addresses_To = [];
        private ObservableCollection<EmailAddress> _Addresses_CC = [];
        private ObservableCollection<EmailAddress> _Addresses_BCC = [];
        private string _Subject = string.Empty;
        private string _Body = string.Empty;
        private ObservableCollection<EmailAttachment> _Attachments = [];
        private ObservableCollection<EmbeddedImage> _InlineImages = [];
        private PostOffice.eMailThreadType _MessageCategory = PostOffice.eMailThreadType.NewThread;

        #endregion Fields
        ///////////////////////////////////////////////////////////



        ///////////////////////////////////////////////////////////
        #region Properties

        /// <summary>
        /// A unique ID for all messages, independent of server tags or variable field values
        /// </summary>
        public Guid InternalId
        {
            get => _InternalId;
            set => SetField(ref _InternalId, value, nameof(InternalId));
        }

        /// <summary>
        /// The message ID that comes from the server/service
        /// </summary>
        public string MessageId
        {
            get => _MessageId;
            set => SetField(ref _MessageId, value, nameof(MessageId));
        }

        /// <summary>
        /// A reference to MessageID for replies
        /// </summary>
        
        public string InReplyToId
        {
            get => _InReplyToId;
            set => SetField(ref _InReplyToId, value, nameof(InReplyToId));
        }

        /// <summary>
        /// A list of id references
        /// </summary>
        
        public ObservableCollection<string> ReferenceIDs
        {
            get => _ReferenceIDs;
            set => SetField(ref _ReferenceIDs, value, nameof(ReferenceIDs));
        }

        /// <summary>
        /// An email's reply-to address is not necessarily the same as the sender address
        /// </summary>
        
        public EmailAddress ReplyTo
        {
            get => _ReplyTo;
            set => SetField(ref _ReplyTo, value, nameof(ReplyTo));
        }

        /// <summary>
        /// 
        /// </summary>
        
        public string MailboxName
        {
            get => _MailboxName;
            set => SetField(ref _MailboxName, value, nameof(MailboxName));
        }

        /// <summary>
        /// Eudora's concept of a sender's address/account
        /// </summary>
        
        public Guid PersonalityID
        {
            get => _PersonalityId;
            set => SetField(ref _PersonalityId, value, nameof(PersonalityID));
        }

        /// <summary>
        /// 
        /// </summary>
        
        public string LabelName
        {
            get => _LabelName;
            set => SetField(ref _LabelName, value, nameof(LabelName));
        }

        public EmailEnums.MessageStatus Status
        {
            get => _Status;
            set => SetField(ref _Status, value, nameof(Status));
        }

        public EmailEnums.eReadStatus ReadStatus
        {
            get => _ReadStatus;
            set => SetField(ref _ReadStatus, value, nameof(ReadStatus));
        }

        public EmailEnums.eSendStatus SendStatus
        {
            get => _SendStatus;
            set => SetField(ref _SendStatus, value, nameof(SendStatus));
        }

        public EmailEnums.MessageOrigin Origin
        {
            get => _Origin;
            set => SetField(ref _Origin, value, nameof(Origin));
        }

        /// <summary>
        /// 
        /// </summary>
        public PostOffice.eMailPriority Priority
        {
            get => _Priority;
            set => SetField(ref _Priority, value, nameof(Priority));
        }

        /// <summary>
        /// Date sent
        /// TODO: Initially date created, will update upon sending
        /// </summary>
        public DateTime Date
        {
            get => _Date;
            set => SetField(ref _Date, value, nameof(Date));
        }

        public EmailAddress SenderAddress
        {
            get => _SenderAddress;
            set => SetField(ref _SenderAddress, value, nameof(SenderAddress));
        }

        public ObservableCollection<EmailAddress> Addresses_From
        {
            get => _Addresses_From;
            set => SetField(ref _Addresses_From, value, nameof(Addresses_From));
        }

        /// <summary>
        /// The top-tier addressees, differentiating from CC's
        /// </summary>
        public ObservableCollection<EmailAddress> Addresses_To
        {
            get => _Addresses_To;
            set => SetField(ref _Addresses_To, value, nameof(Addresses_To));
        }

        /// <summary>
        /// List of CC addresses
        /// </summary>
        public ObservableCollection<EmailAddress> Addresses_CC
        {
            get => _Addresses_CC;
            set => SetField(ref _Addresses_CC, value, nameof(Addresses_CC));
        }

        /// <summary>
        /// List of BCC addresses
        /// </summary>
        public ObservableCollection<EmailAddress> Addresses_BCC
        {
            get => _Addresses_BCC;
            set => SetField(ref _Addresses_BCC, value, nameof(Addresses_BCC));
        }

        /// <summary>
        /// Email subject line
        /// </summary>
        public string Subject
        {
            get => _Subject;
            set => SetField(ref _Subject, value, nameof(Subject));
        }

        /// <summary>
        /// Email body content
        /// </summary>
        public string Body
        {
            get => _Body;
            set => SetField(ref _Body, value, nameof(Body));
        }

        /// <summary>
        /// Attchments to the email
        /// </summary>
        public ObservableCollection<EmailAttachment> Attachments
        {
            get => _Attachments;
            set => SetField(ref _Attachments, value, nameof(Attachments));
        }
        
        public ObservableCollection<EmbeddedImage> InlineImages
        {
            get => _InlineImages;
            set => SetField(ref _InlineImages, value, nameof(InlineImages));
        }
        
        public PostOffice.eMailThreadType MessageCategory
        {
            get => _MessageCategory;
            set => SetField(ref _MessageCategory, value, nameof(MessageCategory));
        }

        #endregion Properties
        ///////////////////////////////////////////////////////////




        ///////////////////////////////////////////////////////////
        #region Interface

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

        #endregion Interface
        ///////////////////////////////////////////////////////////
    }
}
