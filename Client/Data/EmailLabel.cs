using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Eudora.Net.Core;
using SQLite;

namespace Eudora.Net.Data
{
    public class EmailLabel : INotifyPropertyChanged
    {
        ///////////////////////////////////////////////////////////
        #region INotifyPropertyChanged
        /////////////////////////////

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void SetField<TField>(ref TField field, TField value, string propertyName)
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

        private Guid _Id = Guid.NewGuid();
        [PrimaryKey]
        public Guid Id
        {
            get => _Id;
            set => SetField(ref _Id, value, nameof(Id));
        }

        private string _Name = string.Empty;
        public string Name 
        {
            get => _Name;
            set => SetField(ref _Name, value, nameof(Name));
        }

        private string _ImageSrc = string.Empty;
        public string ImageSrc 
        {
            get => _ImageSrc;
            set => SetField(ref _ImageSrc, value, nameof(ImageSrc));
        }

        public EmailLabel()
        {

        }

        public EmailLabel(string name, string imageSrc)
        {
            Name = name;
            ImageSrc = imageSrc;
        }
    }

    public static class LabelManager
    {
        public static DatastoreBase<EmailLabel> Datastore;

        static LabelManager()
        {
            Datastore = new("Data", "Labels", "Labels");
        }

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

        public static EmailLabel New(string name)
        {
            try
            {
                EmailLabel label = new() { Name = name };
                Datastore.Add(label);
                return label;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                return new EmailLabel();
            }
        }

        public static void Add(EmailLabel label)
        {
            try
            {
                Datastore.Add(label);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        public static void Remove(EmailLabel label)
        {
            try
            {
                Datastore.Data.Remove(label);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        public static bool Contains(string name)
        {
            if (Datastore.Data.Any(x => x.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase))) return true;
            return false;
        }
    }
}
