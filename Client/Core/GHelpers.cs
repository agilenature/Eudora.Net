using System.Drawing.Imaging;
using System.IO;
using System.IO.Packaging;
using System.Windows.Media;
using System.Drawing;
using System.Security.Cryptography;
using System.Reflection;

namespace Eudora.Net.Core
{
    public static class GHelpers
    {
        public static string NullSelection = "(None)";
        public static string EmailContentToken = "@CONTENT_TOKEN";
        public static SolidColorBrush ErrorBrush = new(Colors.Red);


        public static string LoadResourceFileAsString(string resourceName)
        {
            string resource = string.Empty;

            try
            {
                using (var assemblyResource = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
                {
                    if (assemblyResource is not null)
                    {
                        using (var stream = new StreamReader(assemblyResource))
                        {
                            resource = stream.ReadToEnd();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }

            return resource;
        }

        public static string ConvertImageToPng(string fullPath)
        {
            string? extension = Path.GetExtension(fullPath)?.ToLower();
            if (extension == null)
            {
                Logger.NewEvent(LogEvent.EventCategory.Warning, "No file extension detected");
                return fullPath;
            }
            if (extension == ".png") return fullPath;

            try
            {
                Bitmap bitmap = (Bitmap)Bitmap.FromStream(File.Open(fullPath, FileMode.Open));
                {
                    string newPath = $@"{Path.GetTempFileName()}.png";
                    using (FileStream stream = new(newPath, FileMode.Create, FileAccess.ReadWrite))
                    {
                        bitmap.Save(stream, ImageFormat.Png);
                    }
                    return newPath;
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                return fullPath;
            }
        }

        // For the sake of simplicity, all images are converted to PNG
        // prior to conversion to base64
        public static string? Base64StringFromImageFile(string fullPath)
        {
            string? extension = Path.GetExtension(fullPath)?.ToLower();
            if(extension == null) 
            {
                Logger.NewEvent(LogEvent.EventCategory.Warning, "No file extension detected");
                return null;
            }

            byte[] imageBytes = [];
            string base64String = string.Empty;

            if (extension != ".png")
            {
                try
                {
                    Bitmap bitmap = (Bitmap)Bitmap.FromStream(File.Open(fullPath, FileMode.Open));
                    {
                        using (MemoryStream memStream = new())
                        {
                            bitmap.Save(memStream, ImageFormat.Png);
                            imageBytes = memStream.ToArray();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogException(ex);
                    return null;
                }
            }
            else
            {
                try
                {
                    imageBytes = File.ReadAllBytes(fullPath);
                }
                catch (Exception ex)
                {
                    Logger.LogException(ex);
                    return null;
                }
            }

            try
            {
                base64String = Convert.ToBase64String(imageBytes);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                return null;
            }
            
            return base64String;
        }

        public static string MakeImageFilterString()
        {
            var encoders = ImageCodecInfo.GetImageEncoders();
            string sep = "|";
            string filter = string.Empty;
            foreach (var encoder in encoders)
            {
                if (encoder.CodecName is null)
                {
                    continue;
                }
                string s1 = encoder.CodecName.Substring(8).Replace("Codec", "Files").Trim();
                filter = String.Format("{0}{1}{2} ({3})|{3}", filter, sep, s1, encoder.FilenameExtension);
            }
            return filter[1..];
        }

        public static void test()
        {
            var encoders = ImageCodecInfo.GetImageEncoders();
            foreach(var encoder in encoders)
            {
                
            }
        }

    }
}
