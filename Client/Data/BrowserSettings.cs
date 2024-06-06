using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Eudora.Net.Core;

namespace Eudora.Net.Data
{
    public class BrowserSettings : INotifyPropertyChanged
    {
        public static BrowserSettings Instance { get; private set; }


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

        private static string RootPath = string.Empty;

        public static readonly string SettingsFilename = @"BrowserSettings.stg";
        public static object SettingsLocker = new();

        private static readonly string SearchEnginesFilename = @"SearchEngines.dat";
        private static object SearchEnginesSerializationLocker = new();
        public static ObservableCollection<BrowserSearchEngine> SearchEngines { get; set; } = [];

        private BrowserSearchEngine? _ActiveSearchEngine;
        public BrowserSearchEngine? ActiveSearchEngine
        {
            get => _ActiveSearchEngine;
            set => SetField(ref _ActiveSearchEngine, value, nameof(ActiveSearchEngine));
        }

        private static readonly string BookmarksFilename = @"Bookmarks.dat";
        private static object BookmarksSerializationLocker = new();
        public static ObservableCollection<BrowserBookmark> Bookmarks { get; set; } = [];

        public static JsonSerializerOptions DefaultJsonOptions()
        {
            JsonSerializerOptions JsonOptions = new()
            {
                WriteIndented = true
            };
            return JsonOptions;
        }

        /////////////////////////////
        #endregion Properties
        ///////////////////////////////////////////////////////////

        static BrowserSettings()
        {
            RootPath = Path.Combine(Eudora.Net.Properties.Settings.Default.DataStoreRoot, @"Browser");
            IoUtil.EnsureFolder(RootPath);

            LoadSearchEngines();
            LoadBookmarks();
            Instance = new BrowserSettings();
        }

        public BrowserSettings()
        {
            
        }

        public void Startup()
        {
            Load();
            // Default init
            if (SearchEngines.Count == 0)
            {
                SearchEngines.Add(new BrowserSearchEngine(
                    "Google", 
                    "https://www.google.com/search?q=", 
                    "pack://application:,,,/GUI/res/images/new/google.png"));
                
                SearchEngines.Add(new BrowserSearchEngine(
                    "Bing", 
                    "https://www.bing.com/search?q=", 
                    "pack://application:,,,/GUI/res/images/new/bing.png"));
                
                SearchEngines.Add(new BrowserSearchEngine(
                    "DuckDuckGo", 
                    "https://duckduckgo.com/?q=", 
                    "pack://application:,,,/GUI/res/images/new/duckduckgo.png"));

                SaveSearchEngines();
            }

            _ActiveSearchEngine ??= SearchEngines.First();
        }

        public void Shutdown()
        {
            Save();
            SaveBookmarks();
        }



        ///////////////////////////////////////////////////////////
        #region Serialization
        /////////////////////////////

        public void Load()
        {
            try
            {
                lock(SettingsLocker)
                {
                    string fullPath = Path.Combine(RootPath, SettingsFilename);
                    if (!File.Exists(fullPath)) return;
                    string raw = File.ReadAllText(fullPath);
                    var json = JsonSerializer.Deserialize<BrowserSettings>(raw);
                    if (json is null) return;
                    ActiveSearchEngine = json.ActiveSearchEngine;
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }


        public void Save()
        {
            try
            {
                lock(SettingsLocker)
                {
                    string fullPath = Path.Combine(RootPath, SettingsFilename);
                    var json = JsonSerializer.Serialize<BrowserSettings>(this, IoUtil.JsonWriterOptions);
                    if (json is null) return;
                    File.WriteAllText(fullPath, json);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        public static void LoadSearchEngines()
        {
            try
            {
                lock (SearchEnginesSerializationLocker)
                {
                    string fullPath = Path.Combine(RootPath, SearchEnginesFilename);
                    if (!File.Exists(fullPath)) return;
                    string raw = File.ReadAllText(fullPath);
                    var json = JsonSerializer.Deserialize<ObservableCollection<BrowserSearchEngine>>(raw);
                    if (json is not null)
                    {
                        SearchEngines = json;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        public static void SaveSearchEngines()
        {
            try
            {
                lock (SearchEnginesSerializationLocker)
                {
                    string fullPath = Path.Combine(RootPath, SearchEnginesFilename);
                    string json = JsonSerializer.Serialize<ObservableCollection<BrowserSearchEngine>>(SearchEngines, DefaultJsonOptions());
                    File.WriteAllText(fullPath, json);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        public static void LoadBookmarks()
        {
            try
            {
                lock (BookmarksSerializationLocker)
                {
                    string fullPath = Path.Combine(RootPath, BookmarksFilename);
                    if (!File.Exists(fullPath)) return;
                    string raw = File.ReadAllText(fullPath);
                    var json = JsonSerializer.Deserialize<ObservableCollection<BrowserBookmark>>(raw);
                    if (json is not null)
                    {
                        Bookmarks = json;
                        foreach (var bookmark in Bookmarks)
                        {
                            bookmark.PropertyChanged += (sender, args) => SaveBookmarks();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        public static void SaveBookmarks()
        {
            try
            {
                lock (BookmarksSerializationLocker)
                {
                    string fullPath = Path.Combine(RootPath, BookmarksFilename);
                    var json = JsonSerializer.Serialize<ObservableCollection<BrowserBookmark>>(Bookmarks, IoUtil.JsonWriterOptions);
                    if (json is not null)
                    {
                        File.WriteAllText(fullPath, json);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }
    }

    /////////////////////////////
    #endregion Serialization
    ///////////////////////////////////////////////////////////
}

