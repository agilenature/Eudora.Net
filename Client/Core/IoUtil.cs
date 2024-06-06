using System.IO;
using System.Runtime.InteropServices.Marshalling;
using System.Text.Json;

namespace Eudora.Net.Core
{
    public static class IoUtil
    {
        public static JsonSerializerOptions JsonWriterOptions
        {
            get { return new() { WriteIndented = true }; }
        }

        public static void EnsureFilePath(string fullPath)
        {
            try
            {
                string? path = Path.GetDirectoryName(fullPath);
                if (path is not null)
                {
                    Directory.CreateDirectory(path);
                }
            }
            catch(Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        public static void EnsureFolder(string fullPath)
        {
            try
            {
                string path = Path.GetFullPath(fullPath);
                DirectoryInfo di = Directory.CreateDirectory(path);
                int breaker = 1;
                //if (Directory.Exists(path) == false)
                //{
                //    Directory.CreateDirectory(path);
                //}
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }


        public static void Write<T>(string fullPath, T data, object? locker)
        {
            try
            {
                if (locker is not null) lock (locker)
                    {
                        EnsureFilePath(fullPath);
                        using (var stream = new FileStream(fullPath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                        {
                            JsonSerializer.Serialize<T>(stream, data, JsonWriterOptions);
                        }
                    }
            }
            catch(Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        public static void Read<T>(string fullPath, ref T data, object? locker)
        {
            try
            {
                if (locker is not null) lock (locker)
                    {
                        EnsureFilePath(fullPath);
                        using (var stream = new FileStream(fullPath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                        {
                            var json = JsonSerializer.Deserialize<T>(fullPath);
                            if (json is not null)
                            {
                                data = json;
                            }
                        }
                    }
            }
            catch(Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        public static void EnsureEmptyJson(string fullPath)
        {
            try
            {
                EnsureFilePath(fullPath);
                if(!File.Exists(fullPath))
                {
                    using (var stream = File.Create(fullPath))
                    {
                        List<object> data = [];
                        JsonSerializer.Serialize(stream, data, JsonWriterOptions);
                    }
                }
            }
            catch(Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        public static bool IsValidFilename(string filename)
        {
            if (String.IsNullOrEmpty(filename)) return false;
            if (String.IsNullOrWhiteSpace(filename)) return false;
            if (filename.IndexOfAny(Path.GetInvalidFileNameChars()) != -1) return false;

            return true;
        }

    }
}
