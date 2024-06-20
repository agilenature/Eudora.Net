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
        #endregion FIELDS


        #region PROPERTIES
        public string EudoraDataPath
        {
            get => _EudoraDataPath;
            set => _EudoraDataPath = value;
        }
        #endregion PROPERTIES


        public bool LocateEudoraData()
        {
            string user = Environment.UserName;
            string defaultEudoraDataPath = DefaultEudoraDataPath.Replace("{0}", user);

            if (Directory.Exists(defaultEudoraDataPath))
            {
                EudoraDataPath = defaultEudoraDataPath;
                return true;
            }

            return false;
        }

        public bool ImportAccounts()
        {
            return false;
        }

        public bool ImportMailboxes()
        {
            DirectoryInfo di = new(EudoraDataPath);
            string searchQuery = string.Format("*{0}", mailboxExtension);
            var files = di.GetFiles(searchQuery);
            if (files.Length == 0)
            {
                return false;
            }

            foreach (var mailbox in files)
            {
                //using (StreamReader sr = new(mailbox.FullName))
                //{
                //    List<string> lines = [];
                //    string? line;
                //    while ((line = sr.ReadLine()) != null)
                //    {
                //        lines.Add(line);
                //    }

                //    //Parallel.Invoke(() => ParseMailbox(lines));
                //    ParseMailbox(lines);
                //}

                string mbtext = File.ReadAllText(mailbox.FullName);
                ParseMailbox(mbtext);
            }

            return true;
        }

        private void ParseMailbox(string mailbox)
        {
            var messages = mailbox.Split(emailDelimiter, StringSplitOptions.RemoveEmptyEntries);
            foreach (var message in messages)
            {
                ParseMessage(message);
            }
        }

        //private void ParseMailbox(List<string> lines)
        //{
        //    var messages = string.Join(emailDelimiter, lines).Split(emailDelimiter);
        //    //var messages = string.Split(emailDelimiter);
        //    foreach (var message in messages)
        //    {
        //        ParseMessage(message);
        //    }
        //}

        private string[] SplitHeaderLine(string line)
        {
            return line.Split(": ", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        }

        private void ParseMessage(string message)
        {
            Data.EmailMessage email = new();

            var lines = message.Split("\r\n");
            if (lines.Length == 0)
            {
                return;
            }

            // The first line in a Qualcomm Eudora email message is the "From " line,
            // which is an abomination of a string which contains sender address and
            // the timestamp, in a rather funky format with redundant day of week.
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


            foreach (var line in lines)
            {
                if (line.Length == 0)
                {
                    continue;
                }

                else if (line.StartsWith("To: "))
                {
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
                else if (line.StartsWith("Bcc"))
                {
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
                    email.Subject = line.Replace("Subject: ", "");
                }
                //else if (line.StartsWith("Date: "))
                //{
                //    DateTime.TryParse(line.Replace("Date: ", ""), out DateTime date);
                //    email.Date = date;
                //}
                else if (line.StartsWith("Message-Id: "))
                {
                    email.MessageId = line.Replace("Message-Id: ", "");
                }
            }

            int breakhere = 0;
        }

    }
}