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
using Google.Apis.Auth;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Util.Store;
using MailKit.Security;
using Windows.Media.Protection.PlayReady;


namespace Eudora.Net.Core
{
    /// <summary>
    /// The holder and operator of all things mail and mailbox
    /// </summary>
    public class PostOffice
    {
        public static PostOffice Instance;

        private static Timer NewMailCheckTimer = new(
            EmailCheckTimerCallback,
            null,
            Properties.Settings.Default.EmailCheckFrequency * 60000,
            Properties.Settings.Default.EmailCheckFrequency * 60000);

        public static void UpdateTimerFrequency()
        {
            NewMailCheckTimer.Change(
                Properties.Settings.Default.EmailCheckFrequency * 60000,
                Properties.Settings.Default.EmailCheckFrequency * 60000);
        }

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

        public enum eMailDraftStatus
        {
            Draft,
            Sealed
        }

        public enum eMailThreadType
        {
            NewThread,
            Reply,
            Forward
        }

        public enum eMailOrigin
        {
            Outgoing,
            IncomingTo,
            IncomingCC,
            IncomingBCC,
            Default
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
                Mailboxes.Add(new("Inbox", "pack://application:,,,/GUI/res/images/new/newmailbox.png", 0));
            }
            if (!File.Exists(MailboxFullPathFromName("Drafts")))
            {
                Mailboxes.Add(new("Drafts", "pack://application:,,,/GUI/res/images/new/newmailbox.png", 1));
            }
            if (!File.Exists(MailboxFullPathFromName("Sent")))
            {
                Mailboxes.Add(new("Sent", "pack://application:,,,/GUI/res/images/new/newmailbox.png", 2));
            }
            if (!File.Exists(MailboxFullPathFromName("Trash")))
            {
                Mailboxes.Add(new("Trash", "pack://application:,,,/GUI/res/images/new/newmailbox.png", 3));
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
                Logger.Exception(ex);
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
                Logger.Exception(ex);
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
                Logger.Exception(ex);
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
                Logger.Exception(ex);
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
                Logger.Exception(ex);
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
                Logger.Warning($"Failed to find Personality with id: {inMessage.PersonalityID}");
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
            outMessage.MessageCategory = eMailThreadType.Reply;
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
                Logger.Warning($"Failed to find Personality with id: {inMessage.PersonalityID}");
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
            outMessage.MessageCategory = eMailThreadType.Forward;
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
            //using (var uxLocker = new Core.UXLocker())
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
            await Task.Run(async () => CheckAllAccounts());            
        }

        public async void CheckAllAccounts()
        {
            foreach (var personality in PersonalityManager.Datastore.Data)
            {
                Logger.Information($"Checking {personality.EmailAddress}");

                if (personality.IsGmail)
                {
                    RetrieveGmail(personality);
                }
                else
                {
                    if (personality.UsePop)
                    {
                        RetrieveWithPOP(personality);
                    }
                    else
                    {
                        RetrieveWithIMAP(personality);
                    }
                }
            }
        }

        private void RetrieveWithPOP(Personality personality)
        {
            try
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
                        Logger.Information($"Retrieving {client.Count.ToString()} messages");

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
                        foreach (MimeMessage mime in mimes)
                        {
                            try
                            {
                                var m = new MimeToMessage(mime);
                                m.Convert();
                                m.Message.PersonalityID = personality.Id;
                                RouteIncomingMessage(m.Message);
                            }
                            catch (Exception ex)
                            {
                                Logger.Error(ex.Message);
                            }
                        }
                        Logger.Information("Finished");
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex.Message);
                    }
                    finally
                    {
                        client.Disconnect(true);
                        client.Dispose();
                    }
                }
            }
            catch(Exception ex)
            {
                Logger.Error(ex.Message);
            }
        }

        

        private void RetrieveWithIMAP(Personality personality, bool onlyUnread = false)
        {
            try
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
                            Logger.Information($"Retrieving {results.Count} unread messages...");

                            foreach (var uid in results)
                            {
                                remoteInbox.SetFlags(uid, MessageFlags.Seen, false);
                                mimes.Add(remoteInbox.GetMessage(uid));
                            }
                        }
                        else
                        {
                            int messageCount = remoteInbox.Count;
                            Logger.Information($"Retrieving {messageCount} messages...");
                            for (int i = 0; i < messageCount; i++)
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
                            catch (Exception ex)
                            {
                                Logger.Error(ex.Message);
                            }
                        }

                        Logger.Information("Finished");
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex.Message);
                    }
                    finally
                    {
                        client.Disconnect(true);
                        client.Dispose();
                    }
                }
            }
            catch(Exception ex)
            {
                Logger.Error(ex.Message);
            }
        }

        private async void RetrieveGmail(Personality personality, bool onlyUnread = false)
        {
            try
            {
                var oauth2 = GmailLogin(personality).Result;
                if (oauth2 is null)
                {
                    return;
                }

                List<MimeMessage> mimes = [];

                using (var client = new ImapClient())
                {
                    await client.ConnectAsync("imap.gmail.com", 993, SecureSocketOptions.SslOnConnect);
                    await client.AuthenticateAsync(oauth2);

                    var remoteInbox = client.Inbox;
                    var result = remoteInbox.Open(MailKit.FolderAccess.ReadWrite);

                    if (onlyUnread)
                    {
                        List<MailKit.UniqueId> results = [.. remoteInbox.Search(SearchQuery.NotSeen)];
                        Logger.Information($"Retrieving {results.Count} unread messages...");

                        foreach (var uid in results)
                        {
                            remoteInbox.SetFlags(uid, MessageFlags.Seen, false);
                            mimes.Add(remoteInbox.GetMessage(uid));
                        }
                    }
                    else
                    {
                        int messageCount = remoteInbox.Count;
                        Logger.Information($"Retrieving {messageCount} messages...");
                        for (int i = 0; i < messageCount; i++)
                        {
                            mimes.Add(remoteInbox.GetMessage(i));
                        }
                    }

                    // Done with remote inbox & server connection
                    remoteInbox.Close();
                    await client.DisconnectAsync(true);
                }


                // Iterate the mime list, converting each to a Eudora.EmailMessage and route it appropriately
                foreach (MimeMessage mime in mimes)
                {
                    try
                    {
                        var m = new MimeToMessage(mime);
                        m.Message.PersonalityID = personality.Id;
                        m.Convert();
                        RouteIncomingMessage(m.Message);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex.Message);
                    }
                }
            }
            catch(Exception ex)
            {
                Logger.Error(ex.Message);
            }

            Logger.Information("Finished");
        }


        public async Task SendMessage(EmailMessage message)
        {
            try
            {
                Logger.Information("Sending...");
                var mime = PrepareMessage(message);
                if (mime != null)
                {
                    bool messageSent = false;

                    var personality = PersonalityManager.FindPersonality(message.PersonalityID);
                    if (personality is not null)
                    {
                        if (personality.IsGmail)
                        {
                            if (await TransmitGmail(mime))
                            {
                                messageSent = true;
                            }
                            else if (await TransmitMail(mime))
                            {
                                messageSent = true;
                            }
                        }
                    }

                    if (messageSent)
                    {
                        MoveMessage(message, Sent?.Name ?? string.Empty);
                        message.Status = EmailMessage.MessageStatus.Sealed;
                        message.SendStatus = EmailMessage.eSendStatus.Sent;
                    }
                    else
                    {
                        Logger.Warning("Failed to send message");
                    }
                }
                Logger.Information("Message sent");
            }
            catch(Exception ex)
            {
                Logger.Error(ex.Message);
            }
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

            if (message.MessageCategory == eMailThreadType.Reply)
            {
                EudoraStatistics.IncrementCounter(EudoraStatistics.eRowIndex.Mail_NewReply);
            }
            else if (message.MessageCategory == eMailThreadType.Forward)
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
                Logger.Exception(ex);
            }

            // Return the constructed, transmissable mail item
            return new TransmissableMail(converter.Mime, personality);
        }

        /// <summary>
        /// The actual transmission of a MIME message
        /// </summary>
        /// <param name="mail"></param>
        private async Task<bool> TransmitMail(TransmissableMail mail)
        {
            bool result = false;
            using (SmtpClient smtpClient = new())
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
                    Logger.Error(ex.Message);
                }
                finally
                {
                    smtpClient.Disconnect(true);
                    smtpClient.Dispose();
                }
            }
            return result;
        }

        private async Task<bool> TransmitGmail(TransmissableMail mail)
        {
            bool result = false;

            try
            {
                var oauth2 = GmailLogin(mail.Personality).Result;
                if(oauth2 is null)
                {
                    return result;
                }

                using (SmtpClient smtpClient = new())
                {
                    try
                    {
                        await smtpClient.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                        await smtpClient.AuthenticateAsync(oauth2);
                        await smtpClient.SendAsync(mail.MimeMessage);
                        result = true;

                        EudoraStatistics.IncrementCounter(EudoraStatistics.eRowIndex.Mail_NewMessageOut);
                        if (mail.MimeMessage.Attachments.Any())
                        {
                            EudoraStatistics.IncrementCounter(EudoraStatistics.eRowIndex.Mail_NewAttachmentOut, (uint)mail.MimeMessage.Attachments.Count());
                        }
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

            return result;
        }

        public static void EmailCheckTimerCallback(object? state)
        {
            Parallel.Invoke(async () => PostOffice.Instance.CheckMail());
        }

        /////////////////////////////////////////////////////////////////
        #endregion Mail Internal
        /////////////////////////////////////////////////////////////////



        ///////////////////////////////////////////////////////////
        #region Mail Security
        /////////////////////////////

        private async Task<SaslMechanismOAuth2?> GmailLogin(Personality personality)
        {
            try
            {
                var clientSecrets = new ClientSecrets()
                {
                    ClientId = EudoraHelper.IGmailConnection.ClientId,
                    ClientSecret = EudoraHelper.IGmailConnection.ClientSecret
                };

                var codeFlow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
                {
                    DataStore = new FileDataStore("CredentialCacheFolder", false),
                    Scopes = new[] { "https://mail.google.com/" },
                    ClientSecrets = clientSecrets,
                    LoginHint = personality.EmailAddress
                });

                var codeReceiver = new LocalServerCodeReceiver();
                var authCode = new AuthorizationCodeInstalledApp(codeFlow, codeReceiver);

                var credential = await authCode.AuthorizeAsync(personality.EmailAddress, CancellationToken.None);
                if (credential.Token.IsStale)
                {
                    await credential.RefreshTokenAsync(CancellationToken.None);
                }

                var oauth2 = new SaslMechanismOAuth2(credential.UserId, credential.Token.AccessToken);
                if (oauth2 is null)
                {
                    Logger.Warning("Gmail OAuth2 result was null");
                }
                return oauth2;
            }
            catch(Exception ex)
            {
                Logger.Error(ex.Message);
            }

            return null;
        }

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
                Logger.Warning(message);
                return false;
            }

            if ((sslPolicyErrors & SslPolicyErrors.RemoteCertificateNameMismatch) != 0)
            {
                if (certificate is null)
                {
                    message = string.Format("The Common Name for the SSL certificate did not match");
                    Logger.Warning(message);
                    return false;
                }

                var certificate2 = certificate as X509Certificate2;
                var cn = certificate2 != null ? certificate2.GetNameInfo(X509NameType.SimpleName, false) : certificate.Subject;

                message = string.Format("The Common Name for the SSL certificate did not match {0}. Instead, it was {1}", host, cn);
                Logger.Warning(message);
                return false;
            }

            message = string.Format("The SSL certificate for {0} could not be validated for the following reasons:", host);
            Logger.Warning(message);

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
                    Logger.Information(message);
                    foreach (var error in element.ChainElementStatus)
                    {
                        // `error.StatusInformation` contains a human-readable error string while `error.Status` is the corresponding enum value.
                        message = string.Format("\t\u2022 {0}", error.StatusInformation);
                        Logger.Information(message);
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
