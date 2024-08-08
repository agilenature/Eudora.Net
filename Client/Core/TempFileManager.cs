using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Eudora.Net.Core
{
    public static class TempFileManager
    {
        public static List<string> TempFilesThisSession = [];

        public static string CreateTempFileFromStringContent(string content)
        {
            string tempPath = Path.GetTempFileName();
            tempPath = $@"{tempPath}.html";
            TempFilesThisSession.Add(tempPath);

            try
            {
                File.WriteAllText(tempPath, content);
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
            }

            return tempPath;
        }

        public static string CreateTempFileFromStringContent2(string content)
        {
            string tempPath = Path.GetTempFileName();
            tempPath = $@"{tempPath}.tmp";
            TempFilesThisSession.Add(tempPath);

            try
            {
                File.WriteAllText(tempPath, content);
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
            }

            return tempPath;
        }

        public static void Shutdown()
        {
            try
            {
                foreach(var file in TempFilesThisSession)
                {
                    File.Delete(file);
                }
            }
            catch(Exception ex)
            {
                Logger.Exception(ex);
            }
        }
    }
}
