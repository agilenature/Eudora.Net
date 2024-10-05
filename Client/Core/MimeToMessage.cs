using MimeKit;
using System.IO;
using Eudora.Net.Data;


namespace Eudora.Net.Core
{
    /// <summary>
    /// A converter for MimeMessage to Core.MailMessage.
    /// No need to clutter up the PostOffice class with scattered functions
    /// </summary>
    public class MimeToMessage
    {

        ///////////////////////////////////////////////////////////
        #region Properties
        /////////////////////////////

        // These are accessible but read-only from the outside

        private MimeMessage _Mime;
        public MimeMessage Mime
        {
            get => _Mime;
        }

        private EmailMessage _Message;
        public EmailMessage Message
        {
            get => _Message;
        }

        /////////////////////////////
        #endregion Properties
        ///////////////////////////////////////////////////////////




        ///////////////////////////////////////////////////////////
        #region Construction / Init
        /////////////////////////////

        public MimeToMessage(MimeMessage mime)
        {
            _Mime = mime;
            _Message = PostOffice.CreateMessage_Incoming();
        }

        /////////////////////////////
        #endregion Construction / Init
        ///////////////////////////////////////////////////////////




        ///////////////////////////////////////////////////////////
        #region Interface
        /////////////////////////////

        /// <summary>
        /// Execute the parsing andconversion process, step by step.
        /// Apart from the read-only properties, this call is the only interface.
        /// </summary>
        public void Convert()
        {
            ParseHeaders();
            ParseBody();
        }


        /////////////////////////////
        #endregion Interface
        ///////////////////////////////////////////////////////////




        ///////////////////////////////////////////////////////////
        #region Operations
        /////////////////////////////

        private void ParseHeaders()
        {
            var parser = new EmailHeaderParser(_Mime, ref _Message);
            parser.Parse();
        }

        private async void ParseBody()
        {
            try
            {
                var visitor = new EmailBodyParser();

                // Inline message parts
                _Mime.Accept(visitor);
                _Message.Body = visitor.HtmlBody;

                // Attachment message parts
                if (visitor.Attachments.Count == 0) return;

                var mailbox = PostOffice.GetMailboxByName(Message.MailboxName);
                if (mailbox == null) return;

                string attachmentFolder = Path.Combine(Properties.Settings.Default.DataStoreRoot, @"Data/Attachments", Message.InternalId.ToString());
                IoUtil.EnsureFolder(attachmentFolder);

                foreach (var attachment in visitor.Attachments)
                {
                    try
                    {
                        string fileName = string.Empty;
                        string fullPath = string.Empty;

                        if (attachment is MessagePart rfc822)
                        {
                            if (attachment.ContentDisposition is not null)
                            {
                                fileName = attachment.ContentDisposition.FileName;
                            }
                            else
                            {
                                fileName = "attached-message.eml";
                            }

                            fullPath = Path.Combine(attachmentFolder, fileName);
                            using var stream = File.Create(fullPath);
                            {
                                rfc822.Message.WriteTo(stream);
                            }
                        }
                        else if (attachment is MimePart part && !string.IsNullOrEmpty(part.FileName))
                        {
                            fileName = part.FileName;
                            fullPath = Path.Combine(attachmentFolder, fileName);
                            using var stream = File.Create(fullPath);
                            {
                                part.Content.DecodeTo(stream);
                            }
                        }
                        else
                        {
                            // Something about this attachment was malformed or missing in the mime
                            continue;
                        }

                        var mailAttachment = new EmailAttachment()
                        {
                            Name = fileName,
                            Path = fullPath,
                        };
                        Message.Attachments.Add(mailAttachment);

                        // Invoke Microsoft Defender to scan the file
                        await GVirusScanner.ScanFile(fullPath);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }
        }
        

        /////////////////////////////
        #endregion Operations
        ///////////////////////////////////////////////////////////


    }
}
