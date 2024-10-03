using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Eudora.Net.Data
{
    /// <summary>
    /// Converts between EmailMessage and EMailMessage_DB.
    /// Invoked by the DatastoreConversion template instance in Mailbox,
    /// to perform automatic conversion as necessary.
    /// </summary>
    internal class DbConverter_EmailMessage : IDbConverter<EmailMessage, EmailMessage_DB>
    {
        public DbConverter_EmailMessage() 
        { }

        public EmailMessage? Convert(EmailMessage_DB msgIn)
        {
            EmailMessage msg = new()
            {
                InternalId = msgIn.InternalId,
                MessageId = msgIn.MessageId,
                InReplyToId = msgIn.InReplyToId,
                MailboxName = msgIn.MailboxName,
                PersonalityID = msgIn.PersonalityID,
                LabelName = msgIn.LabelName,
                Status = msgIn.Status,
                ReadStatus = msgIn.ReadStatus,
                SendStatus = msgIn.SendStatus,
                Origin = msgIn.Origin,
                Priority = msgIn.Priority,
                Date = msgIn.Date,
                Subject = msgIn.Subject,
                Body = msgIn.Body,
                MessageCategory = msgIn.MessageCategory
            };

            msg.ReplyTo = JsonSerializer.Deserialize<EmailAddress>(msgIn.ReplyTo) ?? new();

            msg.SenderAddress = JsonSerializer.Deserialize<EmailAddress>(msgIn.SenderAddress) ?? new();

            msg.ReferenceIDs = JsonSerializer.Deserialize<ObservableCollection<string>>(msgIn.ReferenceIDs) ?? [];

            msg.Addresses_From = JsonSerializer.Deserialize<ObservableCollection<EmailAddress>>(msgIn.Addresses_From) ?? [];

            msg.Addresses_To = JsonSerializer.Deserialize<ObservableCollection<EmailAddress>>(msgIn.Addresses_To) ?? [];

            msg.Addresses_CC = JsonSerializer.Deserialize<ObservableCollection<EmailAddress>>(msgIn.Addresses_CC) ?? [];

            msg.Addresses_BCC = JsonSerializer.Deserialize<ObservableCollection<EmailAddress>>(msgIn.Addresses_BCC) ?? [];

            msg.Attachments = JsonSerializer.Deserialize<ObservableCollection<EmailAttachment>>(msgIn.Attachments) ?? [];

            msg.InlineImages = JsonSerializer.Deserialize<ObservableCollection<EmbeddedImage>>(msgIn.InlineImages) ?? [];

            return msg;
        }

        public EmailMessage_DB? Convert(EmailMessage msgIn)
        {
            EmailMessage_DB msg = new()
            {
                InternalId = msgIn.InternalId,
                MessageId = msgIn.MessageId,
                InReplyToId = msgIn.InReplyToId,
                MailboxName = msgIn.MailboxName,
                PersonalityID = msgIn.PersonalityID,
                LabelName = msgIn.LabelName,
                Status = msgIn.Status,
                ReadStatus = msgIn.ReadStatus,
                SendStatus = msgIn.SendStatus,
                Origin = msgIn.Origin,
                Priority = msgIn.Priority,
                Date = msgIn.Date,
                Subject = msgIn.Subject,
                Body = msgIn.Body,
                MessageCategory = msgIn.MessageCategory
            };

            msg.ReplyTo = JsonSerializer.Serialize<EmailAddress>(msgIn.ReplyTo);

            msg.SenderAddress = JsonSerializer.Serialize<EmailAddress>(msgIn.SenderAddress);

            msg.ReferenceIDs = JsonSerializer.Serialize<ObservableCollection<string>>(msgIn.ReferenceIDs);

            msg.Addresses_From = JsonSerializer.Serialize<ObservableCollection<EmailAddress>>(msgIn.Addresses_From);

            msg.Addresses_To = JsonSerializer.Serialize<ObservableCollection<EmailAddress>>(msgIn.Addresses_To);

            msg.Addresses_CC = JsonSerializer.Serialize<ObservableCollection<EmailAddress>>(msgIn.Addresses_CC);

            msg.Addresses_BCC = JsonSerializer.Serialize<ObservableCollection<EmailAddress>>(msgIn.Addresses_BCC);

            msg.Attachments = JsonSerializer.Serialize<ObservableCollection<EmailAttachment>>(msgIn.Attachments);

            msg.InlineImages = JsonSerializer.Serialize<ObservableCollection<EmbeddedImage>>(msgIn.InlineImages);

            return null;
        }
    }
}
