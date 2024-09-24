using Eudora.Net.GUI;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Eudora.Net.Core;

namespace Eudora.Net.Data
{
    /// <summary>
    /// An ecapsulation of the action(s) to be taken upon a filtering match.
    /// Each Filter has a list of actions, all of which will be applied to
    /// the matching email.
    /// </summary>
    public class EmailFilterAction : INotifyPropertyChanged
    {
        ///////////////////////////////////////////////////////////
        #region INotifyPropertyChanged
        /////////////////////////////

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

        /////////////////////////////
        #endregion INotifyPropertyChanged
        ///////////////////////////////////////////////////////////



        ///////////////////////////////////////////////////////////
        #region Properties
        /////////////////////////////

        public enum eFilterActionKey
        {
            Move,
            Copy,
            Forward,
            Reply,
            Priority,
            Label,
            Show,
            Print,
            Sound,
            Notify,
            SkipRemaining
        }

        private string _DisplayName = string.Empty;
        public string DisplayName
        {
            get => _DisplayName;
            set => SetField(ref _DisplayName, value, nameof(DisplayName));
        }

        private eFilterActionKey _ActionKey = default;
        public eFilterActionKey ActionKey
        {
            get => _ActionKey;
            set => SetField(ref _ActionKey, value, nameof(ActionKey));
        }

        private string _MailboxName = string.Empty;
        public string MailboxName
        {
            get => _MailboxName;
            set => SetField(ref _MailboxName, value, nameof(MailboxName));
        }

        private PostOffice.eMailPriority _Priority = PostOffice.eMailPriority.Normal;
        public PostOffice.eMailPriority Priority
        {
            get => _Priority;
            set => SetField(ref _Priority, value, nameof(Priority));
        }

        private string _Notification = string.Empty;
        public string Notification
        {
            get => _Notification;
            set => SetField(ref _Notification, value, nameof(Notification));
        }

        private string _StringValue = string.Empty;
        public string StringValue
        {
            get => _StringValue;
            set => SetField(ref _StringValue, value, nameof(StringValue));
        }

        private Guid _PersonalityValue = Guid.Empty;
        public Guid PersonalityValue
        {
            get => _PersonalityValue;
            set => SetField(ref _PersonalityValue, value, nameof(PersonalityValue));
        }

        /////////////////////////////
        #endregion Properties
        ///////////////////////////////////////////////////////////

        public EmailFilterAction()
        {
            CommonCtor();
        }

        public EmailFilterAction(eFilterActionKey key, string name)
        {
            ActionKey = key;
            DisplayName = name;
            CommonCtor();
        }

        private void CommonCtor()
        {
            
        }

        ///////////////////////////////////////////////////////////
        #region Interface
        /////////////////////////////

        public void Act(EmailMessage message)
        {
            switch(ActionKey)
            {
                case eFilterActionKey.Copy:
                    CopyMessage(message);
                    break;
                case eFilterActionKey.Forward:
                    ForwardMessage(message);
                    break;
                case eFilterActionKey.Label:
                    LabelMessage(message);
                    break;
                case eFilterActionKey.Move:
                    MoveMessage(message);
                    break;
                case eFilterActionKey.Notify:
                    Notify(message);
                    break;
                case eFilterActionKey.Print:
                    PrintMessage(message);
                    break;
                case eFilterActionKey.Priority:
                    SetPriority(message);
                    break;
                case eFilterActionKey.Reply:
                    ReplyToMessage(message);
                    break;
                case eFilterActionKey.Show:
                    ShowMessage(message);
                    break;
                case eFilterActionKey.SkipRemaining:
                    SkipRemainingFilters();
                    break;
                case eFilterActionKey.Sound:
                    PlaySound();
                    break;
            }
        }

        /////////////////////////////
        #endregion Interface
        ///////////////////////////////////////////////////////////



        ///////////////////////////////////////////////////////////
        #region Internal
        /////////////////////////////

        private void MoveMessage(EmailMessage message)
        {
            PostOffice.MoveMessage(message, MailboxName);
        }

        private void CopyMessage(EmailMessage message)
        {
            PostOffice.CopyMessage(message, MailboxName);
        }

        private void ForwardMessage(EmailMessage message)
        {
            var msg = PostOffice.CreateMessage_Forward(message);
            MainWindow.Instance?.ShowMailMessage(msg);
        }

        private void ReplyToMessage(EmailMessage message)
        {
            var msg = PostOffice.CreateMessage_Reply(message);
            MainWindow.Instance?.ShowMailMessage(msg);
        }

        private void SetPriority(EmailMessage message)
        {
            message.Priority = Priority;
        }

        private void LabelMessage(EmailMessage message)
        {
            message.LabelName = StringValue;
        }

        private void ShowMessage(EmailMessage message)
        {
            MainWindow.Instance?.ShowMailMessage(message);
        }

        private void PrintMessage(EmailMessage message)
        {

        }

        private void PlaySound()
        {
            GSoundPlayer.Instance.Play(StringValue);
        }

        private void Notify(EmailMessage message)
        {
            Logger.Notify(Notification);
        }

        private void SkipRemainingFilters()
        {

        }        

        /////////////////////////////
        #endregion Internal
        ///////////////////////////////////////////////////////////
    }

    public static class EmailFilterActions
    {
        public static ObservableCollection<EmailFilterAction> Actions = [];

        static EmailFilterActions()
        {
            Actions.Add(new(EmailFilterAction.eFilterActionKey.Copy, "Copy message"));
            Actions.Add(new(EmailFilterAction.eFilterActionKey.Move, "Move message"));

            Actions.Add(new(EmailFilterAction.eFilterActionKey.Forward, "Forward message"));
            Actions.Add(new(EmailFilterAction.eFilterActionKey.Reply, "Reply to message"));

            Actions.Add(new(EmailFilterAction.eFilterActionKey.Show, "View message"));

            Actions.Add(new(EmailFilterAction.eFilterActionKey.Priority, "Set message priority"));
            Actions.Add(new(EmailFilterAction.eFilterActionKey.Label, "Label message"));

            Actions.Add(new(EmailFilterAction.eFilterActionKey.Notify, "Notify me"));
            Actions.Add(new(EmailFilterAction.eFilterActionKey.Sound, "Play a sound"));

            Actions.Add(new(EmailFilterAction.eFilterActionKey.Print, "Print message"));

            Actions.Add(new(EmailFilterAction.eFilterActionKey.SkipRemaining, "Skip remaining filter actions"));
        }
    }
}
