using System.Collections.ObjectModel;


namespace Eudora.Net.Core
{
    public class LockedObservableCollection<T> : ObservableCollection<T>
    {
        private object Locker = new();

        public new int Count()
        {
            lock(Locker)
            {
                return base.Count;
            }
        }

        public new void Add(T item)
        {
            lock (Locker) 
            {
                base.Add(item);
            }
        }

        public void AddUnique(T item)
        {
            lock(Locker)
            {
                if(base.Contains(item)) { return; } else { base.Add(item); }
            }
        }

        public void AddRange(ObservableCollection<T> collectionSource)
        {
            lock (Locker)
            {
                foreach (var t in collectionSource)
                {
                    base.Add(t);
                }
            }
        }

        public void AddRangeUnique(ObservableCollection<T> collectionSource)
        {
            lock(Locker)
            {
                foreach(var t in collectionSource)
                {
                    if (base.Contains(t)) { continue; } else { base.Add(t); }
                }
            }
        }

        public new void Remove(T item)
        {
            lock(Locker)
            {
                base.Remove(item);
            }
        }

        public new void Clear() 
        { 
            lock (Locker)
            {
                base.Clear();
            } 
        }

        public new bool Contains(T item)
        {
            lock (Locker)
            {
                return base.Contains(item);
            }
        }

        public List<T> Find(Func<T, bool> predicate)
        {
            lock (Locker)
            {
                return Items.Where(predicate).ToList();
            }
        }
    }
}
