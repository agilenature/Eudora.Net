using System.Diagnostics;
using System.IO;


namespace Eudora.Net.Core
{
    internal class GVirusScanner
    {
        private static string wdPath = string.Empty;
        private static bool wdExists = false;

        static GVirusScanner()
        {
            wdPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Windows Defender", "MpCmdRun.exe");
            wdExists = File.Exists(wdPath);
        }

        public static async Task<bool> ScanFile(string fileFullPath)
        {
            try
            {
                using (var process = Process.Start(wdPath, $"-Scan -ScanType 3 -File \"{fileFullPath}\" -DisableRemediation"))
                {
                    try
                    {
                        await process.WaitForExitAsync().WaitAsync(TimeSpan.FromMilliseconds(20000));
                    }
                    catch (TimeoutException ex) //timeout
                    {
                        throw new TimeoutException("Timeout waiting for MpCmdRun.exe to return", ex);
                    }
                    finally
                    {
                        process.Kill(); //always kill the process, it's fine if it's already exited, but if we were timed out or cancelled via token - let's kill it
                    }
                }
            }
            catch(Exception ex)
            {
                FaultReporter.Error(ex);
            }
            return false;
        }
    }
}
