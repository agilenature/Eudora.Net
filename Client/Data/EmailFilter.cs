using Eudora.Net.Core;
using SQLite;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Eudora.Net.Data
{
    /// <summary>
    /// An email filter or rule
    /// These will exist in an ObservableCollection for binding,
    /// which is why it inerits INotifyPropertyChanged
    /// </summary>
    public class EmailFilter : INotifyPropertyChanged
    {
        ////////////////////////////////////////////////////////////////
        #region INotifyPropertyChanged
        ////////////////////////////////////////////////////////////////

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void SetField<TField>(ref TField field, TField value, string propertyName)
        {
            if (EqualityComparer<TField>.Default.Equals(field, value))
            {
                return;
            }

            field = value;
            OnPropertyChanged(propertyName);
        }

        ////////////////////////////////////////////////////////////////
        #endregion INotifyPropertyChanged
        ////////////////////////////////////////////////////////////////


        ////////////////////////////////////////////////////////////////
        #region Fields
        ////////////////////////////////////////////////////////////////

        private Guid _Id = Guid.NewGuid();
        private string _Name = string.Empty;
        private UInt32 _Priority = 0;
        private EmailSearch.EmailSearchAtom _Atom = new();
        private EmailFilterAction _Action = new();

        ////////////////////////////////////////////////////////////////
        #endregion Fields
        ////////////////////////////////////////////////////////////////



        ////////////////////////////////////////////////////////////////
        #region Properties
        ////////////////////////////////////////////////////////////////

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

        public UInt32 Priority
        {
            get => _Priority;
            set => SetField(ref _Priority, value, nameof(Priority));
        }

        [SQLite.Ignore]
        public EmailSearch.EmailSearchAtom Atom
        {
            get => _Atom;
            set => SetField(ref _Atom, value, nameof(Atom));
        }

        [SQLite.Ignore]
        public EmailFilterAction Action
        {
            get => _Action;
            set => SetField(ref _Action, value, nameof(Action));
        }


        ////////////////////////////////////////////////////////////////
        #endregion Properties
        ////////////////////////////////////////////////////////////////



        ////////////////////////////////////////////////////////////////
        #region Construction & Initialization
        ////////////////////////////////////////////////////////////////

        public EmailFilter()
        {
        }

        private void Action_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(Action));
        }

        private void Atom_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(Atom));
        }

        public static EmailFilter Default()
        {
            return new EmailFilter();
        }


        ////////////////////////////////////////////////////////////////
        #endregion Construction & Initialization
        ////////////////////////////////////////////////////////////////



        ////////////////////////////////////////////////////////////////
        #region Interface
        ////////////////////////////////////////////////////////////////

        /// <summary>
        /// Does this filter apply to the given message?
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task<bool> AppliesToMessage(EmailMessage message)
        {
            var result = await EmailSearchEngine.SearchOneMessage(Atom, message);
            return result;
        }

        /// <summary>
        /// In sequence, run all the actions of this filter unless one returns
        /// false to break the chain.
        /// </summary>
        /// <param name="message"></param>
        public void PerformActions(EmailMessage message)
        {
            Action.Act(message);
        }

        ////////////////////////////////////////////////////////////////
        #endregion Interface
        ////////////////////////////////////////////////////////////////
    }




    internal static class FilterManager
    {
        public static DatastoreBase<EmailFilter> Datastore;

        static FilterManager()
        {
            Datastore = new("Data", "Filters", "Filters");
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

        public static EmailFilter New()
        {
            EmailFilter filter = new();
            Datastore.Add(filter);
            return filter;
        }

        public static EmailFilter New(string name)
        {
            EmailFilter filter = new() { Name  = name };
            Datastore.Add(filter);
            return filter;
        }

        public static void Add(EmailFilter filter)
        {
            Datastore.Add(filter);
        }

        public static void Remove(EmailFilter filter)
        {
            Datastore.Delete(filter);
        }

        public static bool Contains(string name)
        {
            if (Datastore.Data.Any(x => x.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase))) return true;
            return false;
        }

        public static void Sort()
        {
            Datastore.Data.Sort(i => i.Priority);
        }
    }

}
