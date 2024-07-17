using Eudora.Net.Core;
using SQLite;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Runtime.CompilerServices;

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

        [Ignore]
        public DatastoreBase<Contact> Contacts { get; private set; }


        [PrimaryKey]
        public int Id { get; set; }

        private string _Title = string.Empty;
        public string Title
        {
            get => _Title;
            set => SetField(ref _Title, value, nameof(Title));
        }

        /////////////////////////////
        #endregion Properties
        ///////////////////////////////////////////////////////////

        public AddressBook()
        {
        }

        public AddressBook(string title)
        {
            _Title = title;
        }

        public void NewContact()
        {
            Contact contact = new() { AddressBookName = Title };
            Contacts.Add(contact);
        }

        public void RemoveContact(Contact contact)
        {
            Contacts.Remove(contact);
        }
        
        public void Load()
        {
            try
            {
                Contacts = new("Data", "AddressBooks", Title);
                Contacts.Open();
                Contacts.Load();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        public void Close()
        {
            try
            {
                Contacts.Close();
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
                Contacts.Remove(contact);
            }
            catch(Exception ex)
            {
                Logger.LogException(ex);
            }
        }
    }

    internal static class AddressBookManager
    {
        public static DatastoreBase<AddressBook> Datastore;

        static AddressBookManager()
        {
            Datastore = new("Data", "AddressBooks", "AddressBooks");
        }

        public static void Startup()
        {
            try
            {
                Datastore.Open();
                Datastore.Load();

                foreach(AddressBook book in Datastore.Data)
                {
                    book.Load();
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        public static void Shutdown()
        {
            try
            {
                foreach (AddressBook book in Datastore.Data)
                {
                    book.Close();
                }
                Datastore.Close();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        public static void New(string name)
        {
            AddressBook book = new(name);
            book.Load();
            Add(book);
        }

        public static void Add(AddressBook book)
        {
            Datastore.Data.Add(book);
        }

        public static void Remove(AddressBook book)
        {
            Datastore.Data.Remove(book);
        }

        public static AddressBook? Get(string name)
        {
            return Datastore.Data.Where(i => i.Title.Equals(name, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
        }

        public static bool Contains(string title)
        {
            if (Datastore.Data.Any(x => x.Title.Equals(title, StringComparison.CurrentCultureIgnoreCase))) return true;
            return false;
        }
    }
}
