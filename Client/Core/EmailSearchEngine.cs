using System.Collections.ObjectModel;
using Eudora.Net.Data;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

namespace Eudora.Net.Core
{
    /// <summary>
    /// The search engine proper
    /// </summary>
    public static class EmailSearchEngine
    {
        ///////////////////////////////////////////////////////////
        #region Properties
        /////////////////////////////

        public static ObservableCollection<EmailSearch.QueryKey> QueryKeys { get; set; } = [];
        public static ObservableCollection<EmailSearch.SearchOperand> SearchOperands { get; set; } = [];
 
        /////////////////////////////
        #endregion Properties
        ///////////////////////////////////////////////////////////



        ///////////////////////////////////////////////////////////
        #region Interface
        /////////////////////////////

        public static void Startup()
        {
            // List of search operands
            SearchOperands.Add(new("Contains", EmailSearch.eSearchOperand.Contains));
            SearchOperands.Add(new("Does not contain", EmailSearch.eSearchOperand.DoesNotContain));
            SearchOperands.Add(new("Equals", EmailSearch.eSearchOperand.Equals));
            SearchOperands.Add(new("Does not equal", EmailSearch.eSearchOperand.NotEqual));
            SearchOperands.Add(new("Less than", EmailSearch.eSearchOperand.LessThan));
            SearchOperands.Add(new("Less than or equal", EmailSearch.eSearchOperand.LessThanOrEqual));
            SearchOperands.Add(new("Greater than or equal", EmailSearch.eSearchOperand.GreaterThanOrEqual));
            SearchOperands.Add(new("Greater than", EmailSearch.eSearchOperand.GreaterThan));

            // List of query keys
            QueryKeys.Add(new("From", EmailSearch.eQueryKey.From));
            //QueryKeys.Add(new("Sender address", EmailSearch.eQueryKey.SenderAddress));

            QueryKeys.Add(new("Subject", EmailSearch.eQueryKey.Subject));

            QueryKeys.Add(new("Date", EmailSearch.eQueryKey.Date));

            QueryKeys.Add(new("Priority", EmailSearch.eQueryKey.Priority));

            QueryKeys.Add(new("Body", EmailSearch.eQueryKey.Body));

            QueryKeys.Add(new("All Addressees", EmailSearch.eQueryKey.AllAddressees));
            QueryKeys.Add(new("Addresses To", EmailSearch.eQueryKey.To));
            QueryKeys.Add(new("Addresses BCC", EmailSearch.eQueryKey.BCC));
            QueryKeys.Add(new("Addresses CC", EmailSearch.eQueryKey.CC));

            QueryKeys.Add(new("Label", EmailSearch.eQueryKey.LabelName));


            //QueryKeys.Add(new("Internal ID", EmailSearch.eQueryKey.InternalID));
            //QueryKeys.Add(new("Mailbox", EmailSearch.eQueryKey.MailboxName));
            //QueryKeys.Add(new("Message ID", EmailSearch.eQueryKey.MessageID));
            //QueryKeys.Add(new("Origin", EmailSearch.eQueryKey.Origin));
            //QueryKeys.Add(new("Personality", EmailSearch.eQueryKey.PersonalityID));
            //QueryKeys.Add(new("Read Status", EmailSearch.eQueryKey.ReadStatus));
            //QueryKeys.Add(new("Reference IDs", EmailSearch.eQueryKey.ReferenceIDs));
            //QueryKeys.Add(new("Reply to", EmailSearch.eQueryKey.ReplyTo));
            //QueryKeys.Add(new("Reply-to ID", EmailSearch.eQueryKey.ReplyToID));
            //QueryKeys.Add(new("Send status", EmailSearch.eQueryKey.SendStatus));
            //QueryKeys.Add(new("Status", EmailSearch.eQueryKey.Status));
        }

        public static void Shutdown()
        {

        }

        public static async Task<bool> SearchOneMessage(EmailSearch.EmailSearchAtom search, EmailMessage message)
        {
            bool result = false;
            var key = search.QueryKey.Key;
            var operand = search.Operand.Operand;

            switch (key)
            {
                case EmailSearch.eQueryKey.AllAddressees:
                    result = await SearchAllAddressees(message, search.StringValue, operand);
                    break;
                case EmailSearch.eQueryKey.BCC:
                    result = await SearchAddressesBCC(message, search.StringValue, operand);
                    break;
                case EmailSearch.eQueryKey.Body:
                    result = await SearchBody(message, search.StringValue, operand);
                    break;
                case EmailSearch.eQueryKey.CC:
                    result = await SearchAddressesCC(message, search.StringValue, operand);
                    break;
                case EmailSearch.eQueryKey.Date:
                    result = await SearchDate(message, search.DateTimeValue, operand);
                    break;
                case EmailSearch.eQueryKey.From:
                    result = await SearchAllFrom(message, search.StringValue, operand);
                    break;
                case EmailSearch.eQueryKey.InternalID:
                    result = await SearchInternalId(message, search.GuidValue, operand);
                    break;
                case EmailSearch.eQueryKey.LabelName:
                    result = await SearchLabelName(message, search.StringValue, operand);
                    break;
                case EmailSearch.eQueryKey.MailboxName:
                    result = await SearchMailboxName(message, search.StringValue, operand);
                    break;
                case EmailSearch.eQueryKey.MessageID:
                    result = await SearchMessageId(message, search.StringValue, operand);
                    break;
                case EmailSearch.eQueryKey.Origin:
                    result = await SearchMessageOrigin(message, search.OriginValue, operand);
                    break;
                case EmailSearch.eQueryKey.PersonalityID:
                    result = await SearchPersonalityId(message, search.GuidValue, operand);
                    break;
                case EmailSearch.eQueryKey.Priority:
                    result = await SearchPriority(message, search.PriorityValue, operand);
                    break;
                case EmailSearch.eQueryKey.ReadStatus:
                    result = await SearchReadStatus(message, search.ReadStatusValue, operand);
                    break;
                case EmailSearch.eQueryKey.ReferenceIDs:
                    result = await SearchReferenceIds(message, search.StringValue, operand);
                    break;
                case EmailSearch.eQueryKey.ReplyTo:
                    result = await SearchReplyTo(message, search.StringValue, operand);
                    break;
                case EmailSearch.eQueryKey.ReplyToID:
                    result = await SearchReplyToId(message, search.StringValue, operand);
                    break;
                case EmailSearch.eQueryKey.SenderAddress:
                    result = await SearchSenderAddress(message, search.StringValue, operand);
                    break;
                case EmailSearch.eQueryKey.SendStatus:
                    result = await SearchSendStatus(message, search.SendStatusValue, operand);
                    break;
                case EmailSearch.eQueryKey.Status:
                    result = await SearchMessageStatus(message, search.MessageStatusValue, operand);
                    break;
                case EmailSearch.eQueryKey.Subject:
                    result = await SearchSubject(message, search.StringValue, operand);
                    break;
                case EmailSearch.eQueryKey.To:
                    result = await SearchAddressesTo(message, search.StringValue, operand);
                    break;
            }
            return result;
        }

        public static async Task<ObservableCollection<EmailMessage>> ExecuteSearch(EmailSearch.EmailSearchAtom search)
        {
            ObservableCollection<EmailMessage> results = [];

            foreach (var mailboxItem in search.MailboxList)
            {
                if (!mailboxItem.IsChecked) continue;
                var mailbox = PostOffice.GetMailboxByName(mailboxItem.Name);
                if (mailbox is null) continue;
                foreach (EmailMessage message in mailbox.Messages)
                {
                    bool result = await SearchOneMessage(search, message);
                    if (result is true)
                    {
                        results.Add(message);
                    }
                }
          }

            return results;
        }


        /////////////////////////////
        #endregion Interface
        ///////////////////////////////////////////////////////////




        ///////////////////////////////////////////////////////////
        #region Internal
        /////////////////////////////
        
        /// <summary>
        /// Pre-filters operand vs data type such that only valid operands need to be
        /// handled in each field search function
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="operand"></param>
        /// <returns></returns>
        private static bool ValidateOperand<T>(EmailSearch.eSearchOperand operand)
        {
            switch(operand)
            {
                case EmailSearch.eSearchOperand.Contains:
                    if (typeof(T) == typeof(string)) return true;
                    if (typeof(T) == typeof(EmailAddress)) return true;
                    break;
                case EmailSearch.eSearchOperand.DoesNotContain:
                    if (typeof(T) == typeof(string)) return true;
                    if (typeof(T) == typeof(EmailAddress)) return true;
                    break;
                case EmailSearch.eSearchOperand.Equals:
                    if (typeof(T) == typeof(string)) return true;
                    if (typeof(T) == typeof(EmailAddress)) return true;
                    if (typeof(T) == typeof(DateTime)) return true;
                    if (typeof(T) == typeof(Guid)) return true;
                    if (typeof(T) == typeof(Enum)) return true;
                    break;
                case EmailSearch.eSearchOperand.NotEqual:
                    if (typeof(T) == typeof(string)) return true;
                    if (typeof(T) == typeof(EmailAddress)) return true;
                    if (typeof(T) == typeof(DateTime)) return true;
                    if (typeof(T) == typeof(Guid)) return true;
                    if (typeof(T) == typeof(Enum)) return true;
                    break;
                case EmailSearch.eSearchOperand.LessThan:
                    if (typeof(T) == typeof(DateTime)) return true;
                    break;
                case EmailSearch.eSearchOperand.LessThanOrEqual:
                    if (typeof(T) == typeof(DateTime)) return true;
                    break;
                case EmailSearch.eSearchOperand.GreaterThanOrEqual:
                    if (typeof(T) == typeof(DateTime)) return true;
                    break;
                case EmailSearch.eSearchOperand.GreaterThan:
                    if (typeof(T) == typeof(DateTime)) return true;
                    break;
            }

            return false;
        }

        private static async Task<bool> SearchInternalId(EmailMessage message, Guid value, EmailSearch.eSearchOperand operand)
        {
            if(!ValidateOperand<Guid>(operand)) return false;

            switch(operand)
            {
                case EmailSearch.eSearchOperand.Equals:
                    if(message.InternalId == value) return true;
                    break;
                case EmailSearch.eSearchOperand.NotEqual:
                    if(message.InternalId  != value) return true;
                    break;
            }

            return false;
        }

        private static async Task<bool> SearchMessageId(EmailMessage message, string value, EmailSearch.eSearchOperand operand)
        {
            if (!ValidateOperand<string>(operand)) return false;

            switch (operand)
            {
                case EmailSearch.eSearchOperand.Equals:
                    if (message.MessageId == value) return true;
                    break;
                case EmailSearch.eSearchOperand.NotEqual:
                    if (message.MessageId != value) return true;
                    break;
            }

            return false;
        }

        private static async Task<bool> SearchReplyToId(EmailMessage message, string value, EmailSearch.eSearchOperand operand)
        {
            if (!ValidateOperand<string>(operand)) return false;

            switch (operand)
            {
                case EmailSearch.eSearchOperand.Equals:
                    if (message.InReplyToId == value) return true;
                    break;
                case EmailSearch.eSearchOperand.NotEqual:
                    if (message.InReplyToId != value) return true;
                    break;
            }

            return false;
        }

        private static async Task<bool> SearchReferenceIds(EmailMessage message, string value, EmailSearch.eSearchOperand operand)
        {
            if (!ValidateOperand<string>(operand)) return false;

            foreach(var id in message.ReferenceIDs)
            {
                if (operand == EmailSearch.eSearchOperand.Equals && id == value) return true;
                else if (operand == EmailSearch.eSearchOperand.NotEqual && id != value) return true;
            }

            return false;
        }

        private static async Task<bool> SearchReplyTo(EmailMessage message, string value, EmailSearch.eSearchOperand operand)
        {
            if (!ValidateOperand<EmailAddress>(operand)) return false;

            switch(operand)
            {
                case EmailSearch.eSearchOperand.Contains:
                    if (message.ReplyTo.Name.Contains(value)) return true;
                    if (message.ReplyTo.Address.Contains(value)) return true;
                    break;
                case EmailSearch.eSearchOperand.DoesNotContain:
                    if (!message.ReplyTo.Name.Contains(value)) return true;
                    if (!message.ReplyTo.Address.Contains(value)) return true;
                    break;
                case EmailSearch.eSearchOperand.Equals:
                    if (message.ReplyTo.Name.Equals(value, StringComparison.CurrentCultureIgnoreCase)) return true;
                    if (message.ReplyTo.Address.Equals(value, StringComparison.CurrentCultureIgnoreCase)) return true;
                    break; 
                case EmailSearch.eSearchOperand.NotEqual:
                    if (!message.ReplyTo.Name.Equals(value, StringComparison.CurrentCultureIgnoreCase)) return true;
                    if (!message.ReplyTo.Address.Equals(value, StringComparison.CurrentCultureIgnoreCase)) return true;
                    break;
            }

            return false;
        }

        private static async Task<bool> SearchMailboxName(EmailMessage message, string value, EmailSearch.eSearchOperand operand)
        {
            if (!ValidateOperand<string>(operand)) return false;

            switch(operand)
            {
                case EmailSearch.eSearchOperand.Equals:
                    if (message.MailboxName.Equals(value, StringComparison.CurrentCultureIgnoreCase)) return true;
                    break;

                case EmailSearch.eSearchOperand.NotEqual:
                    if (!message.MailboxName.Equals(value, StringComparison.CurrentCultureIgnoreCase)) return true;
                    break;
            }

            return false;
        }

        private static async Task<bool> SearchPersonalityId(EmailMessage message, Guid value, EmailSearch.eSearchOperand operand)
        {
            if (!ValidateOperand<Guid>(operand)) return false;

            switch(operand)
            {
                case EmailSearch.eSearchOperand.Equals:
                    if(message.PersonalityID == value) return true;
                    break;

                case EmailSearch.eSearchOperand.NotEqual:
                    if (message.PersonalityID != value) return true;
                    break;
            }

            return false;
        }

        private static async Task<bool> SearchLabelName(EmailMessage message, string value, EmailSearch.eSearchOperand operand)
        {
            if (!ValidateOperand<string>(operand)) return false;

            switch(operand)
            {
                case EmailSearch.eSearchOperand.Equals:
                    if (message.LabelName.Equals(value, StringComparison.CurrentCultureIgnoreCase)) return true;
                    break;

                case EmailSearch.eSearchOperand.NotEqual:
                    if (!message.LabelName.Equals(value, StringComparison.CurrentCultureIgnoreCase)) return true;
                    break;
            }

            return false;
        }

        private static async Task<bool> SearchMessageStatus(EmailMessage message, EmailMessage.MessageStatus value, EmailSearch.eSearchOperand operand)
        {
            if (!ValidateOperand<Enum>(operand)) return false;

            switch(operand)
            {
                case EmailSearch.eSearchOperand.Equals:
                    if (message.Status == value) return true;
                    break;

                case EmailSearch.eSearchOperand.NotEqual:
                    if (message.Status != value) return true;
                    break;
            }

            return false;
        }

        private static async Task<bool> SearchReadStatus(EmailMessage message, EmailMessage.eReadStatus value, EmailSearch.eSearchOperand operand)
        {
            if (!ValidateOperand<Enum>(operand)) return false;

            switch (operand)
            {
                case EmailSearch.eSearchOperand.Equals:
                    if (message.ReadStatus == value) return true;
                    break;

                case EmailSearch.eSearchOperand.NotEqual:
                    if (message.ReadStatus != value) return true;
                    break;
            }

            return false;
        }

        private static async Task<bool> SearchSendStatus(EmailMessage message, EmailMessage.eSendStatus value, EmailSearch.eSearchOperand operand)
        {
            if (!ValidateOperand<Enum>(operand)) return false;

            switch (operand)
            {
                case EmailSearch.eSearchOperand.Equals:
                    if (message.SendStatus == value) return true;
                    break;

                case EmailSearch.eSearchOperand.NotEqual:
                    if (message.SendStatus != value) return true;
                    break;
            }

            return false;
        }

        private static async Task<bool> SearchMessageOrigin(EmailMessage message, EmailMessage.MessageOrigin value, EmailSearch.eSearchOperand operand)
        {
            if (!ValidateOperand<Enum>(operand)) return false;

            switch (operand)
            {
                case EmailSearch.eSearchOperand.Equals:
                    if (message.Origin == value) return true;
                    break;

                case EmailSearch.eSearchOperand.NotEqual:
                    if (message.Origin != value) return true;
                    break;
            }

            return false;
        }

        private static async Task<bool> SearchPriority(EmailMessage message, PostOffice.eMailPriority value, EmailSearch.eSearchOperand operand)
        {
            // Priority is an enum, however it's also a special case where <> can apply
            //if (!ValidateOperand<Enum>(operand)) return false;

            switch(operand)
            {
                case EmailSearch.eSearchOperand.Equals:
                    if(message.Priority == value) return true;
                    break;

                case EmailSearch.eSearchOperand.NotEqual:
                    if (message.Priority != value) return true;
                    break;

                case EmailSearch.eSearchOperand.LessThan:
                    if (message.Priority < value) return true;
                    break;

                case EmailSearch.eSearchOperand.LessThanOrEqual:
                    if (message.Priority <= value) return true;
                    break;

                case EmailSearch.eSearchOperand.GreaterThanOrEqual:
                    if (message.Priority >= value) return true;
                    break;

                case EmailSearch.eSearchOperand.GreaterThan:
                    if (message.Priority > value) return true;
                    break;
            }

            return false;
        }

        private static async Task<bool> SearchDate(EmailMessage message,  DateTime value, EmailSearch.eSearchOperand operand)
        {
            if (!ValidateOperand<DateTime>(operand)) return false;

            switch(operand)
            {
                case EmailSearch.eSearchOperand.Equals:
                    if(message.Date == value) return true;
                    break;

                case EmailSearch.eSearchOperand.NotEqual:
                    if (message.Date != value) return true;
                    break;

                case EmailSearch.eSearchOperand.LessThan:
                    if (message.Date < value) return true;
                    break;

                case EmailSearch.eSearchOperand.LessThanOrEqual:
                    if (message.Date <= value) return true;
                    break;

                case EmailSearch.eSearchOperand.GreaterThanOrEqual:
                    if (message.Date >= value) return true;
                    break;

                case EmailSearch.eSearchOperand.GreaterThan:
                    if (message.Date > value) return true;
                    break;
            }

            return false;
        }

        private static async Task<bool> SearchSenderAddress(EmailMessage message, string value, EmailSearch.eSearchOperand operand)
        {
            if (!ValidateOperand<EmailAddress>(operand)) return false;

            switch(operand)
            {
                case EmailSearch.eSearchOperand.Equals:
                    if (message.SenderAddress.Name.Equals(value, StringComparison.CurrentCultureIgnoreCase)) return true;
                    if (message.SenderAddress.Address.Equals(value, StringComparison.CurrentCultureIgnoreCase)) return true;
                    break;

                case EmailSearch.eSearchOperand.NotEqual:
                    if (!message.SenderAddress.Name.Equals(value, StringComparison.CurrentCultureIgnoreCase)) return true;
                    if (!message.SenderAddress.Address.Equals(value, StringComparison.CurrentCultureIgnoreCase)) return true;
                    break;

                case EmailSearch.eSearchOperand.Contains:
                    if (message.SenderAddress.Name.Contains(value, StringComparison.CurrentCultureIgnoreCase)) return true;
                    if (message.SenderAddress.Address.Contains(value, StringComparison.CurrentCultureIgnoreCase)) return true;
                    break;

                case EmailSearch.eSearchOperand.DoesNotContain:
                    if (!message.SenderAddress.Name.Contains(value, StringComparison.CurrentCultureIgnoreCase)) return true;
                    if (!message.SenderAddress.Address.Contains(value, StringComparison.CurrentCultureIgnoreCase)) return true;
                    break;
            }

            return false;
        }

        private static async Task<bool> SearchAddressesFrom(EmailMessage message, string value, EmailSearch.eSearchOperand operand)
        {
            if (!ValidateOperand<EmailAddress>(operand)) return false;

            foreach(var emailAddress in message.Addresses_From)
            {
                switch (operand)
                {
                    case EmailSearch.eSearchOperand.Equals:
                        if (emailAddress.Name.Equals(value, StringComparison.CurrentCultureIgnoreCase)) return true;
                        if (emailAddress.Address.Equals(value, StringComparison.CurrentCultureIgnoreCase)) return true;
                        break;

                    case EmailSearch.eSearchOperand.NotEqual:
                        if (!emailAddress.Name.Equals(value, StringComparison.CurrentCultureIgnoreCase)) return true;
                        if (!emailAddress.Address.Equals(value, StringComparison.CurrentCultureIgnoreCase)) return true;
                        break;

                    case EmailSearch.eSearchOperand.Contains:
                        if (emailAddress.Name.Contains(value, StringComparison.CurrentCultureIgnoreCase)) return true;
                        if (emailAddress.Address.Contains(value, StringComparison.CurrentCultureIgnoreCase)) return true;
                        break;

                    case EmailSearch.eSearchOperand.DoesNotContain:
                        if (!emailAddress.Name.Contains(value, StringComparison.CurrentCultureIgnoreCase)) return true;
                        if (!emailAddress.Address.Contains(value, StringComparison.CurrentCultureIgnoreCase)) return true;
                        break;
                }
            }

            return false;
        }

        private static async Task<bool> SearchAllFrom(EmailMessage message, string value, EmailSearch.eSearchOperand operand)
        {
            bool result = false;

            result = await SearchSenderAddress(message, value, operand);
            if (result) return result;

            result = await SearchAddressesFrom(message, value, operand);
            if (result) return result;

            return result;
        }

        private static async Task<bool> SearchAddressesTo(EmailMessage message, string value, EmailSearch.eSearchOperand operand)
        {
            if (!ValidateOperand<EmailAddress>(operand)) return false;

            foreach (var emailAddress in message.Addresses_To)
            {
                switch (operand)
                {
                    case EmailSearch.eSearchOperand.Equals:
                        if (emailAddress.Name.Equals(value, StringComparison.CurrentCultureIgnoreCase)) return true;
                        if (emailAddress.Address.Equals(value, StringComparison.CurrentCultureIgnoreCase)) return true;
                        break;

                    case EmailSearch.eSearchOperand.NotEqual:
                        if (!emailAddress.Name.Equals(value, StringComparison.CurrentCultureIgnoreCase)) return true;
                        if (!emailAddress.Address.Equals(value, StringComparison.CurrentCultureIgnoreCase)) return true;
                        break;

                    case EmailSearch.eSearchOperand.Contains:
                        if (emailAddress.Name.Contains(value, StringComparison.CurrentCultureIgnoreCase)) return true;
                        if (emailAddress.Address.Contains(value, StringComparison.CurrentCultureIgnoreCase)) return true;
                        break;

                    case EmailSearch.eSearchOperand.DoesNotContain:
                        if (!emailAddress.Name.Contains(value, StringComparison.CurrentCultureIgnoreCase)) return true;
                        if (!emailAddress.Address.Contains(value, StringComparison.CurrentCultureIgnoreCase)) return true;
                        break;
                }
            }

            return false;
        }

        private static async Task<bool> SearchAddressesCC(EmailMessage message, string value, EmailSearch.eSearchOperand operand)
        {
            if (!ValidateOperand<EmailAddress>(operand)) return false;

            foreach (var emailAddress in message.Addresses_CC)
            {
                switch (operand)
                {
                    case EmailSearch.eSearchOperand.Equals:
                        if (emailAddress.Name.Equals(value, StringComparison.CurrentCultureIgnoreCase)) return true;
                        if (emailAddress.Address.Equals(value, StringComparison.CurrentCultureIgnoreCase)) return true;
                        break;

                    case EmailSearch.eSearchOperand.NotEqual:
                        if (!emailAddress.Name.Equals(value, StringComparison.CurrentCultureIgnoreCase)) return true;
                        if (!emailAddress.Address.Equals(value, StringComparison.CurrentCultureIgnoreCase)) return true;
                        break;

                    case EmailSearch.eSearchOperand.Contains:
                        if (emailAddress.Name.Contains(value, StringComparison.CurrentCultureIgnoreCase)) return true;
                        if (emailAddress.Address.Contains(value, StringComparison.CurrentCultureIgnoreCase)) return true;
                        break;

                    case EmailSearch.eSearchOperand.DoesNotContain:
                        if (!emailAddress.Name.Contains(value, StringComparison.CurrentCultureIgnoreCase)) return true;
                        if (!emailAddress.Address.Contains(value, StringComparison.CurrentCultureIgnoreCase)) return true;
                        break;
                }
            }

            return false;
        }

        private static async Task<bool> SearchAddressesBCC(EmailMessage message, string value, EmailSearch.eSearchOperand operand)
        {
            if (!ValidateOperand<EmailAddress>(operand)) return false;

            foreach (var emailAddress in message.Addresses_BCC)
            {
                switch (operand)
                {
                    case EmailSearch.eSearchOperand.Equals:
                        if (emailAddress.Name.Equals(value, StringComparison.CurrentCultureIgnoreCase)) return true;
                        if (emailAddress.Address.Equals(value, StringComparison.CurrentCultureIgnoreCase)) return true;
                        break;

                    case EmailSearch.eSearchOperand.NotEqual:
                        if (!emailAddress.Name.Equals(value, StringComparison.CurrentCultureIgnoreCase)) return true;
                        if (!emailAddress.Address.Equals(value, StringComparison.CurrentCultureIgnoreCase)) return true;
                        break;

                    case EmailSearch.eSearchOperand.Contains:
                        if (emailAddress.Name.Contains(value, StringComparison.CurrentCultureIgnoreCase)) return true;
                        if (emailAddress.Address.Contains(value, StringComparison.CurrentCultureIgnoreCase)) return true;
                        break;

                    case EmailSearch.eSearchOperand.DoesNotContain:
                        if (!emailAddress.Name.Contains(value, StringComparison.CurrentCultureIgnoreCase)) return true;
                        if (!emailAddress.Address.Contains(value, StringComparison.CurrentCultureIgnoreCase)) return true;
                        break;
                }
            }

            return false;
        }

        private static async Task<bool> SearchAllAddressees(EmailMessage message, string value, EmailSearch.eSearchOperand operand)
        {
            bool result = false;

            result = await SearchAddressesTo(message, value, operand);
            if(result) return result;

            result = await SearchAddressesCC(message, value, operand);
            if (result) return result;

            result = await SearchAddressesBCC(message, value, operand);
            if (result) return result;

            return result;
        }

        private static async Task<bool> SearchSubject(EmailMessage message, string value, EmailSearch.eSearchOperand operand)
        {
            if (!ValidateOperand<string>(operand)) return false;

            switch(operand)
            {
                case EmailSearch.eSearchOperand.Contains:
                    if (message.Subject.Contains(value, StringComparison.CurrentCultureIgnoreCase)) return true;
                    break;

                case EmailSearch.eSearchOperand.DoesNotContain:
                    if (!message.Subject.Contains(value, StringComparison.CurrentCultureIgnoreCase)) return true;
                    break;

                case EmailSearch.eSearchOperand.Equals:
                    if (message.Subject.Equals(value, StringComparison.CurrentCultureIgnoreCase)) return true;
                    break;

                case EmailSearch.eSearchOperand.NotEqual:
                    if (!message.Subject.Equals(value, StringComparison.CurrentCultureIgnoreCase)) return true;
                    break;
            }

            return false;
        }

        private static async Task<bool> SearchBody(EmailMessage message, string value, EmailSearch.eSearchOperand operand)
        {
            if (!ValidateOperand<string>(operand)) return false;

            switch(operand)
            {
                case EmailSearch.eSearchOperand.Contains:
                    if (message.Body.Contains(value, StringComparison.CurrentCultureIgnoreCase)) return true;
                    break;

                case EmailSearch.eSearchOperand.DoesNotContain:
                    if (!message.Body.Contains(value, StringComparison.CurrentCultureIgnoreCase)) return true;
                    break;
            }

            return false;
        }

        private static async Task<bool> HasAttachments(EmailMessage message)
        {
            return message.Attachments.Count > 0;
        }

        private static async Task<bool> HasAttachmentNamed(EmailMessage message, string value)
        {
            foreach(EmailAttachment attachment in message.Attachments)
            {
                if(attachment.Name.Equals(value, StringComparison.CurrentCultureIgnoreCase)) return true;
            }
            return false;
        }


        /////////////////////////////
        #endregion Internal
        ///////////////////////////////////////////////////////////
    }
}

#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
