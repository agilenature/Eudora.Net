using System.Diagnostics;
using System.IO;


namespace Eudora.Net.Core
{
    /// <summary>
    /// A wrapper for the invocation of Microsoft Defender for scanning email attachments
    /// </summary>
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
                ProcessStartInfo psi = new()
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    ArgumentList = { $"-Scan -ScanType 3 -File \"{fileFullPath}\" -DisableRemediation" }
                };

                using (var process = Process.Start(wdPath, $"-Scan -ScanType 3 -File \"{fileFullPath}\" -DisableRemediation"))
                //using(var process = Process.Start(psi))
                {
                    if (process is null)
                    {
                        Logger.Warning("Failed to start Windows Defender");
                        return false;
                    }
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
                        process.Kill();
                    }
                }
            }
            catch(Exception ex)
            {
                FaultReporter.Error(ex);
            }
            return true;
        }
    }
}
