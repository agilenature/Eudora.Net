using Eudora.Net.Core;
using SQLite;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

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
            OnPropertyChanged(propertyName);
        }

        /////////////////////////////
        #endregion INotifyPropertyChanged
        ///////////////////////////////////////////////////////////


        ///////////////////////////////////////////////////////////
        #region Properties
        /////////////////////////////

        [Ignore]
        public DatastoreBase<Contact> Datastore { get; private set; }


        private Guid _Id = Guid.NewGuid();
        [PrimaryKey]
        public Guid Id
        {
            get => _Id;
            set => SetField(ref _Id, value, nameof(Id));
        }

        private string _Name = "unnamed";
        public string Name
        {
            get => _Name;
            set => SetField(ref _Name, value, nameof(Name));
        }

        /////////////////////////////
        #endregion Properties
        ///////////////////////////////////////////////////////////

        public AddressBook()
        {
        }

        public AddressBook(string name)
        {
            Datastore = new("Data", "AddressBooks", name);
            Name = name;
        }

        public void NewContact()
        {
            Contact contact = new() { AddressBookName = Name };
            Datastore.Add(contact);
        }

        public void RemoveContact(Contact contact)
        {
            Datastore.Remove(contact);
        }
        
        public void Load()
        {
            try
            {
                if(Datastore is null)
                {
                    Datastore = new("Data", "AddressBooks", Name);
                }
                Datastore.Open();
                Datastore.Load();
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
                Datastore.Close();
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
                Datastore.Remove(contact);
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
        public static ObservableCollection<AddressBook> Data => Datastore.Data;

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
            Add(book);
            book.Load();            
        }

        public static void Add(AddressBook book)
        {
            Datastore.Add(book);
        }

        public static void Remove(AddressBook book)
        {
            Datastore.Remove(book);
        }

        public static AddressBook? Get(string name)
        {
            return Datastore.Data.Where(i => i.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
        }

        public static bool Contains(string name)
        {
            if (Datastore.Data.Any(x => x.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase))) return true;
            return false;
        }
    }
}
