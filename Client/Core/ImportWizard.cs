using Eudora.Net.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Email;

namespace Eudora.Net.Core
{
    internal class ImportWizard
    {
        // Step 0: Locate Eudora Data
        // Step 1: Email Accounts
        // Step 2: Mailboxes
        // Step 2.1: Mail
        // Step 3: Address Book
        // Step 3.1: Contacts

        #region FIELDS
        private string _EudoraDataPath = string.Empty;
        private List<string> Accounts = [];
        private List<string> Mailboxes = [];
        private List<string> AddressBooks = [];
        static string DefaultEudoraPath = @"C:\Program Files (x86)\Qualcomm\Eudora";
        static string DefaultEudoraDataPath = @"C:\Users\{0}\AppData\Roaming\Qualcomm\Eudora";
        private static string mailboxExtension = ".mbx";
        private static string emailDelimiter = "\r\nFrom ";
        private List<ImportedEmbeddedContent> EmbeddedImages = [];
        #endregion FIELDS


        #region PROPERTIES
        public string EudoraDataPath
        {
            get => _EudoraDataPath;
            set => _EudoraDataPath = value;
        }
        #endregion PROPERTIES


        // STEP 0: Locate Eudora Data
        public bool LocateEudoraData()
        {
            try
            {
                string user = Environment.UserName;
                string defaultEudoraDataPath = DefaultEudoraDataPath.Replace("{0}", user);

                if (Directory.Exists(defaultEudoraDataPath))
                {
                    EudoraDataPath = defaultEudoraDataPath;
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                return false;
            }

            return false;
        }

        // STEP 1: Import Email Accounts
        public bool ImportAccounts()
        {
            try
            {
                string iniFilePath = Path.Combine(EudoraDataPath, "eudora.ini");
                if (!File.Exists(iniFilePath))
                {
                    return false;
                }

                string[] lines = File.ReadAllLines(iniFilePath);
                if (lines.Length == 0)
                {
                    return false;
                }

                // The default or "dominant" account is the first one listed in the eudora.ini file,
                // not found in the "personalities" section

            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                return false;
            }

            return false;
        }

        // STEP 2: Import Mailboxes & Mail
        private Mailbox? GetMailboxFromFilename(string filename)
        {
            try
            {
                string name = Path.GetFileNameWithoutExtension(filename);
                
                // default mailboxes
                if(name.Equals("in", StringComparison.CurrentCultureIgnoreCase))
                {
                    return Mailbox.Inbox;
                }
                else if(name.Equals("out", StringComparison.CurrentCultureIgnoreCase))
                {
                    return Mailbox.Outbox;
                }
                else if(name.Equals("junk", StringComparison.CurrentCultureIgnoreCase))
                {
                    return Mailbox.Junk;
                }
                else if(name.Equals("trash", StringComparison.CurrentCultureIgnoreCase))
                {
                    return Mailbox.Trash;
                }

                // CreateImportedMailbox handles the case where the mailbox already exists
                return PostOffice.Instance.CreateImportedMailbox(name);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                return null;
            }
        }

        public bool ImportMailboxes()
        {
            try
            {
                DirectoryInfo di = new(EudoraDataPath);
                string searchQuery = string.Format("*{0}", mailboxExtension);
                var files = di.GetFiles(searchQuery);
                if (files.Length == 0)
                {
                    return false;
                }

                foreach (var mailboxFile in files)
                {
                    if (File.Exists(mailboxFile.FullName))
                    {
                        Mailbox? mailbox = GetMailboxFromFilename(mailboxFile.Name);
                        if(mailbox is null)
                        {
                            continue;
                        }
                        string mbtext = File.ReadAllText(mailboxFile.FullName);
                        ParseMailbox(mbtext, mailbox);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                return false;
            }
            return true;
        }

        private void ParseMailbox(string mailboxString, Mailbox mailbox)
        {
            try
            {
                var messages = mailboxString.Split(emailDelimiter, StringSplitOptions.RemoveEmptyEntries);
                foreach (var message in messages)
                {
                    ParseMessage(message, mailbox);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        private string[] SplitHeaderLine(string line)
        {
            try
            {
                return line.Split(": ", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                return new string[] { };
            }
        }

        private void ParseMessage(string message, Mailbox mailbox)
        {
            try
            {
                EmbeddedImages.Clear();
                Data.EmailMessage email = new();
                email.MailboxName = mailbox.Name;

                var lines = message.Split("\r\n").ToList();
                if (lines.Count == 0)
                {
                    return;
                }

                // The first line in a Qualcomm Eudora email message is the "From " line,
                // which is an abomination of a string that contains sender address and
                // the timestamp, in a rather funky format with day of week in two forms.
                // We need to parse this line to extract the date and time of the email.

                string qualcommMess = lines.First();
                qualcommMess = qualcommMess.Replace("From ", "");
                var chars = qualcommMess.ToCharArray();
                int spaceCount = 0;
                for (int i = 0; i < chars.Length; i++)
                {
                    if (chars[i] == ' ')
                    {
                        spaceCount++;
                        if (spaceCount == 2)
                        {
                            string format = "MMM dd HH:mm:ss yyyy";
                            DateTime.TryParseExact(qualcommMess.Substring(i + 1), format, null, System.Globalization.DateTimeStyles.None, out DateTime date);
                            email.Date = date;
                            break;
                        }
                    }
                }
                // Prune this line once we're done parsing it.
                lines.RemoveAt(0);


                // Now we loop through the lines of the email message to extract the headers
                // At the same time, we count the lines comprising the header section.
                // Later we'll prune that chunk of the message so we can focus on the body,
                // which can exist in either plain text or HTML.
                int prunable = 0;
                for (int i = 0; i < lines.Count; i++)
                {
                    string line = lines[i];
                    if (line.Length == 0)
                    {
                        // NOTE: don't count the blank lines as prunable;
                        // these are part of the email message body.
                        continue;
                    }

                    else if (line.StartsWith("To: "))
                    {
                        ++prunable;
                        var to = SplitHeaderLine(line);
                        if (to.Length == 2)
                        {
                            var recipients = to[1].Split(",", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                            if (recipients.Length > 0)
                            {
                                foreach (var recipient in recipients)
                                {
                                    email.Addresses_To.Add(new(string.Empty, recipient));
                                }
                            }
                        }
                    }
                    else if (line.StartsWith("From: "))
                    {
                        ++prunable;
                        var from = SplitHeaderLine(line);
                        if (from.Length == 2)
                        {
                            string displayAddress = from[1];
                            var components = displayAddress.Split("<", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                            EmailAddress sender = new();
                            foreach (var component in components)
                            {
                                if (component.Contains("@"))
                                {
                                    sender.Address = component.Replace("<", "").Replace(">", "");
                                }
                                else
                                {
                                    sender.Name = component;
                                }
                            }
                            email.SenderAddress = sender;
                        }
                    }
                    else if (line.StartsWith("Cc: "))
                    {
                        ++prunable;
                        var cc = SplitHeaderLine(line);
                        if (cc.Length == 2)
                        {
                            var ccs = cc[1].Split(",", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                            if (ccs.Length > 0)
                            {
                                foreach (var c in ccs)
                                {
                                    email.Addresses_CC.Add(new(string.Empty, c));
                                }
                            }
                        }
                    }
                    else if (line.StartsWith("Bcc: "))
                    {
                        ++prunable;
                        var bcc = SplitHeaderLine(line);
                        if (bcc.Length == 2)
                        {
                            var bccs = bcc[1].Split(",", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                            if (bccs.Length > 0)
                            {
                                foreach (var b in bccs)
                                {
                                    email.Addresses_BCC.Add(new(string.Empty, b));
                                }
                            }
                        }
                    }
                    else if (line.StartsWith("Subject: "))
                    {
                        ++prunable;
                        email.Subject = line.Replace("Subject: ", "");
                    }
                    else if (line.StartsWith("Message-Id: "))
                    {
                        ++prunable;
                        string messageId = line.Replace("Message-Id: ", "").Replace("<", "").Replace(">", "");
                        email.MessageId = messageId;
                    }
                    else if (line.StartsWith("X-Attachments: "))
                    {
                        ++prunable;
                    }
                    else if (line.StartsWith("In-Reply-To: "))
                    {
                        ++prunable;
                    }
                    else if (line.StartsWith("References: "))
                    {
                        ++prunable;
                    }
                    else if (line.StartsWith("X-EmbeddedContent: "))
                    {
                        ++prunable;

                        // Embedded content
                        var embeddings = SplitHeaderLine(line);
                        if (embeddings.Length == 2)
                        {
                            embeddings[1] = embeddings[1].Replace("\n", "").Replace("\r\n", "");
                            var cids = embeddings[1].Split(" ", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                            if (cids.Length > 0)
                            {
                                foreach (var cid in cids)
                                {
                                    var parts = cid.Split("=", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                                    if (parts.Length == 2)
                                    {
                                        string id = parts[0].Replace(">", "").Replace("<", "");
                                        string src = parts[1].Replace(">", "").Replace("<", "");

                                        if (!id.StartsWith("cid:"))
                                        {
                                            id = "cid:." + parts[0];
                                        }
                                        ImportedEmbeddedContent content = new()
                                        {
                                            ID = id,
                                            Source = src                                            
                                        };
                                        EmbeddedImages.Add(content);
                                    }
                                }
                            }
                        }
                    }
                }

                // Now that we've parsed the headers, skip this message if it already exists
                // here in the corresponding Eudora.Net mailbox
                foreach (var msg in mailbox.Messages)
                {
                    if (msg.MessageId == email.MessageId)
                    {
                        return;
                    }
                }

                // Trimming out the lines that constitute the variable-length header,
                // what's left should be the body of the email message.
                if (prunable > 0)
                {
                    lines.RemoveRange(0, prunable);
                }

                // Now we have the body of the email message, which may be in HTML format.
                // Prune out the markup that we don't need or want.
                List<string> toPrune = [];
                foreach (var line in lines)
                {
                    if (line.StartsWith("<html>", StringComparison.CurrentCultureIgnoreCase))
                    {
                        toPrune.Add(line);
                    }
                    else if (line.StartsWith("<head>", StringComparison.CurrentCultureIgnoreCase))
                    {
                        toPrune.Add(line);
                    }
                    else if (line.StartsWith("</head>", StringComparison.CurrentCultureIgnoreCase))
                    {
                        toPrune.Add(line);
                    }
                    else if (line.StartsWith("<body>", StringComparison.CurrentCultureIgnoreCase))
                    {
                        toPrune.Add(line);
                    }
                    else if (line.StartsWith("</body>", StringComparison.CurrentCultureIgnoreCase))
                    {
                        toPrune.Add(line);
                    }
                    else if (line.StartsWith("</html>", StringComparison.CurrentCultureIgnoreCase))
                    {
                        toPrune.Add(line);
                    }
                }
                foreach (var prune in toPrune)
                {
                    lines.Remove(prune);
                }

                foreach (var line in lines)
                {
                    email.Body += line;// + "\r\n";

                    // Now replace the CID references with the actual paths to the embedded content
                    foreach (var embedding in EmbeddedImages)
                    {
                        email.Body = email.Body.Replace(embedding.ID, embedding.Source);
                    }
                }

                // Finally, save this email to the mailbox
                mailbox.AddMessage(email);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }
    }
}