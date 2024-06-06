using MimeKit;
using Eudora.Net.Data;
using MimeKit.IO;
using MimeKit.Encodings;
using MimeKit.Text;
using MimeKit.Utils;
using System.Net.Mime;
using System.IO;
using MailKit.Net.Imap;
using System.Drawing;


namespace Eudora.Net.Core
{
    public class MessageToMime
    {
        ///////////////////////////////////////////////////////////
        #region Properties
        /////////////////////////////

        private MimeMessage _Mime = new();
        public MimeMessage Mime
        {
            get => _Mime;
        }

        private EmailMessage _Message;
        public EmailMessage Message
        { 
            get => _Message;
            set { if (_Message != value) _Message = value; }
        }

        /////////////////////////////
        #endregion Properties
        ///////////////////////////////////////////////////////////
        

        public MessageToMime(EmailMessage message)
        {
            _Message = message;
        }

        public void Run()
        {
            Mime.Date = DateTime.Now;
            Mime.Subject = Message.Subject;
            Mime.Sender = new MailboxAddress(Message.SenderAddress.Name, Message.SenderAddress.Address);
            Mime.ReplyTo.Add(Mime.Sender);

            foreach(var address in Message.Addresses_To)
            {
                Mime.To.Add(new MailboxAddress(address.Name, address.Address));
            }

            foreach (var address in Message.Addresses_CC)
            {
                Mime.Cc.Add(new MailboxAddress(address.Name, address.Address));
            }

            foreach (var address in Message.Addresses_BCC)
            {
                Mime.Bcc.Add(new MailboxAddress(address.Name, address.Address));
            }

            BodyBuilder builder = new();


            //Inline images
            string messageBodyImagesConverted = Message.Body;
            foreach (var ii in Message.InlineImages)
            {
                //MimePart part = new(MediaTypeNames.Image.Png)
                //{
                //    ContentId = image.Alt,
                //    ContentTransferEncoding = ContentEncoding.Base64,
                //};
                //string newFilePath = GHelpers.ConvertImageToPng(image.Source);
                //part.Content = new MimeContent(File.OpenRead(newFilePath));
                //part.ContentId = image.Alt;
                //builder.LinkedResources.Add(part);

                //// Still have to edit the html source to change the source to a content id
                //messageBodyImagesConverted = messageBodyImagesConverted.Replace(image.Source, image.HTMLSource);

                //var image = builder.LinkedResources.Add(ii.Alt);
                //image.ContentId = ii.Alt;
                //image.ContentDisposition = new MimeKit.ContentDisposition(MimeKit.ContentDisposition.Attachment);
                //image.ContentDisposition.FileName = ii.Alt;
                //image.ContentTransferEncoding = ContentEncoding.Base64;
                //image.ContentType.MediaType = "image/png";
                MimePart img = new MimePart(MediaTypeNames.Image.Png)
                {
                    ContentId = ii.Alt,
                    ContentTransferEncoding = ContentEncoding.Base64,
                    ContentDisposition = new MimeKit.ContentDisposition(MimeKit.ContentDisposition.Inline)
                };
                img.ContentDisposition.FileName = ii.Alt;
                img.Content = new MimeContent(File.OpenRead(ii.Source));

                messageBodyImagesConverted = messageBodyImagesConverted.Replace(ii.HTMLSource, ii.CIDSource);
                
                builder.LinkedResources.Add(img);
            }

            // attachments
            foreach (var attachment in Message.Attachments)
            {
                builder.Attachments.Add(attachment.Path);
            }

            builder.HtmlBody = messageBodyImagesConverted;//Message.Body;
            Mime.Body = builder.ToMessageBody();
        }
    }
}
