using SQLite;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Eudora.Net.Data
{
    /// <summary>
    /// 
    /// </summary>
    internal class DatastoreBase<T> where T : class, new()
    {
        protected string DbPath = string.Empty;
        protected SQLiteConnection DB = default;
        public ObservableCollection<T> Data { get; private set; } = [];


        public DatastoreBase(string dbPath)
        {
            DbPath = dbPath;
            DB = new SQLiteConnection(DbPath);
            DB.CreateTable<T>();

            Data.CollectionChanged += Data_CollectionChanged;
        }

        private void Data_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add && e.NewItems is not null)
            {
                foreach (T t in e.NewItems)
                {
                    DB.Insert(t);
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove && e.OldItems is not null)
            {
                foreach (T t in e.OldItems)
                {
                    DB.Delete(t);
                }
            }
        }

        private void T_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender is T t)
            {
                DB.Update(t);
            }
            else if (sender is not null)
            {
                Debug.WriteLine($"DataClass1_PropertyChanged: sender is type {sender.GetType()}");
            }
            else
            {
                Debug.WriteLine($"DataClass1_PropertyChanged: sender is null");
            }
        }

        public void Load()
        {
            Data.Clear();
            foreach (T t in DB.Table<T>())
            {
                Data.Add(t);
            }
        }

        /// <summary>
        /// Add is done via the ObservableCollection
        /// </summary>
        /// <param name="item"></param>
        public void Add(T item)
        {
            Data.Add(item);
        }

        /// <summary>
        /// Remove is done via the ObservableCollection
        /// </summary>
        /// <param name="item"></param>
        public void Remove(T item)
        {
            Data.Remove(item);
        }

        /// <summary>
        /// Update works directly on the DB
        /// </summary>
        /// <param name="item"></param>
        public void Update(T item)
        {
            DB.Update(item);
        }
    }
}
