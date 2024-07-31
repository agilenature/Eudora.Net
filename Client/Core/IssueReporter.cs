using System.Diagnostics;
using System.IO;

namespace Eudora.Net.Core
{
    internal static class IssueReporter
    {
        public static async void ReportException(Exception ex)
        {
            try
            {
                DateTime now = DateTime.Now.ToUniversalTime();
                await Send("Eudora.Net Exception", $"{now.ToString()}\r\n{ex.Message}");
            }
            catch(Exception e)
            {
                Logger.LogException(e);
            }
        }

        public static async void ReportFeedback(string feedback)
        {
            try
            {
                DateTime now = DateTime.Now.ToUniversalTime();
                await Send("Eudora.Net Feedback", $"{now.ToString()}\r\n{feedback}");
            }
            catch(Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        private static async Task Send(string subject, string body)
        {
            try
            {
                using Process process = new Process();
                process.StartInfo.FileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"EudoraReporter\EudoraReporter.exe");
                process.StartInfo.ArgumentList.Add(subject);
                process.StartInfo.ArgumentList.Add(body);
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.ErrorDialog = true;

                if(process.Start())
                {
                    process.WaitForExit();
                    Logger.NewEvent(LogEvent.EventCategory.Information, "EudoraReporter.exe exited with code " + process.ExitCode);
                }
                else
                {
                    Logger.NewEvent(LogEvent.EventCategory.Warning, "Failed to start EudoraReporter.exe");
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }
    }
}
