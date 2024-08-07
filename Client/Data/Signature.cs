using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using Eudora.Net.Core;
using SQLite;

namespace Eudora.Net.Data
{
    public class Signature : INotifyPropertyChanged
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
        #region Fields
        /////////////////////////////

        private Guid _Id = Guid.NewGuid();
        private string _Name = string.Empty;
        private string _Content = string.Empty;

        /////////////////////////////
        #endregion Fields
        ///////////////////////////////////////////////////////////


        ///////////////////////////////////////////////////////////
        #region Properties
        /////////////////////////////

        [SQLite.PrimaryKey]
        public Guid Id
        {
            get => _Id;
            set => SetField(ref _Id, value, nameof(Id));
        }

        public string Name
        {
            get => _Name;
            set => SetField(ref _Name, value, nameof(Name));
        }
        
        public string Content
        {
            get => _Content;
            set => SetField(ref _Content, value, nameof(Content));
        }

        /////////////////////////////
        #endregion Properties
        ///////////////////////////////////////////////////////////
        

        public Signature() 
        { }

        public Signature(string name, string content)
        {
            Name = name;
            Content = content;
        }
    }

    internal static class SignatureManager
    {
        public static DatastoreBase<Signature> Datastore;

        static SignatureManager()
        {
            Datastore = new("Data", "Signatures", "Signatures");
        }

        public static void Startup()
        {
            try
            {
                Datastore.Open();
                Datastore.Load();
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
                Datastore.Close();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        public static Signature New(string name)
        {
            Signature signature = new() { Name = name };
            Datastore.Add(signature);
            return signature;
        }

        public static void Remove(Signature signature)
        {
            Datastore.Delete(signature);
        }

        public static bool Contains(string name)
        {
            if(Datastore.Data.Any(x => x.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase))) return true;
            return false;
        }

    }
}
