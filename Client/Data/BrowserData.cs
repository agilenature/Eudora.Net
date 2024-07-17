using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Eudora.Net.Core;

namespace Eudora.Net.Data
{
    public class BrowserData : INotifyPropertyChanged
    {
        public static BrowserData Instance { get; private set; }


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

        public static DatastoreBase<BrowserSearchEngine> SearchEngines { get; private set; } 
        public static DatastoreBase<BrowserBookmark> Bookmarks { get; private set; }


        private BrowserSearchEngine? _ActiveSearchEngine;
        public BrowserSearchEngine? ActiveSearchEngine
        {
            get => _ActiveSearchEngine;
            set => SetField(ref _ActiveSearchEngine, value, nameof(ActiveSearchEngine));
        }


        /////////////////////////////
        #endregion Properties
        ///////////////////////////////////////////////////////////

        static BrowserData()
        {
            Instance = new BrowserData();
        }

        public BrowserData()
        {
            Bookmarks = new("Data", "Browser", "Bookmarks");
            SearchEngines = new("Data", "Browser", "SearchEngines");
        }

        public void Startup()
        {
            try
            {
                SearchEngines.Open();
                SearchEngines.Load();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }

            try
            {
                Bookmarks.Open();
                Bookmarks.Load();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }

            // Default init
            if (SearchEngines.Count() == 0)
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
            }

            ActiveSearchEngine = SearchEngines.Data.Where(x =>
                x.Name.Equals(Properties.Settings.Default.BrowserSearchEngine, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
            
            if(ActiveSearchEngine is null)
            {
                ActiveSearchEngine = SearchEngines.First();
            }
        }

        public void Shutdown()
        {
            try
            {
                SearchEngines.Close();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }

            try
            {
                Bookmarks.Close();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }
    }
}

