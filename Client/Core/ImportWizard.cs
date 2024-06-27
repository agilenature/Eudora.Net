using Eudora.Net.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
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
        private static string emailDelimiter = "From ???@???";
        private List<ImportedEmbeddedContent> EmbeddedImages = [];
        MimeKit.ParserOptions mkParserOptions;
        #endregion FIELDS


        #region PROPERTIES
        public string EudoraDataPath
        {
            get => _EudoraDataPath;
            set => _EudoraDataPath = value;
        }
        #endregion PROPERTIES


        public ImportWizard()
        {
            mkParserOptions = new()
            {
                ParameterComplianceMode = MimeKit.RfcComplianceMode.Looser,
                AddressParserComplianceMode = MimeKit.RfcComplianceMode.Looser,
                AllowAddressesWithoutDomain = true,
                RespectContentLength = false,
                AllowUnquotedCommasInAddresses = true,
                Rfc2047ComplianceMode = MimeKit.RfcComplianceMode.Looser
            };
        }

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
        // Also known as, "An example of how derpy the ini file format can be"
        public bool ImportAccounts()
        {
            try
            {
                string iniFilePath = Path.Combine(EudoraDataPath, "eudora.ini");
                if (!File.Exists(iniFilePath))
                {
                    return false;
                }

                List<string> lines = File.ReadAllLines(iniFilePath).ToList();
                if (lines.Count == 0)
                {
                    return false;
                }
                

                // The default or "dominant" account is the first one listed in the eudora.ini file,
                // not found in the "personalities" section but loose in the "settings" section. Sigh.
                List<string> account = [];
                for (int i = 0; i < lines.Count; i++)
                {
                    if (lines[i].StartsWith("POPAccount"))
                    {
                        account.Add(lines[i]);
                        for (int j = i + 1; j < lines.Count; j++)
                        {
                            account.Add(lines[j]);
                            if (lines[j].StartsWith("SavePassword"))
                            {
                                break;
                            }
                        }
                        ImportAccount(account);
                        break;
                    }
                }

                account.Clear();

                // Now we move on through the ini file to the "personalities" section,
                // there to find the other accounts
                List<string> AccountTokens = [];
                int index = lines.IndexOf("[Personalities]");
                for(int i = index + 1; i < lines.Count; i++)
                {
                    if(lines[i].StartsWith("["))
                    {
                        break;
                    }
                    AccountTokens.Add(lines[i]);
                }

                foreach (var token in AccountTokens)
                {
                    index = lines.IndexOf(token);
                    if (token.StartsWith("SavePassword"))
                    {
                        ImportAccount(account);
                        account.Clear();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                return false;
            }

            return false;
        }

        private void ImportAccount(List<string> lines)
        {

        }

        // STEP 2: Import Mailboxes & Mail
        private Mailbox? GetMailboxFromFilename(string filename)
        {
            try
            {
                string name = Path.GetFileNameWithoutExtension(filename);

                // default mailboxes
                if (name.Equals("in", StringComparison.CurrentCultureIgnoreCase) ||
                    name.Equals("inbox", StringComparison.CurrentCultureIgnoreCase))
                {
                    return Mailbox.Inbox;
                }
                else if (name.Equals("out", StringComparison.CurrentCultureIgnoreCase) ||
                         name.Equals("outbox", StringComparison.CurrentCultureIgnoreCase))
                {
                    return Mailbox.Outbox;
                }
                else if (name.Equals("junk", StringComparison.CurrentCultureIgnoreCase))
                {
                    return Mailbox.Junk;
                }
                else if (name.Equals("trash", StringComparison.CurrentCultureIgnoreCase))
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
                if (Directory.Exists(EudoraDataPath))
                {
                    var files = Directory.GetFiles(EudoraDataPath, "*" + mailboxExtension, SearchOption.AllDirectories);
                    foreach (var file in files)
                    {
                        if (File.Exists(file))
                        {
                            Mailbox? mailbox = GetMailboxFromFilename(file);
                            if (mailbox is null)
                            {
                                continue;
                            }
                            string mbtext = File.ReadAllText(file);
                            ParseMailbox3(mbtext, mailbox);
                        }
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

        // Split the blob into individual email messages
        private void ParseMailbox3(string mailboxBlob, Mailbox mailbox)
        {
            try
            {
                var messages = mailboxBlob.Split(emailDelimiter, StringSplitOptions.RemoveEmptyEntries);
                foreach (var message in messages)
                {
                    ParseMessage3(message, mailbox);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        private void ParseMessage3(string message, Mailbox mailbox)
        {
            try
            {
                List<string> lines = message.Split("\r\n").ToList();

                var index = lines.IndexOf(string.Empty);
                List<string> headers = lines.Take(index).ToList();
                List<string> body = lines.Skip(index).ToList();

                Data.EmailMessage email = new();
                email.MailboxName = mailbox.Name;

                ParseEudoraHeaders3(headers, ref email);
                ParseEudoraBody3(body, ref email);

                if (!mailbox.Contains(email))
                {
                    mailbox.AddMessage(email);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        private string[] SplitHeader(string header)
        {
            string[] parts = [string.Empty, string.Empty];

            try
            {
                var index = header.IndexOf(":");
                if (index > 0)
                {
                    parts[0] = header.Substring(0, index);
                    parts[1] = header.Substring(index + 1);
                }
            }
            catch(Exception ex)
            {
                Logger.LogException(ex);
            }

            return parts;
        }

        private Data.EmailAddress ParseEmailAddress(string displayAddress)
        {
            EmailAddress address = new();
            
            // Format 0: Name <address>
            if (displayAddress.Contains("<") && displayAddress.Contains(">"))
            {
                var components = displayAddress.Split("<", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                if (components.Length == 2)
                {
                    string name = components[0];
                    string addy = components[1].Replace(">", "");
                    address.Name = name;
                    address.Address = addy;
                }
            }

            // Format 1: address
            else
            {
                address.Address = displayAddress;
            }

            return address;
        }

        private List<Data.EmailAddress> SplitAddressList(string addresses)
        {
            List<Data.EmailAddress> list = new();

            try
            {
                var parts = addresses.Split(",", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 0)
                {
                    foreach (var part in parts)
                    {
                        list.Add(ParseEmailAddress(part));
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }

            return list;
        }

        private void ParseEudoraHeaders3(List<string> headers, ref Data.EmailMessage email)
        {
            // Line 0 - date (and possibly the funky Qualcomm mail msg delimiter)
            string line0 = headers.First();
            line0 = line0.Replace("From ???@???", "").Trim();
            line0 = line0.Substring(4);
            string format = "MMM dd HH:mm:ss yyyy";
            DateTime.TryParseExact(line0, format, null, System.Globalization.DateTimeStyles.None, out DateTime date);
            email.Date = date;

            // Remainder of headers; order and content may vary
            foreach (string header in headers)
            {
                var parts = SplitHeader(header);

                if (header.StartsWith("From:", StringComparison.CurrentCultureIgnoreCase))
                {
                    email.SenderAddress = ParseEmailAddress(parts[1]);
                }

                else if (header.StartsWith("To:", StringComparison.CurrentCultureIgnoreCase))
                {
                    foreach (var address in SplitAddressList(parts[1]))
                    {
                        email.Addresses_To.Add(address);
                    }
                }

                else if(header.StartsWith("Cc:", StringComparison.CurrentCultureIgnoreCase))
                {
                    foreach (var address in SplitAddressList(parts[1]))
                    {
                        email.Addresses_CC.Add(address);
                    }
                }

                else if (header.StartsWith("Bcc:", StringComparison.CurrentCultureIgnoreCase))
                {
                    foreach (var address in SplitAddressList(parts[1]))
                    {
                        email.Addresses_BCC.Add(address);
                    }
                }

                else if (header.StartsWith("Subject:", StringComparison.CurrentCultureIgnoreCase))
                {
                    email.Subject = parts[1];
                }
                
                else if (header.StartsWith("Message-Id:", StringComparison.CurrentCultureIgnoreCase))
                {
                }
            }
        }

        private void ParseEudoraBody3(List<string> body, ref Data.EmailMessage email)
        {
            try
            {
                foreach (var line in body)
                {
                    // filter out markup we don't need
                    if (line.Contains("<html>", StringComparison.CurrentCultureIgnoreCase))
                    {
                        continue;
                    }
                    else if (line.Contains("<x-html>", StringComparison.CurrentCultureIgnoreCase))
                    {
                        continue;
                    }
                    else if (line.Contains("<head>", StringComparison.CurrentCultureIgnoreCase))
                    {
                        continue;
                    }
                    else if (line.Contains("</head>", StringComparison.CurrentCultureIgnoreCase))
                    {
                        continue;
                    }
                    else if (line.Contains("<body>", StringComparison.CurrentCultureIgnoreCase))
                    {
                        continue;
                    }
                    else if (line.Contains("</body>", StringComparison.CurrentCultureIgnoreCase))
                    {
                        continue;
                    }
                    else if (line.Contains("</html>", StringComparison.CurrentCultureIgnoreCase))
                    {
                        continue;
                    }
                    else if (line.Contains("</x-html>", StringComparison.CurrentCultureIgnoreCase))
                    {
                        continue;
                    }
                    else
                    {
                        email.Body += line;
                    }                    
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }




        private void ParseMailbox2(string mailboxString, Mailbox mailbox)
        {
            try
            {
                var messages = mailboxString.Split(emailDelimiter, StringSplitOptions.RemoveEmptyEntries);
                foreach (var message in messages)
                {
                    ParseMessage2(message, mailbox);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        private void ParseMessage2(string message, Mailbox mailbox)
        {
            try
            {
                //message = message.Replace("???@???", "").Trim();
                //message = message.Replace("\r\n", "\n").Trim();
                if (!message.StartsWith(emailDelimiter))
                {
                    message = emailDelimiter + message;
                }
                message = message.Trim();

                //string tempFile = TempFileManager.CreateTempFileFromStringContent2(message);
                //MimeKit.MimeMessage mime = MimeKit.MimeMessage.Load(mkParserOptions, tempFile);
                using MemoryStream stream = new(Encoding.UTF8.GetBytes(message));
                stream.Position = 0;
                var mime = MimeKit.MimeMessage.Load(mkParserOptions, stream);
                
                var converter = new Core.MimeToMessage(mime);
                converter.Convert();
                var email = converter.Message;
                
                // Now that we've parsed the headers, skip this message if it already exists
                // here in the corresponding Eudora.Net mailbox
                foreach (var msg in mailbox.Messages)
                {
                    if (msg.MessageId == email.MessageId)
                    {
                        return;
                    }
                }
                
                // Otherwise, save this email to the mailbox
                email.MailboxName = mailbox.Name;
                mailbox.AddMessage(email);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
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
                return line.Split(":", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
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
                        string attachments = line.Replace("X-Attachments:", "").Trim();
                        if(attachments.Length > 0)
                        {
                            int breakhere = 90;
                        }
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