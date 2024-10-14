using System.IO;
using System.Reflection;
using System.Text.Json;

namespace Eudora.Net.Core
{
    public static class IoUtil
    {
        public static JsonSerializerOptions JsonWriterOptions
        {
            get { return new() { WriteIndented = true }; }
        }

        public static string LoadResourceString(string resourceName)
        {
            try
            {
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
                {
                    if (stream is not null)
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            return reader.ReadToEnd();                            
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                FaultReporter.Error(ex);
            }
            Logger.Warning($"Failed to load resource stream {resourceName}");
            return string.Empty;
        }

        public static Stream? LoadResourceStream(string resourceName)
        {
            Stream? stream = null;

            try
            {
                stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
                {
                    return stream;
                }
            }
            catch (Exception ex)
            {
                FaultReporter.Error(ex);
            }
            Logger.Warning($"Failed to load resource stream {resourceName}");
            return stream;
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
                FaultReporter.Error(ex);
            }
        }

        public static void EnsureFolder(string fullPath)
        {
            try
            {
                string path = Path.GetFullPath(fullPath);
                DirectoryInfo di = Directory.CreateDirectory(path);
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
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
                Logger.Exception(ex);
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
                Logger.Exception(ex);
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
                Logger.Exception(ex);
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
