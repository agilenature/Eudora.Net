﻿using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Eudora.Net.Data;


namespace Eudora.Net.Core
{
    public class Mailbox
    {
        ///////////////////////////////////////////////////////////
        #region Properties
        /////////////////////////////

        public static readonly string extension = ".mbx";
        private string DataRoot = string.Empty;
        private object FileLocker = new();
        private object CollectionLocker = new();

        [JsonIgnore]
        public ObservableCollection<EmailMessage> Messages { get; set; } = [];

        private string _Name = string.Empty;
        public string Name
        {
            get => _Name;
            set { if (_Name != value) { _Name = value; InitializeForIO(); } }
        }

        private string _ImageSource = string.Empty;
        public string ImageSource
        {
            get => _ImageSource;
            set { if (_ImageSource != value) { _ImageSource = value; } }
        }

        /////////////////////////////
        #endregion Properties
        ///////////////////////////////////////////////////////////


        public Mailbox()
        {
        }

        public Mailbox(string name, string imageSource) 
        {
            _Name = name;
            _ImageSource = imageSource;
            InitializeForIO();
        }

        private void InitializeForIO()
        {
            DataRoot = Path.Combine(PostOffice.MailboxesPath, Name);
            IoUtil.EnsureFolder(DataRoot);
        }

        public void Delete()
        {
            try
            {
                DirectoryInfo di = new(DataRoot);
                di.Delete(true);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        ///////////////////////////////////////////////////////////
        #region File handling
        /////////////////////////////

        private string MakeFilename(string name)
        {
            return string.Format("{0}{1}", name, EmailMessage.extension);
        }

        private string MakeFullPath(string name)
        {
            return Path.Combine(DataRoot, MakeFilename(name));
        }

        private string MakeAttachmentsFolder(string name)
        {
            return Path.Combine(DataRoot, name);
        }

        private void Message_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender is EmailMessage message)
            {
                SaveMessage(message);
            }
        }

        public void Load()
        {
            try
            {
                lock (FileLocker)
                {
                    DirectoryInfo di = new(DataRoot);
                    string searchQuery = string.Format("*{0}", EmailMessage.extension);
                    var files = di.GetFiles(searchQuery);
                    foreach (var file in files)
                    {
                        if (file.DirectoryName == null)
                        {
                            continue;
                        }

                        string raw = File.ReadAllText(file.FullName);
                        var message = JsonSerializer.Deserialize<EmailMessage>(raw);
                        if (message is not null)
                        {
                            message.PropertyChanged += Message_PropertyChanged;
                            Messages.Add(message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        public EmailMessage NewMessage(bool draft = false)
        {
            EmailMessage message = new() 
            { 
                MailboxName = Name,
                Status = draft ? EmailMessage.MessageStatus.Draft : EmailMessage.MessageStatus.Sealed
            };
            message.PropertyChanged += Message_PropertyChanged;
            Messages.Add(message);
            SaveMessage(message);
            return message;
        }

        public void AddMessage(EmailMessage message)
        {
            if (!App.Current.Dispatcher.CheckAccess())
            {
                App.Current.Dispatcher.BeginInvoke(new Action(() => AddMessage(message)));
                return;
            }
            message.PropertyChanged -= Message_PropertyChanged;
            message.PropertyChanged += Message_PropertyChanged;
            Messages.Add(message);
            SaveMessage(message);
        }

        public void SaveMessage(EmailMessage message)
        {
            try
            {
                lock (FileLocker)
                {
                    string fullPath = MakeFullPath(message.InternalId.ToString());
                    string json = JsonSerializer.Serialize<EmailMessage>(message, IoUtil.JsonWriterOptions);
                    File.WriteAllText(fullPath, json);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        public EmailMessage? LoadMessage(string fullPath)
        {
            EmailMessage? message = null;
            try
            {
                lock (FileLocker)
                {
                    string raw = File.ReadAllText(fullPath);
                    message = JsonSerializer.Deserialize<EmailMessage>(raw);
                    if (message is not null)
                    {
                        message.PropertyChanged += Message_PropertyChanged;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return message;
        }

        public void UnloadMessage(EmailMessage message)
        {
            try
            {
                Messages.Remove(message);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        public void DeleteMessage(EmailMessage message)
        {
            try
            {
                Messages.Remove(message);
                
                // message
                string path = MakeFullPath(message.InternalId.ToString());
                if(File.Exists(path))
                {
                    File.Delete(path);
                }
                // attachments
                path = MakeAttachmentsFolder(message.InternalId.ToString());
                if (Directory.Exists(path))
                {
                    Directory.Delete(path);
                }
            }
            catch(Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        /////////////////////////////
        #endregion File handling
        ///////////////////////////////////////////////////////////
    }
}
