using Eudora.Net.Core;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;

namespace Eudora.Net.Data
{
    internal class EmailMessage_DB
    {
        public Guid InternalId { get; set; }
        public string MessageId { get; set; }
        public string InReplyToId { get; set; }

        public string ReferenceIDs { get; set; }

        public string ReplyTo { get; set; }

        public string MailboxName { get; set; }
        public Guid PersonalityID { get; set; }
        public string LabelName { get; set; }

        public EmailEnums.MessageStatus Status { get; set; }
        public EmailEnums.eReadStatus ReadStatus { get; set; }
        public EmailEnums.eSendStatus SendStatus { get; set; }
        public EmailEnums.MessageOrigin Origin { get; set; }
        
        public PostOffice.eMailPriority Priority { get; set; }
        public DateTime Date { get; set; }
        public string SenderAddress { get; set; }

        public string Addresses_From { get; set; }

        public string Addresses_To { get; set; }

        public string Addresses_CC { get; set; }

        public string Addresses_BCC { get; set; }

        public string Subject { get; set; }
        public string Body { get; set; }

        public string Attachments { get; set; }

        public string InlineImages { get; set; }

        public PostOffice.eMailThreadType MessageCategory { get; set; }
    }
}
