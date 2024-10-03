using Eudora.Net.Core;
using Eudora.Net.ExtensionMethods;
using SQLite;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;

namespace Eudora.Net.Data
{
    /// <summary>
    /// A version of Datastore for when the T class cannot be serialized by SQLite.
    /// 
    /// The DatastoreBase design already synchronizes the WinUI-friendly collection class
    /// representation of the data with the underlying DB table. That being true, 
    /// all we need to make this template work is to insert a converter step 
    /// into each relevant act of the Datastore. At each juncture, this DB template
    /// automatically converts between T (the class the app works with) and TDatabaseClass,
    /// a version of T which SQLite can read and write.
    /// 
    /// This datastore template requires three paramaters:
    /// </summary>
    /// <typeparam name="TDatabaseClass"></typeparam>
    /// <typeparam name="T"></typeparam>
    internal class DatastoreConversion<T, TDatabaseClass, TConverter>
        where T : class, INotifyPropertyChanged, new()
        where TDatabaseClass : class, new()
        where TConverter : class, IDbConverter<T, TDatabaseClass>, new()
    {
        protected string DbFolder { get; set; } = string.Empty;
        protected string DbName { get; set; } = string.Empty;
        protected string DbPath { get; set; } = string.Empty;
        protected SQLiteConnection DB { get; private set; } = default;
        public SortableObservableCollection<T> Data { get; private set; } = [];
        private TConverter Converter = new();

        public DatastoreConversion(string folder, string? subfolder, string name)
        {
            try
            {
                DbName = $"{name}.db";
                DbFolder = Path.Combine(Eudora.Net.Properties.Settings.Default.DataStoreRoot, $"{folder}");
                if (subfolder is not null)
                {
                    DbFolder = Path.Combine(DbFolder, subfolder);
                }
                DbPath = Path.Combine(DbFolder, $"{name}.db");
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
            }
        }

        public DatastoreConversion(string folder, string[]? subfolders, string name)
        {
            try
            {
                DbName = $"{name}.db";
                DbFolder = Path.Combine(Eudora.Net.Properties.Settings.Default.DataStoreRoot, $"{folder}");
                if (subfolders is not null)
                {
                    foreach (string subfolder in subfolders)
                    {
                        DbFolder = Path.Combine(DbFolder, subfolder);
                    }
                }
                DbPath = Path.Combine(DbFolder, $"{name}.db");
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
            }
        }

        private void T_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            try
            {
                if (sender is T t)
                {
                    var converted = Converter.Convert(t);
                    int rowsUpdated = DB.Update(converted);
                    Debug.WriteLine($"{t.GetType()} T_PropertyChanged: {rowsUpdated} rows updated");
                }
                else if (sender is not null)
                {
                    Debug.WriteLine($"T_PropertyChanged: sender is type {sender.GetType()}");
                }
                else
                {
                    Debug.WriteLine($"T_PropertyChanged: sender is null");
                }
            }
            catch (Exception ex)
            {
                Logger.Debug("T_PropertyChanged");
                Logger.Exception(ex);
            }
        }

        public int Count()
        {
            try
            {
                return Data.Count;
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
                return 0;
            }
        }

        public T? First()
        {
            try
            {
                return Data.First();
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
                return default;
            }
        }

        public void Open()
        {
            try
            {
                IoUtil.EnsureFolder(DbFolder);

                var dboptions = new SQLiteConnectionString(DbPath, true, key: EudoraHelper.IDatastore.LocalKey);
                DB = new SQLiteConnection(dboptions);
                DB.CreateTable<TDatabaseClass>();
            }
            catch (Exception ex)
            {
                Logger.Debug("Open()");
                Logger.Exception(ex);
            }
        }

        public void Close()
        {
            try
            {
                // This can occur if the user forces the app to exit during the initial setup screen
                DB?.Close();
            }
            catch (Exception ex)
            {
                Logger.Debug("Close()");
                Logger.Exception(ex);
            }
        }

        public void Load()
        {
            try
            {
                Data.Clear();
                foreach (TDatabaseClass tdb in DB.Table<TDatabaseClass>())
                {
                    T? t = Converter.Convert(tdb);
                    if(t is null)
                    {
                        Logger.Error(
                            "Failed to convert EmailMessage_DB to EmailMessage",
                            "DatastoreConversion::Load");
                        continue;
                    }
                    t.PropertyChanged += T_PropertyChanged;
                    Data.Add(t);
                }
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
            }
        }

        public void Add(T item)
        {
            try
            {
                item.PropertyChanged += T_PropertyChanged;
                TDatabaseClass? itemDb = Converter.Convert(item);
                if (itemDb is not null)
                {
                    DB.InsertOrReplace(itemDb);
                    Data.Add(item);
                }
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
            }
        }

        public void AddUnique(T item)
        {
            try
            {
                if (Contains(item))
                {
                    return;
                }
                item.PropertyChanged += T_PropertyChanged;
                DB.Insert(item);
                Data.AddUnique(item);
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
            }
        }

        public void Delete(T item)
        {
            try
            {
                TDatabaseClass? itemDb = Converter.Convert(item);
                if (itemDb is not null)
                {
                    item.PropertyChanged -= T_PropertyChanged;
                    DB.Delete(itemDb);
                    Data.Remove(item);
                }
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
            }
        }

        public void Update(T item)
        {
            try
            {
                TDatabaseClass? itemDb = Converter.Convert(item);
                if (itemDb is not null)
                {
                    DB.Update(itemDb);
                }
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
            }
        }

        public bool Contains(T t)
        {
            try
            {
                return Data.Contains(t);
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
                return false;
            }
        }

        /// In this template, these operations run on the collection, not the underlying DB.
        /// This avoids unnecessary conversions, possibly in great number.

        public bool Contains(Func<T, bool> lambda)
        {
            try
            {
                return (Data.Where(lambda).FirstOrDefault() is not null);
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
                return false;
            }
        }

        public T? Find(Func<T, bool> lambda)
        {
            try
            {
                return Data.Where(lambda).FirstOrDefault();
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
                return default;
            }
        }

        public ObservableCollection<T>? FindAll(Func<T, bool> lambda)
        {
            try
            {
                return new ObservableCollection<T>(Data.Where(lambda));
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
                return null;
            }
        }

    }
}
