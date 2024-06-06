using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Eudora.Net.GUI
{
    /// <summary>
    /// UX helper/timesaver
    /// Useful for populating single-selection listboxes with an underlying, bound data collection.
    /// Uses INotifyPropertyChanged rather than inheriting from DependencyObject
    /// to keep serialization simple.
    /// </summary>
    public class ExclusiveCollectionItem(object value, string displayName, string imgSource) : INotifyPropertyChanged
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
        #region Properties
        /////////////////////////////

        public string DisplayName
        {
            get => displayName;
            set => SetField(ref displayName, value, nameof(DisplayName));
        }
        public string DisplayImageSource
        {
            get => imgSource;
            set => SetField(ref imgSource, value, nameof(DisplayImageSource));
        }

        private bool _IsSelected = false;
        public bool IsSelected
        {
            get => _IsSelected;
            set => SetField(ref _IsSelected, value, nameof(IsSelected));
        }

        private object _Value = value;
        public object Value
        {
            get => _Value;
            set => SetField(ref _Value, value, nameof(Value));
        }

        /////////////////////////////
        #endregion Properties
        ///////////////////////////////////////////////////////////
    }

    /// <summary>
    /// An observable collection of ExclusiveCollectionItem
    /// </summary>
    public class ExclusiveCollection : ObservableCollection<ExclusiveCollectionItem>
    {
        public ExclusiveCollection()
        {
            
        }

        // Override default Add and Remove
        public new void Add(ExclusiveCollectionItem item)
        {
            if (Contains(item)) return;
            base.Add(item);
            item.PropertyChanged += Item_PropertyChanged;
        }

        public new void Remove(ExclusiveCollectionItem item)
        {
            if (!Contains(item)) return;
            base.Remove(item);
        }

        private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if( sender is ExclusiveCollectionItem thisItem &&
                e.PropertyName is not null && 
                e.PropertyName.Equals("isselected", StringComparison.CurrentCultureIgnoreCase) &&
                thisItem.IsSelected == true)
            {
                foreach(ExclusiveCollectionItem item in Items)
                {
                    if (item != thisItem)
                    {
                        item.IsSelected = false;
                    }
                }                
            }
        }        
    }
}
