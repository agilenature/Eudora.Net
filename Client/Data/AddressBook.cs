using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Eudora.Net.Core;

namespace Eudora.Net.Data
{
    /// <summary>
    /// 
    /// </summary>
    public class AddressBook : INotifyPropertyChanged
    {
        ///////////////////////////////////////////////////////////
        #region INotifyPropertyChanged
        /////////////////////////////

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SetField<TField>(ref TField field, TField value, string propertyName)
        {
            if (EqualityComparer<TField>.Default.Equals(field, value))
            {
                return;
            }

            field = value;
            //OnPropertyChanged(propertyName);
        }

        /////////////////////////////
        #endregion INotifyPropertyChanged
        ///////////////////////////////////////////////////////////
        

        ///////////////////////////////////////////////////////////
        #region Properties
        /////////////////////////////

        public static readonly string extension = ".abk";
        private readonly string DataRoot = string.Empty;
        private object Locker = new();

        private string _Title = string.Empty;
        public string Title
        {
            get => _Title;
            set => SetField(ref _Title, value, nameof(Title));
        }

        public ObservableCollection<Contact> Contacts { get; set; } = [];

        /////////////////////////////
        #endregion Properties
        ///////////////////////////////////////////////////////////

        public AddressBook(string title)
        {
            _Title = title;
            DataRoot = Path.Combine(AddressBookManager.DataRoot, Title);
            IoUtil.EnsureFolder(DataRoot);
        }

        private void Contact_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is Contact contact)
            {
                Save(contact);
            }
        }

        public void NewContact()
        {
            Contact contact = new() { AddressBookName = Title };
            contact.PropertyChanged += Contact_PropertyChanged;
            Contacts.Add(contact);
            Save(contact);
        }

        public void RemoveContact(Contact contact)
        {
            Contacts.Remove(contact);
            Delete(contact);
        }
        
        public void Load()
        {
            try
            {
                lock (Locker)
                {
                    DirectoryInfo di = new(DataRoot);
                    string searchQuery = string.Format("*{0}", Contact.extension);
                    var files = di.GetFiles(searchQuery);
                    foreach (var file in files)
                    {
                        if (file.DirectoryName == null)
                        {
                            continue;
                        }

                        string raw = File.ReadAllText(file.FullName);
                        var contact = JsonSerializer.Deserialize<Contact>(raw);
                        if (contact is not null)
                        {
                            Contacts.Add(contact);
                            contact.PropertyChanged += Contact_PropertyChanged;
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        public void Save(Contact contact)
        {
            try
            {
                lock (Locker)
                {
                    string fullPath = MakeFullPath(contact.Name);
                    string json = JsonSerializer.Serialize<Contact>(contact, IoUtil.JsonWriterOptions);
                    File.WriteAllText(fullPath, json);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        public void Delete(Contact contact)
        {
            try
            {
                lock(Locker)
                {
                    Contacts.Remove(contact);
                    string fullPath = MakeFullPath(contact.Name);
                    File.Delete(fullPath);
                }
            }
            catch(Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        private string MakeFilename(string name)
        {
            return string.Format("{0}{1}", name, Contact.extension);
        }

        private string MakeFullPath(string name)
        {
            return Path.Combine(DataRoot, MakeFilename(name));
        }

        private string MakeFolder(string name)
        {
            return Path.Combine(DataRoot, name);
        }
    }

    public static class AddressBookManager
    {
        public static readonly string DataRoot = string.Empty;
        private static object Locker;

        public static ObservableCollection<AddressBook> Collection { get; private set; } = [];

        static AddressBookManager()
        {
            Locker = new();
            DataRoot = Path.Combine(Eudora.Net.Properties.Settings.Default.DataStoreRoot, "AddressBooks");
            IoUtil.EnsureFolder(DataRoot);
        }

        public static void Startup()
        {
            Load();
        }

        public static void Shutdown()
        {

        }

        private static void AddressBook_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is AddressBook book)
            {
                Save(book);
            }
        }

        private static string MakeFilename(string name)
        {
            return string.Format("{0}{1}", name, AddressBook.extension);
        }

        private static string MakeFullPath(string name)
        {
            return Path.Combine(DataRoot, MakeFilename(name));
        }

        private static string MakeFolder(string name)
        {
            return Path.Combine(DataRoot, name);
        }

        public static AddressBook? Get(string name)
        {
            return Collection.Where(i => i.Title.Equals(name, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
        }

        public static void New(string name)
        {
            AddressBook book = new(name);
            Add(book);
        }

        public static void Add(AddressBook book)
        {
            Collection.Add(book);
            Save(book);
        }

        public static void Remove(AddressBook book)
        {
            Collection.Remove(book);

            try
            {
                string fullPath = MakeFullPath(book.Title);
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        public static bool Contains(string title)
        {
            if (Collection.Any(x => x.Title.Equals(title, StringComparison.CurrentCultureIgnoreCase))) return true;
            return false;
        }

        private static void Load()
        {
            try
            {
                lock (Locker)
                {
                    DirectoryInfo di = new(DataRoot);
                    string searchQuery = string.Format("*{0}", AddressBook.extension);
                    var files = di.GetFiles(searchQuery);
                    foreach (var file in files)
                    {
                        if (file.DirectoryName == null)
                        {
                            continue;
                        }

                        string raw = File.ReadAllText(file.FullName);
                        var book = JsonSerializer.Deserialize<AddressBook>(raw);
                        if (book is not null)
                        {
                            book.PropertyChanged += AddressBook_PropertyChanged;
                            book.Load();
                            Collection.Add(book);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        public static void Save(AddressBook book)
        {
            try
            {
                lock (Locker)
                {
                    string fullPath = MakeFullPath(book.Title);
                    string json = JsonSerializer.Serialize<AddressBook>(book, IoUtil.JsonWriterOptions);
                    File.WriteAllText(fullPath, json);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }
    }

}
