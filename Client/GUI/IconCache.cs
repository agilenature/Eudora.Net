using Eudora.Net.Core;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;


namespace Eudora.Net.GUI
{
    /// <summary>
    /// Maintain a cache of ImageSource corresponding to icons of file types.
    /// TODO: The library entries should be checked from time to time to see
    /// if a new program-fileType association has made some of the data old.
    /// </summary>
    public static class IconCache
    {
        private static readonly string LibraryName = "IconLibrary.lib";
        private static object SerializationLocker;
        private static Dictionary<string, BitmapSource> Library = [];

        static IconCache()
        {
            SerializationLocker = new object();
           // Load();
        }

        public static void Load()
        {
            try
            {
                string FullPath = Path.Combine(Eudora.Net.Properties.Settings.Default.DataStoreRoot, LibraryName);
                lock (SerializationLocker)
                {
                    using (var stream = new FileStream(FullPath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                    {
                        var json = JsonSerializer.Deserialize<Dictionary<string, BitmapSource>>(stream);
                        if (json is not null)
                        {
                            Library = json;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
            }
        }

        public static void Save()
        {
            try
            {
                string FullPath = Path.Combine(Eudora.Net.Properties.Settings.Default.DataStoreRoot, LibraryName);
                lock (SerializationLocker)
                {
                    var json = JsonSerializer.Serialize(Library, IoUtil.JsonWriterOptions);
                    if (json is not null)
                    {
                        File.WriteAllText(FullPath, json);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
            }
        }

        public static BitmapSource? GetBitmapSourceFromFile(string filePath)
        {
            string extension = Path.GetExtension(filePath);
            BitmapSource? source = GetImage(extension);
            if(source is not null)
            {
                return source;
            }

            try
            {
                using (var icon = Icon.ExtractAssociatedIcon(filePath))
                {
                    if (icon is not null)
                    {
                        source = Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                        Add(extension, source);
                    }
                }
            }
            catch(Exception ex)
            {
                Logger.Exception(ex);
            }
            return source;
        }

        public static BitmapSource? GetBitmapSourceFromFolder(string folder)
        {
            try
            {
                BitmapSource? source = GetImage(folder);
                if (source is not null)
                {
                    return source;
                }

                using (var icon = GWin32Native.GetFolderIcon(folder))
                {
                    if (icon is not null)
                    {
                        source = Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                        Add(folder, source);
                        return source;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
            }

            return null;
        }

        public static void Add(string extension, BitmapSource source)
        {
            try
            {
                //Library.TryAdd(extension, source);
            }
            catch(Exception ex)
            {
                Logger.Exception(ex);
            }
        }

        public static BitmapSource? GetImage(string extension)
        {
            Library.TryGetValue(extension, out BitmapSource? source);
            return source;
        }

        
    }
}
