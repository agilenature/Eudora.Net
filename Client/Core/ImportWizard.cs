using Eudora.Net.Data;
using Eudora.Net.ExtensionMethods;
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
                for (int i = index + 1; i < lines.Count; i++)
                {
                    if (lines[i].StartsWith("["))
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
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }

            return parts;
        }

        private Data.EmailAddress ParseEmailAddress(string displayAddress)
        {
            EmailAddress address = new();

            try
            {// Format 0: Name <address>
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
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
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
            try
            {// Line 0 - date (and possibly the funky Qualcomm mail msg delimiter)
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

                    else if (header.StartsWith("Cc:", StringComparison.CurrentCultureIgnoreCase))
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
                        email.MessageId = parts[1].Replace("<", "").Replace(">", "").Trim();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        private void ParseAttachment(string line, Data.EmailMessage email)
        {
            try
            {
                string attachmentFolder = Path.Combine(PostOffice.MailboxesPath, email.MailboxName, email.InternalId.ToString());
                IoUtil.EnsureFolder(attachmentFolder);

                string srcPath = line.Replace("Attachment Converted:", "").Replace("\"", "").Trim();
                string destPath = Path.Combine(attachmentFolder, Path.GetFileName(srcPath));
                File.Copy(srcPath, destPath);

                Data.EmailAttachment attachment = new();
                attachment.Path = destPath;
                attachment.Name = Path.GetFileName(attachment.Path);
                email.Attachments.Add(attachment);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        private void ParseEmbedded(string line, Data.EmailMessage email)
        {
            try
            {
                // strip the line to the image filename
                string fileName = line.Replace("Embedded Content:", "").Replace("\"", "").Trim();
                fileName = fileName.Substring(0, fileName.IndexOf(":"));
                string srcPath = Path.Combine(EudoraDataPath, "Embedded");
                srcPath = Path.Combine(srcPath, fileName);

                // copy the image to the mailbox folder
                string destFolder = Path.Combine(PostOffice.MailboxesPath, email.MailboxName, email.InternalId.ToString());
                IoUtil.EnsureFolder(destFolder);

                string destPath = Path.Combine(destFolder, fileName);

                File.Copy(srcPath, destPath);

                EmbeddedImage ei = new()
                {
                    Source = destPath,
                };

                email.InlineImages.Add(ei);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        private void ParseEudoraBody3(List<string> body, ref Data.EmailMessage email)
        {
            // Note to whoever might be reading this:
            // Qualcomm's email message format places statements about attachments and embedded images
            // at the end of the message. This is why we parse the message body twice. On the first pass,
            // we parse out and create/copy the attached or embedded assets. In the second pass, we make whatever
            // changes are necessary to point to the local copies of the assets, while copying the rest of the body
            // to the new email message.

            // First pass
            try
            {
                foreach (var line in body)
                {
                    // Qualcomm Eudora mail format notes the attachment thusly
                    if (line.StartsWith("Attachment Converted:", StringComparison.CurrentCultureIgnoreCase))
                    {
                        ParseAttachment(line, email);
                        continue;
                    }
                    // Embedded images are noted like this
                    else if (line.StartsWith("Embedded Content:", StringComparison.CurrentCultureIgnoreCase))
                    {
                        ParseEmbedded(line, email);
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }

            // Second pass
            try
            {

                foreach (var line in body)
                {
                    // We still need to skip over these two types
                    if (line.StartsWith("Attachment Converted:", StringComparison.CurrentCultureIgnoreCase))
                    {
                        continue;
                    }
                    else if (line.StartsWith("Embedded Content:", StringComparison.CurrentCultureIgnoreCase))
                    {
                        continue;
                    }

                    // Here we will convert the src attribute value to point to the local copy of the image
                    // Eudora stores the image filename as the alt parameter in the img tag
                    // so we just need to replace the src value with the alt value
                    // An example copied from a Eudora email:
                    // <img src="cid:ii_lrqzllcm0" alt="Amaxresdefault.jpg"
                    else if (line.Contains("<img"))
                    {
                        var indexStart = line.IndexOf("<img");
                        var indexEnd = line.IndexOf(">", indexStart);
                        var imgTag = line.Substring(indexStart, indexEnd - indexStart + 1);

                        var altstart = imgTag.IndexOf("alt=");
                        var altend = imgTag.IndexOf("\"", altstart + 5);
                        var alt = imgTag.Substring(altstart + 5, altend - altstart - 5);

                        EmbeddedImage? ei = email.InlineImages.Where(x => x.Source.Contains(alt)).FirstOrDefault();

                        if (ei is not null)
                        {
                            string newImgSrc = alt;
                            string newImgTag = $"<img src=\"{ei.Source}\">";

                            string newLine = line.Replace(imgTag, newImgTag);
                            email.Body += newLine;
                        }
                        else
                        {
                            email.Body += line;
                        }
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

        private Data.AddressBook? GetAddressBook(string bookName)
        {
            Data.AddressBook? book = null;

            try
            {
                book = AddressBookManager.Get(bookName);
                if (book is null)
                {
                    AddressBookManager.New(bookName);
                    book = AddressBookManager.Get(bookName);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return book;
        }

        private string[] SplitContactField(string fieldString)
        {
            var parts = fieldString.Split(":", StringSplitOptions.TrimEntries);
            if (parts.Length == 2)
            {
                parts[0] = parts[0].Trim().ToLower();
                parts[1] = parts[1].Replace(">", " ");
            }
            return parts;
        }

        public bool ImportContacts()
        {
            // Step 1: locate all address book files
            string bookpath = Path.Combine(EudoraDataPath, "nickname");
            List<string> addressBooks = Directory.GetFiles(bookpath, "*.txt").ToList();
            addressBooks.Add(Path.Combine(EudoraDataPath, "nndbase.txt"));

            // Step 2: parse each address book file
            foreach (var book in addressBooks)
            {
                ParseAddressBook(book);
            }

            return true;
        }

        private void ParseAddressBook(string bookPath)
        {
            try
            {
                if (File.Exists(bookPath))
                {
                    string bookName = Path.GetFileNameWithoutExtension(bookPath);
                    Data.AddressBook? book = GetAddressBook(bookName);
                    if (book is null)
                    {
                        return;
                    }

                    List<string> lines = File.ReadAllLines(bookPath).ToList();
                    string[] contactData = [string.Empty, string.Empty];
                    foreach (var line in lines)
                    {
                        if (line.StartsWith("alias"))
                        {
                            contactData[0] = line;
                        }
                        else if (line.StartsWith("note"))
                        {
                            contactData[1] = line;
                            if (contactData[0].Length > 0)
                            {
                                ParseContact(contactData, book);
                                contactData = [string.Empty, string.Empty];
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        private void ParseContact(string[] contactData, AddressBook book)
        {
            try
            {
                if (contactData[0].Length > 0 && contactData[1].Length > 0)
                {
                    string alias = contactData[0].Replace("alias", "").Trim();
                    string note = contactData[1];

                    Data.Contact contact = new();

                    // line 1 of the Eudora format
                    var lastSpace = alias.LastIndexOf(" ");
                    contact.Name = alias.Substring(0, lastSpace).Trim();
                    
                    if (lastSpace > 0)
                    {
                        string addresses = alias.Substring(lastSpace).Trim();
                        string[] splitAddresses = addresses.Split(",", StringSplitOptions.TrimEntries);
                        if(splitAddresses.Length > 0)
                        {
                            contact.EmailAddress = splitAddresses[0];
                        }
                    }
                    

                    // line 2 of the Eudora format
                    string[] fields = note.Substring(note.IndexOf("<")).Split("<", StringSplitOptions.TrimEntries);
                    foreach (var field in fields)
                    {
                        var parts = SplitContactField(field);
                        if (parts.Length != 2)
                        {
                            continue;
                        }

                        // home page
                        if (parts[0].Equals("first", StringComparison.CurrentCultureIgnoreCase))
                        {
                            contact.FirstName = parts[1];
                        }
                        else if (parts[0].Equals("last", StringComparison.CurrentCultureIgnoreCase))
                        {
                            contact.LastName = parts[1];
                        }
                        else if (parts[0].Equals("address", StringComparison.CurrentCultureIgnoreCase))
                        {
                            contact.StreetAddress_Home = parts[1];
                        }
                        else if (parts[0].Equals("city", StringComparison.CurrentCultureIgnoreCase))
                        {
                            contact.City_Home = parts[1];
                        }
                        else if (parts[0].Equals("state", StringComparison.CurrentCultureIgnoreCase))
                        {
                            contact.State_Home = parts[1];
                        }
                        else if (parts[0].Equals("country", StringComparison.CurrentCultureIgnoreCase))
                        {
                            contact.Country_Home = parts[1];
                        }
                        else if (parts[0].Equals("zip", StringComparison.CurrentCultureIgnoreCase))
                        {
                            contact.PostalCode_Home = parts[1];
                        }
                        else if (parts[0].Equals("phone", StringComparison.CurrentCultureIgnoreCase))
                        {
                            contact.Phone1_Home = parts[1];
                        }
                        else if (parts[0].Equals("mobile", StringComparison.CurrentCultureIgnoreCase))
                        {
                            contact.Phone2_Home = parts[1];
                        }
                        else if (parts[0].Equals("fax", StringComparison.CurrentCultureIgnoreCase))
                        {
                            contact.Fax_Home = parts[1];
                        }
                        else if (parts[0].Equals("web", StringComparison.CurrentCultureIgnoreCase))
                        {
                            contact.Url_Home = parts[1];
                        }

                        // work page
                        else if (parts[0].Equals("title", StringComparison.CurrentCultureIgnoreCase))
                        {
                            contact.Title_Work = parts[1];
                        }
                        else if (parts[0].Equals("company", StringComparison.CurrentCultureIgnoreCase))
                        {
                            contact.Organization_Work = parts[1];
                        }
                        else if (parts[0].Equals("address2", StringComparison.CurrentCultureIgnoreCase))
                        {
                            contact.StreetAddress_Work = parts[1];
                        }
                        else if (parts[0].Equals("city2", StringComparison.CurrentCultureIgnoreCase))
                        {
                            contact.City_Work = parts[1];
                        }
                        else if (parts[0].Equals("state2", StringComparison.CurrentCultureIgnoreCase))
                        {
                            contact.State_Work = parts[1];
                        }
                        else if (parts[0].Equals("country2", StringComparison.CurrentCultureIgnoreCase))
                        {
                            contact.Country_Work = parts[1];
                        }
                        else if (parts[0].Equals("zip2", StringComparison.CurrentCultureIgnoreCase))
                        {
                            contact.PostalCode_Work = parts[1];
                        }
                        else if (parts[0].Equals("mobile2", StringComparison.CurrentCultureIgnoreCase))
                        {
                            contact.Phone2_Work = parts[1];
                        }
                        else if (parts[0].Equals("fax2", StringComparison.CurrentCultureIgnoreCase))
                        {
                            contact.Fax_Work = parts[1];
                        }
                        else if (parts[0].Equals("web2", StringComparison.CurrentCultureIgnoreCase))
                        {
                            contact.Url_Work = parts[1];
                        }

                        // other page
                        else if (parts[0].Equals("otheremail", StringComparison.CurrentCultureIgnoreCase))
                        {
                            contact.OtherEmails = parts[1];
                        }
                        else if (parts[0].Equals("otherphone", StringComparison.CurrentCultureIgnoreCase))
                        {
                            contact.OtherPhones = parts[1];
                        }
                        else if (parts[0].Equals("otherweb", StringComparison.CurrentCultureIgnoreCase))
                        {
                            contact.OtherUrls = parts[1];
                        }
                    }

                    book.Datastore.AddUnique(contact);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }
    }
}