using Eudora.Net.Core;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;

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
        #region Properties
        ////////////////////////////////////////////////////////////////

        public static readonly string extension = ".flt";

        private string _Name = string.Empty;
        public string Name
        {
            get => _Name;
            set => SetField(ref _Name, value, nameof(Name));
        }

        private UInt32 _Priority = 0;
        public UInt32 Priority
        {
            get => _Priority;
            set => SetField(ref _Priority, value, nameof(Priority));
        }

        private EmailSearch.EmailSearchAtom _Atom = new();
        public EmailSearch.EmailSearchAtom Atom
        {
            get => _Atom;
            set => SetField(ref _Atom, value, nameof(Atom));
        }

        private EmailFilterAction _Action = new();
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




    public static class FilterManager
    {
        private static readonly string DataRoot = string.Empty;
        private static object Locker;

        public static SortableObservableCollection<EmailFilter> Collection { get; private set; } = [];

        static FilterManager()
        {
            Locker = new();
            DataRoot = Path.Combine(Eudora.Net.Properties.Settings.Default.DataStoreRoot, "Filters");
            IoUtil.EnsureFolder(DataRoot);
        }

        public static void Startup()
        {
            Load();
        }

        public static void Shutdown()
        {

        }

        private static void Filter_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is EmailFilter filter)
            {
                Save(filter);
            }
        }

        private static string MakeFilename(string name)
        {
            return string.Format("{0}{1}", name, EmailFilter.extension);
        }

        private static string MakeFullPath(string name)
        {
            return string.Format("{0}{1}{2}", DataRoot, name, EmailFilter.extension);
        }

        public static EmailFilter New(string name)
        {
            EmailFilter filter = new() { Name  = name };
            filter.PropertyChanged += Filter_PropertyChanged;
            Collection.Add(filter);
            Save(filter);
            return filter;
        }

        public static void Add(EmailFilter filter)
        {
            Collection.Add(filter);
            Save(filter);
        }

        public static void Remove(EmailFilter filter)
        {
            Collection.Remove(filter);

            try
            {
                string fullPath = MakeFullPath(filter.Name);
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

        public static bool Contains(string name)
        {
            if (Collection.Any(x => x.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase))) return true;
            return false;
        }

        private static void Load()
        {
            try
            {
                lock (Locker)
                {
                    DirectoryInfo di = new(DataRoot);
                    string searchQuery = string.Format("*{0}", EmailFilter.extension);
                    var files = di.GetFiles(searchQuery);
                    foreach (var file in files)
                    {
                        if (file.DirectoryName == null)
                        {
                            continue;
                        }

                        string raw = File.ReadAllText(file.FullName);
                        var filter = JsonSerializer.Deserialize<EmailFilter>(raw);
                        if (filter is not null)
                        {
                            Collection.Add(filter);
                            filter.PropertyChanged += Filter_PropertyChanged;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        public static void Save(EmailFilter filter)
        {
            try
            {
                lock (Locker)
                {
                    string fullPath = MakeFullPath(filter.Name);
                    string json = JsonSerializer.Serialize<EmailFilter>(filter, IoUtil.JsonWriterOptions);
                    File.WriteAllText(fullPath, json);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        public static EmailFilter New()
        {
            EmailFilter filter = new();
            Collection.Add(filter);
            Save(filter);
            return filter;
        }

        public static void Sort()
        {
            Collection.Sort(i => i.Priority);
        }
    }

}
