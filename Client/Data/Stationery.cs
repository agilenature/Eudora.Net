using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using Eudora.Net.Core;
using Eudora.Net.HtmlTemplates;
using SQLite;

namespace Eudora.Net.Data
{
    // <summary>
    // 
    // </summary>
    public class Stationery : INotifyPropertyChanged
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
        private HtmlAttribute _Style = new("style", string.Empty);

        /////////////////////////////
        #endregion Fields
        ///////////////////////////////////////////////////////////


        ///////////////////////////////////////////////////////////
        #region Properties
        /////////////////////////////

        [PrimaryKey]
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

        public HtmlAttribute Style
        {
            get => _Style;
            set => SetField(ref _Style, value, nameof(Style));
        }

        /////////////////////////////
        #endregion Properties
        ///////////////////////////////////////////////////////////

        public Stationery()
        {
        }

        public Stationery(string name, string content)
        {
            Name = name;
            Content = content;
        }
    }

    internal static class StationeryManager
    {
        public static DatastoreBase<Stationery> Datastore;

        static StationeryManager()
        {
            Datastore = new("Data", "Stationery", "Stationery");
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

        public static Stationery New(string name)
        {
            Stationery stationery = new() 
            { 
                Name = name,
                Content = HtmlDepot.html_Stationery
            };
            
            Datastore.Add(stationery);
            return stationery;
        }

        public static void Add(Stationery stationery)
        {
            Datastore.Add(stationery);
        }

        public static void Remove(Stationery stationery)
        {
            Datastore.Remove(stationery);
        }

        public static bool Contains(string name)
        {
            if (Datastore.Data.Any(x => x.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase))) return true;
            return false;
        }
    }
}
