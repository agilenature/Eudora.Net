using Eudora.Net.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Eudora.Net.Data
{
    internal class DatastoreManager<T> where T : class, INotifyPropertyChanged, new()
    {
        protected static DatastoreBase<T> Datastore { get; private set; }

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

        public static void Add(T t)
        {
            Datastore.Add(t);
        }

        public static void Remove(T t)
        {
            Datastore.Remove(t);
        }
    }
}
