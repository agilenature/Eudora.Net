using System.Collections.ObjectModel;

namespace Eudora.Net.Core
{
    /// <summary>
    /// Extension class of ObservableCollection<T>
    /// For reasons which escape me, the built-in type has no sorting
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SortableObservableCollection<T> : ObservableCollection<T>
    {
        public SortableObservableCollection(IEnumerable<T> collection) :
            base(collection)
        { }

        public SortableObservableCollection() : base() { }

        public void Sort<TKey>(Func<T, TKey> keySelector)
        {
            Sort(Items.OrderBy(keySelector));
        }

        public void Sort<TKey>(Func<T, TKey> keySelector, IComparer<TKey> comparer)
        {
            Sort(Items.OrderBy(keySelector, comparer));
        }

        public void SortDescending<TKey>(Func<T, TKey> keySelector)
        {
            Sort(Items.OrderByDescending(keySelector));
        }

        public void SortDescending<TKey>(Func<T, TKey> keySelector,
            IComparer<TKey> comparer)
        {
            Sort(Items.OrderByDescending(keySelector, comparer));
        }

        public void Sort(IEnumerable<T> sortedItems)
        {
            List<T> sortedItemsList = sortedItems.ToList();
            for (int i = 0; i < sortedItemsList.Count; i++)
            {
                Items[i] = sortedItemsList[i];
            }
        }
    }
}
