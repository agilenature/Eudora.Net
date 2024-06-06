using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using Eudora.Net.Core;

namespace Eudora.Net.Data
{
    public class Signature : INotifyPropertyChanged
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

        public static readonly string extension = ".sig";

        private string _Name = string.Empty;
        public string Name
        {
            get => _Name;
            set => SetField(ref _Name, value, nameof(Name));
        }

        private string _Content = string.Empty;
        public string Content
        {
            get => _Content;
            set => SetField(ref _Content, value, nameof(Content));
        }

        /////////////////////////////
        #endregion Properties
        ///////////////////////////////////////////////////////////
        

        public Signature() 
        { }

        public Signature(string name, string content)
        {
            Name = name;
            Content = content;
        }
    }

    public static class SignatureManager
    {
        private static readonly string DataRoot = string.Empty;
        private static object Locker;

        public static ObservableCollection<Signature> Collection { get; private set; } = [];

        static SignatureManager()
        {
            Locker = new();
            DataRoot = Path.Combine(Eudora.Net.Properties.Settings.Default.DataStoreRoot, "Signatures");
            IoUtil.EnsureFolder(DataRoot);
        }

        public static void Startup()
        {
            Load();
        }

        public static void Shutdown()
        {
            
        }

        private static void Signature_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is Signature signature)
            {
                Save(signature);
            }
        }

        private static string MakeFilename(string name)
        {
            return $@"{name}{Signature.extension}";
        }

        private static string MakeFullPath(string name)
        {
            return Path.Combine(DataRoot, MakeFilename(name));
        }

        public static Signature New(string name)
        {
            Signature signature = new() { Name = name };
            signature.PropertyChanged += Signature_PropertyChanged;
            Collection.Add(signature);
            Save(signature);
            return signature;
        }

        public static void Remove(Signature signature)
        {
            Collection.Remove(signature);

            try
            {
                string fullPath = MakeFullPath(signature.Name);
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
            }
            catch(Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        public static bool Contains(string name)
        {
            if(Collection.Any(x => x.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase))) return true;
            return false;
        }

        private static void Load()
        {
            try
            {
                lock (Locker)
                {
                    DirectoryInfo di = new(DataRoot);
                    string searchQuery = string.Format("*{0}", Signature.extension);
                    var files = di.GetFiles(searchQuery);
                    foreach (var file in files)
                    {
                        if (file.DirectoryName == null)
                        {
                            continue;
                        }

                        Signature signature = new()
                        {
                            Name = Path.GetFileNameWithoutExtension(file.Name),
                            Content = File.ReadAllText(file.FullName)
                        };
                        Collection.Add(signature);
                        signature.PropertyChanged += Signature_PropertyChanged;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        public static void Save(Signature signature)
        {
            try
            {
                lock (Locker)
                {
                    string fullPath = MakeFullPath(signature.Name);
                    File.WriteAllText(fullPath, signature.Content);
                }
            }
            catch(Exception ex)
            {
                Logger.LogException(ex);
            }
        }
    }


    
}
