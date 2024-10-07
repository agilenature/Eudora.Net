using MimeKit;
using Eudora.Net.Data;

namespace Eudora.Net.Core
{
    /// <summary>
    /// A helper & parser for turning MIME message headers into Data.MailMessage headers
    /// </summary>
    public class EmailHeaderParser
    {
        private MimeMessage Mime;
        private EmailMessage Message;

        public EmailHeaderParser(MimeMessage mime, ref EmailMessage message)
        {
            Mime = mime;
            Message = message;
        }

        public void Parse()
        {
            // date & subject
            Message.Date = Mime.Date.DateTime;
            Message.Subject = Mime.Subject;

            // message ID
            Message.MessageId = Mime.MessageId;

            // ID of ReplyTo; a reference to the message this might be a response to
            Message.InReplyToId = Mime.InReplyTo;

            // reference IDs
            foreach (var referenceID in Mime.References)
            {
                Message.ReferenceIDs.Add(referenceID);
            }

            // Sender(s)

            // Reply-to address(es) might not be the same as Sender address(es)
            if(Mime.ReplyTo is not null && Mime.ReplyTo.Count > 0)
            {
                var replyto = Mime.ReplyTo.First() as MailboxAddress;
                if(replyto is not null)
                {
                    Message.ReplyTo = new() { Name = replyto.Name, Address = replyto.Address };
                }                
            }

            foreach(var address in Mime.From)
            {
                var mba = address as MailboxAddress;
                if(mba is not null)
                {
                    Message.Addresses_From.Add(new EmailAddress(mba.Name, mba.Address));
                    if(String.IsNullOrEmpty(Message.SenderAddress.Name))
                    {
                        Message.SenderAddress = new EmailAddress(mba.Name, mba.Address);
                    }
                }
            }
            

            // Addressees
            foreach(var address in Mime.To)
            {
                var mba = address as MailboxAddress;
                if (mba is not null)
                {
                    Message.Addresses_To.Add(new EmailAddress(mba.Name, mba.Address));
                }
            }
            foreach(var address in Mime.Cc)
            {
                var mba = address as MailboxAddress;
                if (mba is not null)
                {
                    Message.Addresses_CC.Add(new EmailAddress(mba.Name, mba.Address));
                }
            }
            foreach(var address in Mime.Bcc)
            {
                var mba = address as MailboxAddress;
                if (mba is not null)
                {
                    Message.Addresses_BCC.Add(new EmailAddress(mba.Name, mba.Address));
                }
            }
        }
    }
}
