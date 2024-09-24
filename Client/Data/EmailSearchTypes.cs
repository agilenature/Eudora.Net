using Eudora.Net.Core;
using Eudora.Net.Data;
using SQLite;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Eudora.Net.EmailSearch
{
    public enum eSearchOperand
    {
        Contains,
        DoesNotContain,
        Equals,
        NotEqual,
        GreaterThan,
        GreaterThanOrEqual,
        LessThan,
        LessThanOrEqual
    }

    /// <summary>
    /// Wrapper for binding to the UX
    /// </summary>
    public class SearchOperand : INotifyPropertyChanged
    {
        ////////////////////////////////////////////////////////////////
        #region INotifyPropertyChanged
        ////////////////////////////////////////////////////////////////

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void SetField<TField>(ref TField field, TField value, string propertyName)
        {
            if (EqualityComparer<TField>.Default.Equals(field, value))
            {
                return;
            }

            field = value;
            OnPropertyChanged(propertyName);
        }

        ////////////////////////////////////////////////////////////////
        #endregion INotifyPropertyChanged
        ////////////////////////////////////////////////////////////////
        
        private string _DisplayName = string.Empty;
        public string DisplayName
        {
            get => _DisplayName;
            set => SetField(ref _DisplayName, value, nameof(DisplayName));
        }

        private eSearchOperand _Operand = default;
        public eSearchOperand Operand
        {
            get => _Operand;
            set => SetField(ref _Operand, value, nameof(Operand));
        }

        public SearchOperand()
        { }

        public SearchOperand(string displayName, eSearchOperand operand)
        {
            DisplayName = displayName;
            Operand = operand;
        }
    }

    public enum eQueryKey
    {
        InternalID,
        MessageID,
        ReplyToID,
        ReferenceIDs,
        ReplyTo,
        MailboxName,
        PersonalityID,
        LabelName,
        Status,
        ReadStatus,
        SendStatus,
        Origin,
        Priority,
        Date,
        SenderAddress,
        From,
        To,
        CC,
        BCC,
        AllAddressees,
        Subject,
        Body
    }

    /// <summary>
    /// Wrapper for binding to the UX
    /// </summary>
    public class QueryKey : INotifyPropertyChanged
    {
        ////////////////////////////////////////////////////////////////
        #region INotifyPropertyChanged
        ////////////////////////////////////////////////////////////////

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void SetField<TField>(ref TField field, TField value, string propertyName)
        {
            if (EqualityComparer<TField>.Default.Equals(field, value))
            {
                return;
            }

            field = value;
            OnPropertyChanged(propertyName);
        }

        ////////////////////////////////////////////////////////////////
        #endregion INotifyPropertyChanged
        ////////////////////////////////////////////////////////////////

        private string _DisplayName = string.Empty;
        public string DisplayName
        {
            get => _DisplayName;
            set => SetField(ref _DisplayName, value, nameof(DisplayName));
        }

        private eQueryKey _Key = default;
        public eQueryKey Key
        {
            get => _Key;
            set => SetField(ref _Key, value, nameof(Key));
        }

        public QueryKey()
        {
            Build();
        }

        public QueryKey(string displayName, eQueryKey key)
        {
            DisplayName = displayName;
            Key = key;
            Build();
        }

        private void Build()
        {
            Lookup = [];
            Lookup.Add(eQueryKey.AllAddressees, typeof(string));
            Lookup.Add(eQueryKey.BCC, typeof(string));
            Lookup.Add(eQueryKey.Body, typeof(string));
            Lookup.Add(eQueryKey.CC, typeof(string));
            Lookup.Add(eQueryKey.Date, typeof(DateTime));
            Lookup.Add(eQueryKey.From, typeof(string));
            Lookup.Add(eQueryKey.InternalID, typeof(Guid));
            Lookup.Add(eQueryKey.LabelName, typeof(string));
            Lookup.Add(eQueryKey.MailboxName, typeof(string));
            Lookup.Add(eQueryKey.MessageID, typeof(string));
            Lookup.Add(eQueryKey.Origin, typeof(EmailMessage.MessageOrigin));
            Lookup.Add(eQueryKey.PersonalityID, typeof(Guid));
            Lookup.Add(eQueryKey.Priority, typeof(PostOffice.eMailPriority));
            Lookup.Add(eQueryKey.ReadStatus, typeof(EmailMessage.eReadStatus));
            Lookup.Add(eQueryKey.ReferenceIDs, typeof(string));
            Lookup.Add(eQueryKey.ReplyTo, typeof(string));
            Lookup.Add(eQueryKey.ReplyToID, typeof(string));
            Lookup.Add(eQueryKey.SenderAddress, typeof(string));
            Lookup.Add(eQueryKey.SendStatus, typeof(EmailMessage.eSendStatus));
            Lookup.Add(eQueryKey.Status, typeof(EmailMessage.MessageStatus));
            Lookup.Add(eQueryKey.Subject, typeof(string));
            Lookup.Add(eQueryKey.To, typeof(string));
        }

        [SQLite.Ignore]
        public static Dictionary<eQueryKey, Type> Lookup { get; private set; }

        //static QueryKey()
        //{
        //    Lookup = [];
        //    Lookup.Add(eQueryKey.AllAddressees, typeof(string));
        //    Lookup.Add(eQueryKey.BCC, typeof(string));
        //    Lookup.Add(eQueryKey.Body, typeof(string));
        //    Lookup.Add(eQueryKey.CC, typeof(string));
        //    Lookup.Add(eQueryKey.Date, typeof(DateTime));
        //    Lookup.Add(eQueryKey.From, typeof(string));
        //    Lookup.Add(eQueryKey.InternalID, typeof(Guid));
        //    Lookup.Add(eQueryKey.LabelName, typeof(string));
        //    Lookup.Add(eQueryKey.MailboxName, typeof(string));
        //    Lookup.Add(eQueryKey.MessageID, typeof(string));
        //    Lookup.Add(eQueryKey.Origin, typeof(EmailMessage.MessageOrigin));
        //    Lookup.Add(eQueryKey.PersonalityID, typeof(Guid));
        //    Lookup.Add(eQueryKey.Priority, typeof(PostOffice.eMailPriority));
        //    Lookup.Add(eQueryKey.ReadStatus, typeof(EmailMessage.eReadStatus));
        //    Lookup.Add(eQueryKey.ReferenceIDs, typeof(string));
        //    Lookup.Add(eQueryKey.ReplyTo, typeof(string));
        //    Lookup.Add(eQueryKey.ReplyToID, typeof(string));
        //    Lookup.Add(eQueryKey.SenderAddress, typeof(string));
        //    Lookup.Add(eQueryKey.SendStatus, typeof(EmailMessage.eSendStatus));
        //    Lookup.Add(eQueryKey.Status, typeof(EmailMessage.MessageStatus));
        //    Lookup.Add(eQueryKey.Subject, typeof(string));
        //    Lookup.Add(eQueryKey.To, typeof(string));
        //}
    }

    /// <summary>
    /// Encapsulates one unit of search for the engine to perform.
    /// This micro-package makes it convenient to connect the search engine to the UX
    /// </summary>
    public class EmailSearchAtom : INotifyPropertyChanged
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

        private string _DisplayName = string.Empty;
        public string DisplayName
        {
            get => _DisplayName;
            set => SetField(ref _DisplayName, value, nameof(DisplayName));
        }
        
        private QueryKey _QueryKey = new();
        public QueryKey QueryKey
        {
            get => _QueryKey;
            set => SetField(ref _QueryKey, value, nameof(QueryKey));
        }

        private SearchOperand _Operand = new();
        public SearchOperand Operand
        {
            get => _Operand;
            set => SetField(ref _Operand, value, nameof(Operand));
        }

        private string _StringValue = string.Empty;
        public string StringValue
        {
            get => _StringValue;
            set => SetField(ref _StringValue, value, nameof(StringValue));
        }

        private EmailAddress _EmailAddressValue = new();
        public EmailAddress EmailAddressValue
        {
            get => _EmailAddressValue;
            set => SetField(ref _EmailAddressValue, value, nameof(EmailAddressValue));
        }

        private DateTime _DateTimeValue = default;
        public DateTime DateTimeValue
        {
            get => _DateTimeValue;
            set => SetField(ref _DateTimeValue, value, nameof(DateTimeValue));
        }

        private Guid _GuidValue = Guid.NewGuid();
        public Guid GuidValue
        {
            get => _GuidValue;
            set => SetField(ref _GuidValue, value, nameof(GuidValue));
        }

        private EmailMessage.MessageOrigin _OriginValue = default;
        public EmailMessage.MessageOrigin OriginValue
        {
            get => _OriginValue;
            set => SetField(ref _OriginValue, value, nameof(OriginValue));
        }
        
        private EmailMessage.MessageStatus _MessageStatusValue = default;
        public EmailMessage.MessageStatus MessageStatusValue
        {
            get => _MessageStatusValue;
            set => SetField(ref _MessageStatusValue, value, nameof(MessageStatusValue));
        }

        private EmailMessage.eReadStatus _ReadStatusValue = default;
        public EmailMessage.eReadStatus ReadStatusValue
        {
            get => _ReadStatusValue;
            set => SetField(ref _ReadStatusValue, value, nameof(ReadStatusValue));
        }

        private EmailMessage.eSendStatus _SendStatusValue = default;
        public EmailMessage.eSendStatus SendStatusValue
        {
            get => _SendStatusValue;
            set => SetField(ref _SendStatusValue, value, nameof(SendStatusValue));
        }

        private PostOffice.eMailPriority _PriorityValue = PostOffice.eMailPriority.Normal;
        public PostOffice.eMailPriority PriorityValue
        {
            get => _PriorityValue;
            set => SetField(ref _PriorityValue, value, nameof(PriorityValue));
        }



        private ObservableCollection<MailboxListItem> _MailboxList = [];
        [SQLite.Ignore]
        public ObservableCollection<MailboxListItem> MailboxList
        {
            get => _MailboxList;
            set => SetField(ref _MailboxList, value, nameof(MailboxList));
        }

        /////////////////////////////
        #endregion Properties
        ///////////////////////////////////////////////////////////
        
        public EmailSearchAtom()
        {
            QueryKey.PropertyChanged += QueryKey_PropertyChanged;
            Operand.PropertyChanged += Operand_PropertyChanged;
            EmailAddressValue.PropertyChanged += EmailAddressValue_PropertyChanged;
            //PostOffice.Instance.Mailboxes.CollectionChanged += Mailboxes_CollectionChanged;
            RemakeMailboxList();
        }

        public void SelectAllMailboxes()
        {
            foreach(var mailbox in MailboxList)
            {
                mailbox.IsChecked = true;
            }
        }

        private void EmailAddressValue_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(EmailAddressValue));
        }

        private void Operand_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(Operand));
        }

        private void QueryKey_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(QueryKey));
        }

        private void Mailboxes_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RemakeMailboxList();
        }

        private void RemakeMailboxList()
        {
            MailboxList.Clear();
            foreach(Mailbox mailbox in PostOffice.Mailboxes)
            {
                MailboxList.Add(new(mailbox.Name, mailbox.ImageSource));
            }
        }
    }


    /// <summary>
    /// Helper class to populate the list of mailboxes to search
    /// </summary>
    public class MailboxListItem : INotifyPropertyChanged
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

        private bool _IsChecked = true;
        public bool IsChecked
        {
            get => _IsChecked;
            set => SetField(ref _IsChecked, value, nameof(IsChecked));
        }

        private string _Name = string.Empty;
        public string Name
        {
            get => _Name;
            set => SetField(ref _Name, value, nameof(Name));
        }

        private string _ImageSource = string.Empty;
        public string ImageSource
        {
            get => _ImageSource;
            set => SetField(ref _ImageSource, value, nameof(ImageSource));
        }

        /////////////////////////////
        #endregion Properties
        ///////////////////////////////////////////////////////////
         

        public MailboxListItem()
        {

        }

        public MailboxListItem(string name, string imageSource)
        {
            _Name = name;
            _ImageSource = imageSource;
        }
    }
}
