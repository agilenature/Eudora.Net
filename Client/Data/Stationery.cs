using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using Eudora.Net.Core;
using Eudora.Net.HtmlTemplates;

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

        public static readonly string extension = ".sta";
        private string _Name = string.Empty;
        private string _Content = string.Empty;
        private HtmlAttribute _Style = new("style", string.Empty);

        /////////////////////////////
        #endregion Fields
        ///////////////////////////////////////////////////////////


        ///////////////////////////////////////////////////////////
        #region Properties
        /////////////////////////////

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

    public static class StationeryManager
    {
        private static readonly string DataRoot = string.Empty;
        private static object Locker;

        public static ObservableCollection<Stationery> Collection { get; private set; } = [];

        static StationeryManager()
        {
            Locker = new();
            DataRoot = Path.Combine(Eudora.Net.Properties.Settings.Default.DataStoreRoot, "Stationery");
            IoUtil.EnsureFolder(DataRoot);
        }

        public static void Startup()
        {
            Load();
        }

        public static void Shutdown()
        {

        }

        private static void Stationery_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is Stationery stationery)
            {
                Save(stationery);
            }
        }

        private static string MakeFilename(string name)
        {
            return $@"{name}{Stationery.extension}";
        }

        private static string MakeFullPath(string name)
        {
            return Path.Combine(DataRoot, MakeFilename(name));
        }

        public static Stationery New(string name)
        {
            Stationery stationery = new() 
            { 
                Name = name,
                Content = HtmlDepot.html_Stationery
            };
            
            stationery.PropertyChanged += Stationery_PropertyChanged;
            Collection.Add(stationery);
            Save(stationery);
            return stationery;
        }

        public static void Add(Stationery stationery)
        {
            Collection.Add(stationery);
            Save(stationery);
        }

        public static void Remove(Stationery stationery)
        {
            Collection.Remove(stationery);

            try
            {
                string fullPath = MakeFullPath(stationery.Name);
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
                    string searchQuery = string.Format("*{0}", Stationery.extension);
                    var files = di.GetFiles(searchQuery);
                    foreach (var file in files)
                    {
                        if (file.DirectoryName == null)
                        {
                            continue;
                        }

                        Stationery stationery = new()
                        {
                            Name = Path.GetFileNameWithoutExtension(file.Name),
                            Content = File.ReadAllText(file.FullName)
                        };
                        Collection.Add(stationery);
                        stationery.PropertyChanged += Stationery_PropertyChanged;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        public static void Save(Stationery stationery)
        {
            try
            {
                lock (Locker)
                {
                    string fullPath = MakeFullPath(stationery.Name);
                    File.WriteAllText(fullPath, stationery.Content);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }
    }
}
