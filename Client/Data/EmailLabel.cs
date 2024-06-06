using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Eudora.Net.Core;

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

        public static readonly string extension = ".lbl";

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
        private static readonly string DataRoot = string.Empty;
        private static object Locker;

        public static ObservableCollection<EmailLabel> Collection { get; private set; } = [];

        static LabelManager()
        {
            Locker = new();
            DataRoot = Path.Combine(Eudora.Net.Properties.Settings.Default.DataStoreRoot, "Labels");
            IoUtil.EnsureFolder(DataRoot);
        }

        public static void Startup()
        {
            // Default labels
            Collection.Add(new EmailLabel("Label 1", "pack://application:,,,/GUI/res/images/tb32/tb32_78.png"));
            Collection.Add(new EmailLabel("Label 2", "pack://application:,,,/GUI/res/images/tb32/tb32_79.png"));
            Collection.Add(new EmailLabel("Label 3", "pack://application:,,,/GUI/res/images/tb32/tb32_80.png"));
            Collection.Add(new EmailLabel("Label 4", "pack://application:,,,/GUI/res/images/tb32/tb32_81.png"));
            Collection.Add(new EmailLabel("Label 5", "pack://application:,,,/GUI/res/images/tb32/tb32_82.png"));
            Collection.Add(new EmailLabel("Label 6", "pack://application:,,,/GUI/res/images/tb32/tb32_83.png"));
            Collection.Add(new EmailLabel("Label 7", "pack://application:,,,/GUI/res/images/tb32/tb32_84.png"));

            // User labels
            Load();
        }

        public static void Shutdown()
        {

        }

        private static void Label_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is EmailLabel label)
            {
                Save(label);
            }
        }

        private static string MakeFilename(string name)
        {
            return string.Format("{0}{1}", name, EmailLabel.extension);
        }

        private static string MakeFullPath(string name)
        {
            return Path.Combine(DataRoot, MakeFilename(name));
        }

        public static EmailLabel New(string name)
        {
            EmailLabel label = new() { Name = name };
            label.PropertyChanged += Label_PropertyChanged;
            Collection.Add(label);
            Save(label);
            return label;
        }

        public static void Add(EmailLabel label)
        {
            Collection.Add(label);
            Save(label);
        }

        public static void Remove(EmailLabel label)
        {
            Collection.Remove(label);

            try
            {
                string fullPath = MakeFullPath(label.Name);
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        public static bool Contains(string name)
        {
            if (Collection.Any(x => x.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase))) return true;
            return false;
        }

        private static void Load()
        {
            try
            {
                lock (Locker)
                {
                    DirectoryInfo di = new(DataRoot);
                    string searchQuery = string.Format("*{0}", EmailLabel.extension);
                    var files = di.GetFiles(searchQuery);
                    foreach (var file in files)
                    {
                        if (file.DirectoryName == null)
                        {
                            continue;
                        }

                        string raw = File.ReadAllText(file.FullName);
                        var label = JsonSerializer.Deserialize<EmailLabel>(raw);
                        if (label is not null)
                        {
                            Collection.Add(label);
                            label.PropertyChanged += Label_PropertyChanged;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        public static void Save(EmailLabel label)
        {
            try
            {
                lock (Locker)
                {
                    string fullPath = MakeFullPath(label.Name);
                    string json = JsonSerializer.Serialize<EmailLabel>(label, IoUtil.JsonWriterOptions);
                    File.WriteAllText(fullPath, json);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }
    }
}
