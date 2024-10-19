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
    /// A utility base for a single-table DB
    /// </summary>
    public class DatastoreBase<T> where T : class, INotifyPropertyChanged, new()
    {
        protected string DbFolder { get; set; } = string.Empty;
        protected string DbName { get; set; } = string.Empty;
        protected string DbPath { get; set; } = string.Empty;
        protected SQLiteConnection DB { get; private set; } = default;
        public SortableObservableCollection<T> Data { get; private set; } = [];


        public DatastoreBase(string folder, string? subfolder, string name)
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
                FaultReporter.Error(ex);
            }
        }

        public DatastoreBase(string folder, string[]? subfolders, string name)
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
                FaultReporter.Error(ex);
            }
        }

        private void T_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            try
            {
                if (sender is T t)
                {
                    int rowsUpdated = DB.Update(t);
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
            catch(Exception ex)
            {
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

                var dboptions = new SQLiteConnectionString(DbPath, true, key:EudoraHelper.IDatastore.LocalKey);
                DB = new SQLiteConnection(dboptions);
                DB.CreateTable<T>();
            }
            catch (Exception ex)
            {
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
                Logger.Exception(ex);
            }
        }

        public void Load()
        {
            try
            {
                Data.Clear();
                foreach (T t in DB.Table<T>())
                {
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
                DB.InsertOrReplace(item);
                Data.Add(item);   
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
                if(Contains(item))
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
                item.PropertyChanged -= T_PropertyChanged;
                DB.Delete(item);
                Data.Remove(item);
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
                DB.Update(item);
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

        public bool Contains(Func<T, bool> lambda)
        {
            try
            {
                return (DB.Table<T>().Where(lambda).FirstOrDefault() is not null);
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
                return DB.Table<T>().Where(lambda).FirstOrDefault();
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
                return new ObservableCollection<T>(DB.Table<T>().Where(lambda));
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
                return null;
            }
        }
    }
}
