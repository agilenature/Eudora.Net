using Eudora.Net.GUI;
using Eudora.Net.HtmlTemplates;
using Eudora.Net.ExtensionMethods;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Pop3;
using MailKit.Net.Smtp;
using MailKit.Search;
using MimeKit;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using Eudora.Net.Data;

namespace Eudora.Net.Core
{
    /// <summary>
    /// The holder and operator of all things mail and mailbox
    /// </summary>
    public class PostOffice
    {
        public static PostOffice Instance;

        ///////////////////////////////////////////////////////////
        #region Properties
        /////////////////////////////

        public static Mailbox? Inbox
        {
            get => PostOffice.Instance.GetMailboxByName("Inbox");
        }

        public static Mailbox? Drafts
        {
            get => PostOffice.Instance.GetMailboxByName("Drafts");
        }

        public static Mailbox? Sent
        {
            get => PostOffice.Instance.GetMailboxByName("Sent");
        }

        public static Mailbox? Trash
        {
            get => PostOffice.Instance.GetMailboxByName("Trash");
        }

        private static string _MailboxesPath = string.Empty;
        public static string MailboxesPath
        {
            get => _MailboxesPath;
            set { if (_MailboxesPath != value) _MailboxesPath = value; }
        }

        public enum eMailPriority
        {
            Lowest,
            Low,
            Normal,
            High,
            Highest
        }

        public enum eMailDeliveryStatus
        {
            Sent,
            Received,
            Queued
        }

        public enum eMailMessageStatus
        {
            Draft,
            Sealed
        }

        public enum eOutgoingType
        {
            NewThread,
            Reply,
            Forward
        }

        public SortableObservableCollection<Mailbox> Mailboxes { get; set; } = [];

        /////////////////////////////
        #endregion Properties
        ///////////////////////////////////////////////////////////


        ///////////////////////////////////////////////////////////
        #region Fields
        /////////////////////////////

        private static object MailboxSerializationLocker = new();
        
        /////////////////////////////
        #endregion Fields
        ///////////////////////////////////////////////////////////


        /// <summary>
        /// A helper struct to contain MIME message & account data
        /// </summary>
        /// <param name="mime"></param>
        /// <param name="personality"></param>
        class TransmissableMail(MimeMessage mime, Personality personality)
        {
            public MimeMessage MimeMessage { get; set; } = mime;
            public Personality Personality { get; set; } = personality;
        }



        /////////////////////////////////////////////////////////////////
        #region Construction & Initialization
        /////////////////////////////////////////////////////////////////

        static PostOffice()
        {
            Instance = new PostOffice();

            MailboxSerializationLocker = new();
        }

        public PostOffice()
        {
            //MailboxesPath = Path.Combine(Eudora.Net.Properties.Settings.Default.DataStoreRoot, @"Mailboxes");
        }

        public void Startup()
        {
            MailboxesPath = Path.Combine(Eudora.Net.Properties.Settings.Default.DataStoreRoot, @"Mailboxes");

            if (!Path.Exists(MailboxesPath))
            {
                Directory.CreateDirectory(MailboxesPath);
            }

            // Default mailboxes
            if (!File.Exists(MailboxFullPathFromName("Inbox")))
            {
                Mailboxes.Add(new("Inbox", "pack://application:,,,/GUI/res/images/new/newinbox.png", 0));
            }
            if (!File.Exists(MailboxFullPathFromName("Drafts")))
            {
                Mailboxes.Add(new("Drafts", "pack://application:,,,/GUI/res/images/new/newdrafts.png", 1));
            }
            if (!File.Exists(MailboxFullPathFromName("Sent")))
            {
                Mailboxes.Add(new("Sent", "pack://application:,,,/GUI/res/images/new/newsent.png", 2));
            }
            if (!File.Exists(MailboxFullPathFromName("Trash")))
            {
                Mailboxes.Add(new("Trash", "pack://application:,,,/GUI/res/images/new/newtrashbox.png", 3));
            }

            LoadMailboxes();
        }

        public void Shutdown()
        {
            SaveMailboxes();
        }

        /////////////////////////////////////////////////////////////////
        #endregion Construction & Initialization
        /////////////////////////////////////////////////////////////////



        /////////////////////////////////////////////////////////////////
        #region Mailbox Interface
        /////////////////////////////////////////////////////////////////

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Mailbox? GetMailboxByName(string name)
        {
            return Mailboxes.Where(i => i.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        public void AddUserMailbox(string name, string imageSource)
        {
            try
            {
                Mailbox mailbox = new Mailbox(name, imageSource);
                Mailboxes.Add(mailbox);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        public Mailbox? CreateImportedMailbox(string name)
        {
            try
            {
                Mailbox? box = GetMailboxByName(name);
                if (box is not null)
                {
                    return box;
                }

                Mailbox mailbox = new(name, "pack://application:,,,/GUI/res/images/tb32/tb32_51.png");
                Mailboxes.Add(mailbox);
                return mailbox;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="deleteMessages"></param>
        public void RemoveUserMailbox(string name, bool deleteMessages = true)
        {
            // Just in case...
            string nameCheck = name.ToLower();
            if (nameCheck.Equals("inbox", StringComparison.CurrentCultureIgnoreCase) ||
                nameCheck.Equals("drafts", StringComparison.CurrentCultureIgnoreCase) ||
                nameCheck.Equals("sent", StringComparison.CurrentCultureIgnoreCase) ||
                nameCheck.Equals("trash", StringComparison.CurrentCultureIgnoreCase))
            {
                return;
            }

            try
            {
                var mailbox = GetMailboxByName(name);
                if (mailbox is null) return;

                mailbox.Delete();
                Mailboxes.Remove(mailbox);
                string mailboxFile = $@"{name}{Mailbox.extension}";
                File.Delete(Path.Combine(MailboxesPath, mailboxFile));

                var mainWnd = MainWindow.Instance;
                if (mainWnd is null) return;
                var wnd = mainWnd.MDI.FindWindow(mailbox);
                wnd?.Close();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }            
        }

        /////////////////////////////////////////////////////////////////
        #endregion Mailbox Interface
        /////////////////////////////////////////////////////////////////



        /////////////////////////////////////////////////////////////////
        #region Mailbox Internal
        /////////////////////////////////////////////////////////////////

        private string MailboxFullPathFromName(string name)
        {
            //string filename = string.Format("{0}{1}", name, Mailbox.extension);
            string filename = $@"{name}{Mailbox.extension}";
            return Path.Combine(_MailboxesPath, filename);
        }

        private void LoadMailboxes()
        {
            try
            {
                lock(MailboxSerializationLocker)
                {
                    DirectoryInfo di = new(MailboxesPath);
                    //string searchQuery = string.Format("*{0}", Mailbox.extension);
                    string searchQuery = $@"*{Mailbox.extension}";
                    var files = di.GetFiles(searchQuery);
                    foreach (var file in files)
                    {
                        if (file.DirectoryName == null)
                        {
                            continue;
                        }

                        string raw = File.ReadAllText(file.FullName);
                        var mailbox = JsonSerializer.Deserialize<Mailbox>(raw);
                        if (mailbox is not null)
                        {
                            mailbox.Load();
                            Mailboxes.Add(mailbox);
                        }
                    }

                    // File-on-disk order will differ from the desired order
                    Mailboxes.Sort(Mailboxes.OrderBy(i => i.SortOrder));
                }
            }
            catch(Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        private void SaveMailboxes()
        {
            try
            {
                lock(MailboxSerializationLocker)
                {
                    foreach(Mailbox mailbox in Mailboxes)
                    {
                        string path = MailboxFullPathFromName(mailbox.Name);
                        string json = JsonSerializer.Serialize<Mailbox>(mailbox);
                        File.WriteAllText(path, json);
                    }
                }
            }
            catch(Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        /////////////////////////////////////////////////////////////////
        #endregion Mailbox Internal
        /////////////////////////////////////////////////////////////////



        /////////////////////////////////////////////////////////////////
        #region Mail Interface
        /////////////////////////////////////////////////////////////////


        public static EmailMessage CreateMessage_Outgoing(Personality personality)
        {
            EmailMessage message = new();
            message.Status = EmailMessage.MessageStatus.Draft;
            message.Origin = EmailMessage.MessageOrigin.Outgoing;
            message.PersonalityID = personality.Id;
            message.SenderAddress.Address = personality.EmailAddress;
            message.SenderAddress.Name = personality.EmailName;
            message.ReplyTo = personality.ToEmailAddress();
            message.MailboxName = Drafts?.Name ?? string.Empty;
            PostOffice.Instance.GetMailboxByName(message.MailboxName)?.AddMessage(message);

            message.Body = HtmlDepot.MakeBlankEmail(message);
            return message;
        }

        public EmailMessage CreateMessage_Reply(EmailMessage inMessage)
        {
            EmailMessage outMessage = new();

            Personality? personality = PersonalityManager.FindPersonality(inMessage.PersonalityID);
            if(personality is null)
            {
                Logger.NewEvent(LogEvent.EventCategory.Warning, "Failed to find Personality with id: " + inMessage.PersonalityID);
                return outMessage;
            }
            
            outMessage.MailboxName = Drafts?.Name ?? string.Empty;
            outMessage.Status = EmailMessage.MessageStatus.Draft;
            outMessage.Origin = EmailMessage.MessageOrigin.Outgoing;
            outMessage.InReplyToId = inMessage.MessageId;
            outMessage.ReferenceIDs = inMessage.ReferenceIDs;
            outMessage.PersonalityID = inMessage.PersonalityID;
            outMessage.SenderAddress.Address = personality.EmailAddress;
            outMessage.SenderAddress.Name = personality.EmailName;
            outMessage.ReplyTo = personality.ToEmailAddress();
            outMessage.MessageCategory = eOutgoingType.Reply;
            PostOffice.Instance.GetMailboxByName(outMessage.MailboxName)?.AddMessage(outMessage);

            // subject
            if(inMessage.Subject.Contains("re:", StringComparison.CurrentCultureIgnoreCase))
            {
                outMessage.Subject = inMessage.Subject;
            }
            else
            {
                outMessage.Subject = string.Format("Re: {0}", inMessage.Subject);
            }

            // to
            if (!inMessage.ReplyTo.Empty)
            {
                outMessage.Addresses_To.Add(inMessage.ReplyTo);
            }
            else
            {
                outMessage.Addresses_To.Add(inMessage.SenderAddress);
            }

            outMessage.Body = HtmlDepot.MakeEmailReply(inMessage);
            return outMessage;
        }

        public EmailMessage CreateMessage_ReplyAll(EmailMessage inMessage)
        {
            // Begin by creating a reply to the sender
            EmailMessage outMessage = CreateMessage_Reply(inMessage);

            // Then add the other addressees
            outMessage.Addresses_To.AddRangeUnique(inMessage.Addresses_From);
            outMessage.Addresses_To.AddRangeUnique(inMessage.Addresses_To);
            outMessage.Addresses_CC.AddRangeUnique(inMessage.Addresses_CC);
            outMessage.Addresses_BCC.AddRangeUnique(inMessage.Addresses_BCC);

            return outMessage;
        }

        public EmailMessage CreateMessage_Forward(EmailMessage inMessage)
        {
            EmailMessage outMessage = new();
            Personality? personality = PersonalityManager.FindPersonality(inMessage.PersonalityID);
            if (personality is null)
            {
                Logger.NewEvent(LogEvent.EventCategory.Warning, "Failed to find Personality with id: " + inMessage.PersonalityID);
                return outMessage;
            }

            outMessage.MailboxName = Drafts?.Name ?? string.Empty;
            outMessage.Status = EmailMessage.MessageStatus.Draft;
            outMessage.Origin = EmailMessage.MessageOrigin.Outgoing;
            outMessage.InReplyToId = inMessage.MessageId;
            outMessage.ReferenceIDs = inMessage.ReferenceIDs;
            outMessage.PersonalityID = inMessage.PersonalityID;
            outMessage.SenderAddress.Address = personality.EmailAddress;
            outMessage.SenderAddress.Name = personality.EmailName;
            outMessage.ReplyTo = personality.ToEmailAddress();
            outMessage.MessageCategory = eOutgoingType.Forward;
            PostOffice.Instance.GetMailboxByName(outMessage.MailboxName)?.AddMessage(outMessage);

            outMessage.Body = HtmlDepot.MakeEmailForward(inMessage);
            return outMessage;
        }

        public static EmailMessage CreateMessage_Incoming()
        {
            EmailMessage message = new();
            message.Status = EmailMessage.MessageStatus.Sealed;
            message.Origin = EmailMessage.MessageOrigin.IncomingTo;
            message.MailboxName = Inbox?.Name ?? string.Empty;
            return message;
        }

        


        /// <summary>
        /// TODO: MoveMessage must also move attachments folder
        /// </summary>
        /// <param name="message"></param>
        /// <param name="mailboxName"></param>
        public void MoveMessage(EmailMessage message, string mailboxName)
        {
            using (var uxLocker = new Core.UXLocker())
            {
                var mailboxTo = GetMailboxByName(mailboxName);
                var mailboxFrom = GetMailboxByName(message.MailboxName);
                if (mailboxTo is not null && mailboxFrom is not null)
                {
                    message.MailboxName = mailboxName;
                    mailboxFrom.DeleteMessage(message);
                    mailboxTo.AddMessage(message);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="mailboxName"></param>
        public void CopyMessage(EmailMessage message, string mailboxName)
        {
            using (var uxLocker = new Core.UXLocker())
            {
                var mailboxTo = GetMailboxByName(mailboxName);
                var mailboxFrom = GetMailboxByName(message.MailboxName);
                if (mailboxTo is not null && mailboxFrom is not null)
                {
                    EmailMessage copy = (EmailMessage)message.Clone();
                    copy.MailboxName = mailboxName;
                    mailboxTo.AddMessage(copy);
                }
            }
        }

        /// <summary>
        /// Check all accounts that exist
        /// </summary>
        public async void CheckMail()
        {
            await Task.Run(() => CheckAllAccounts());            
        }

        public void CheckAllAccounts()
        {
            foreach (var personality in PersonalityManager.Datastore.Data)
            {
                Logger.NewEvent(LogEvent.EventCategory.Information, "Checking " + personality.EmailAddress);
                if(personality.UsePop)
                {
                    RetrieveWithPOP(personality);
                }
                else
                {
                    RetrieveWithIMAP(personality);
                }
            }
        }

        private void RetrieveWithPOP(Personality personality)
        {
            using (Pop3Client client = new Pop3Client())
            {
                client.ServerCertificateValidationCallback = CertificateValidationCallback;

                try
                {
                    client.CheckCertificateRevocation = true;

                    client.Connect(
                        personality.Server_Incoming,
                        personality.Port_Incoming,
                        personality.SocketOptions_Incoming);

                    client.Authenticate(personality.EmailAddress, personality.EmailPassword);
                    Logger.NewEvent(LogEvent.EventCategory.Information, "Retrieving " + client.Count.ToString() + " messages");

                    

                    // Retrieve MIME messages from server & disconnect
                    List<MimeMessage> mimes = [];
                    for (int i = 0; i < client.Count; i++)
                    {
                        MimeMessage? mime = client.GetMessage(i);
                        
                        if (mime is not null)
                        {
                            mimes.Add(mime);
                        }
                    }
                    client.Disconnect(true);

                    // Iterate the mime list, converting each to a MailMessage and route it appropriately
                    foreach(MimeMessage mime in mimes)
                    {
                        try
                        {
                            var m = new MimeToMessage(mime);
                            m.Convert();
                            m.Message.PersonalityID = personality.Id;
                            RouteIncomingMessage(m.Message);
                        }
                        catch(Exception ex)
                        {
                            Logger.LogException(ex);
                        }
                    }
                    Logger.NewEvent(LogEvent.EventCategory.Information, "Finished");
                }
                catch (Exception ex)
                {
                    Logger.LogException(ex);
                }
                finally
                {
                    client.Disconnect(true);
                    client.Dispose();
                }
            }
        }

        

        private void RetrieveWithIMAP(Personality personality, bool onlyUnread = false)
        {
            using (ImapClient client = new ImapClient())
            {
                client.ServerCertificateValidationCallback = CertificateValidationCallback;
                try
                {
                    client.CheckCertificateRevocation = true;

                    client.Connect(
                        personality.Server_Incoming,
                        personality.Port_Incoming,
                        personality.SocketOptions_Incoming);

                    client.Authenticate(personality.EmailAddress, personality.EmailPassword);
                    var remoteInbox = client.Inbox;
                    var result = remoteInbox.Open(MailKit.FolderAccess.ReadWrite);

                    List<MimeMessage> mimes = [];

                    if (onlyUnread)
                    {
                        List<MailKit.UniqueId> results = [.. remoteInbox.Search(SearchQuery.NotSeen)];
                        Logger.NewEvent(LogEvent.EventCategory.Information, $"Retrieving {results.Count} unread messages...");
                        
                        foreach (var uid in results)
                        {
                            remoteInbox.SetFlags(uid, MessageFlags.Seen, false);
                            mimes.Add(remoteInbox.GetMessage(uid));
                        }
                    }
                    else
                    {
                        int messageCount = remoteInbox.Count;
                        Logger.NewEvent(LogEvent.EventCategory.Information, $"Retrieving {messageCount} messages...");
                        for(int i = 0; i < messageCount; i++)
                        {
                            mimes.Add(remoteInbox.GetMessage(i));
                        }
                    }

                    // Done with remote inbox & server connection
                    remoteInbox.Close();
                    client.Disconnect(true);


                    // Iterate the mime list, converting each to a MailMessage and route it appropriately
                    foreach (MimeMessage mime in mimes)
                    {
                        try
                        {
                            var m = new MimeToMessage(mime);
                            m.Message.PersonalityID = personality.Id;
                            m.Convert();
                            RouteIncomingMessage(m.Message);
                        }
                        catch(Exception ex)
                        {
                            Logger.LogException(ex);
                        }
                    }

                    Logger.NewEvent(LogEvent.EventCategory.Information, "Finished");
                }
                catch (Exception ex)
                {
                    Logger.LogException(ex);
                }
                finally
                {
                    client.Disconnect(true);
                    client.Dispose();
                }
            }
        }


        public async Task SendMessage(EmailMessage message)
        {
            Logger.NewEvent(LogEvent.EventCategory.Information, "Sending...");
            var mime = PrepareMessage(message);
            if (mime != null)
            {
                if (TransmitMail(mime))
                {
                    MoveMessage(message, Sent?.Name ?? string.Empty);
                    message.Status = EmailMessage.MessageStatus.Sealed;
                    message.SendStatus = EmailMessage.eSendStatus.Sent;
                }
            }
            Logger.NewEvent(LogEvent.EventCategory.Information, "Message sent");
        }        
        

        /////////////////////////////////////////////////////////////////
        #endregion Mail Interface
        /////////////////////////////////////////////////////////////////



        /////////////////////////////////////////////////////////////////
        #region Mail Internal
        /////////////////////////////////////////////////////////////////

        /// <summary>
        /// Decide what to do with this message. By default it will go to the
        /// Inbox unless the application of one or more filters decides otherwise.
        /// </summary>
        /// <param name="message"></param>
        private async void RouteIncomingMessage(EmailMessage message)
        {
            Mailbox? mailbox = Inbox;
            if (mailbox is null) return;
            
            var existing = mailbox.Messages.Where(msg => msg.MessageId.Equals(message.MessageId, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
            if (existing is not null) return;
            
            mailbox.AddMessage(message);
            Notifier.NotifyNewEmail(message);

            EudoraStatistics.IncrementCounter(EudoraStatistics.eRowIndex.Mail_NewMessageIn);
            if(message.Attachments.Any())
            {
                EudoraStatistics.IncrementCounter(EudoraStatistics.eRowIndex.Mail_NewAttachmentIn, (uint)message.Attachments.Count);
            }

            if (message.MessageCategory == eOutgoingType.Reply)
            {
                EudoraStatistics.IncrementCounter(EudoraStatistics.eRowIndex.Mail_NewReply);
            }
            else if (message.MessageCategory == eOutgoingType.Forward)
            {
                EudoraStatistics.IncrementCounter(EudoraStatistics.eRowIndex.Mail_NewForward);
            }

            // Check this message against each of the existing filters
            foreach (EmailFilter filter in FilterManager.Datastore.Data)
            {
                var result = await filter.AppliesToMessage(message);
                if (result)
                {
                    filter.Action.Act(message);
                    EmailFilterReporter.NewReport(message, filter);
                }
            }
        }

        /// <summary>
        /// Transmute Eudora.Net MailMessage to a MIME message
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private TransmissableMail? PrepareMessage(EmailMessage message)
        {
            // Retrieve personality based on the key the message was created with
            Personality? personality = PersonalityManager.FindPersonality(message.PersonalityID);
            if (personality == null)
            {
                return null;
            }

            // Fill out the actual mime message from MailMessage
            var converter = new MessageToMime(message);
            try
            {
                converter.Run();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }

            // Return the constructed, transmissable mail item
            return new TransmissableMail(converter.Mime, personality);
        }

        /// <summary>
        /// The actual transmission of a MIME message
        /// </summary>
        /// <param name="mail"></param>
        private bool TransmitMail(TransmissableMail mail)
        {
            bool result = false;
            using (SmtpClient smtpClient = new SmtpClient())
            {
                try
                {
                    smtpClient.Connect(mail.Personality.Server_Outgoing, mail.Personality.Port_Outgoing, mail.Personality.SocketOptions_Outgoing);
                    smtpClient.Authenticate(mail.Personality.EmailAddress, mail.Personality.EmailPassword);
                    smtpClient.Send(mail.MimeMessage);
                    result = true;

                    EudoraStatistics.IncrementCounter(EudoraStatistics.eRowIndex.Mail_NewMessageOut);
                    if(mail.MimeMessage.Attachments.Any())
                    {
                        EudoraStatistics.IncrementCounter(EudoraStatistics.eRowIndex.Mail_NewAttachmentOut, (uint)mail.MimeMessage.Attachments.Count());
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogException(ex);
                }
                finally
                {
                    smtpClient.Disconnect(true);
                    smtpClient.Dispose();
                }
            }
            return result;
        }


        /////////////////////////////////////////////////////////////////
        #endregion Mail Internal
        /////////////////////////////////////////////////////////////////



        ///////////////////////////////////////////////////////////
        #region Mail Security
        /////////////////////////////

        static bool CertificateValidationCallback(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
            {
                return true;
            }

            string host = (string)sender;
            string message = string.Empty;

            if ((sslPolicyErrors & SslPolicyErrors.RemoteCertificateNotAvailable) != 0)
            {
                message = string.Format("The SSL certificate was not available for {0}", host);
                Logger.NewEvent(LogEvent.EventCategory.Warning, message);
                return false;
            }

            if ((sslPolicyErrors & SslPolicyErrors.RemoteCertificateNameMismatch) != 0)
            {
                if (certificate is null)
                {
                    message = string.Format("The Common Name for the SSL certificate did not match");
                    Logger.NewEvent(LogEvent.EventCategory.Warning, message);
                    return false;
                }

                var certificate2 = certificate as X509Certificate2;
                var cn = certificate2 != null ? certificate2.GetNameInfo(X509NameType.SimpleName, false) : certificate.Subject;

                message = string.Format("The Common Name for the SSL certificate did not match {0}. Instead, it was {1}", host, cn);
                Logger.NewEvent(LogEvent.EventCategory.Warning, message);
                return false;
            }

            message = string.Format("The SSL certificate for {0} could not be validated for the following reasons:", host);
            Logger.NewEvent(LogEvent.EventCategory.Warning, message);

            if (chain is not null)
            {
                foreach (var element in chain.ChainElements)
                {
                    // Each element in the chain will have its own status list. If the status list is empty, it means that the
                    // certificate itself did not contain any errors.
                    if (element.ChainElementStatus.Length == 0)
                    {
                        continue;
                    }

                    message = string.Format("\u2022 {0}", element.Certificate.Subject);
                    Logger.NewEvent(LogEvent.EventCategory.Information, message);
                    foreach (var error in element.ChainElementStatus)
                    {
                        // `error.StatusInformation` contains a human-readable error string while `error.Status` is the corresponding enum value.
                        message = string.Format("\t\u2022 {0}", error.StatusInformation);
                        Logger.NewEvent(LogEvent.EventCategory.Information, message);
                    }
                }
            }

            return false;
        }

        /////////////////////////////
        #endregion Mail Security
        ///////////////////////////////////////////////////////////


    }
}
