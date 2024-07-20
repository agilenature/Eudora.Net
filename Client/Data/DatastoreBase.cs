using Eudora.Net.Core;
using Eudora.Net.ExtensionMethods;
using SQLite;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;

namespace Eudora.Net.Data
{
    /// <summary>
    /// 
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
            DbName = $"{name}.db";
            DbFolder = Path.Combine(Eudora.Net.Properties.Settings.Default.DataStoreRoot, $"{folder}");
            if (subfolder is not null)
            {
                DbFolder = Path.Combine(DbFolder, $"{subfolder}");
            }
            DbPath = Path.Combine(DbFolder, $"{name}.db" );
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
                Logger.LogException(ex);
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
                Logger.LogException(ex);
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
                Logger.LogException(ex);
                return default;
            }
        }

        public void Open()
        {
            try
            {
                IoUtil.EnsureFolder(DbFolder);
                DB = new SQLiteConnection(DbPath);
                DB.CreateTable<T>();
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
                DB.Close();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
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
                Logger.LogException(ex);
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
                Logger.LogException(ex);
            }
        }

        public void AddUnique(T item)
        {
            try
            {
                item.PropertyChanged += T_PropertyChanged;
                DB.Insert(item);
                Data.AddUnique(item);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        public void Remove(T item)
        {
            try
            {
                item.PropertyChanged -= T_PropertyChanged;
                DB.Delete(item);
                Data.Remove(item);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
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
                Logger.LogException(ex);
            }
        }
    }
}
