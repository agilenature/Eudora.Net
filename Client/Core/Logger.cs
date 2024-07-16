using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Windows.Media;

namespace Eudora.Net.Core
{
    public class LogEvent
    {
        public enum EventCategory
        {
            Information,
            Warning,
            Error,
            Notify,
            Debug
        }
        public EventCategory Category { get; set; }
        public DateTime Timestamp { get; set; }
        public string Message { get; set; } = string.Empty;
        public Color BrushColor { get; set; } = Colors.Orange;
    }

    public class Logger
    {
        public static ObservableCollection<LogEvent> Events = new ObservableCollection<LogEvent>();

        private static string FormatMessage(LogEvent.EventCategory eventCategory, DateTime timestamp, string message)
        {
            string output = string.Empty;
            string header = string.Empty;
            switch (eventCategory)
            {
                case LogEvent.EventCategory.Information:
                    header = "Info";
                    break;

                case LogEvent.EventCategory.Warning:
                    header = "Warning";
                    break;

                case LogEvent.EventCategory.Error:
                    header = "Error";
                    break;

                case LogEvent.EventCategory.Debug:
                    header = "Debug";
                    break;
            }
            //output = String.Format("[{0}] [{1}]:\t{2}", timestamp.ToString(), header, message);
            output = $@"{timestamp} {header}: {message}";
            return output;
        }

        private static Color BrushColorFromCategory(LogEvent.EventCategory category)
        {
            Color color = Colors.Black;
            switch (category)
            {
                case LogEvent.EventCategory.Warning:
                    color = Colors.DarkOrange;
                    break;

                case LogEvent.EventCategory.Error:
                    color = Colors.Crimson;
                    break;

                case LogEvent.EventCategory.Notify:
                    color = Colors.Green;
                    break;
            }
            return color;
        }

        public static void NewEvent(LogEvent.EventCategory category, string message)
        {
            if(!App.Current.Dispatcher.CheckAccess())
            {
                App.Current.Dispatcher.BeginInvoke(new Action(() => NewEvent(category, message)));
                return;
            }
            LogEvent ev = new()
            {
                Category = category,
                Timestamp = DateTime.Now,
                Message = message,
                BrushColor = BrushColorFromCategory(category)
            };
            Events.Add(ev);
        }

        public static void LogException(Exception ex)
        {
            // Show the exception message by default
            string message = ex.Message;

            // If possible, get a more detailed message consisting of
            // some data from the stack frame. I heart C#.
            System.Diagnostics.StackTrace stackTrace = new(ex);
            var frame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
            if (frame is not null)
            {
                MethodBase? mb = frame.GetMethod();
                if (mb is not null)
                {
                    message = $"{mb.ReflectedType}.{mb.Name}{Environment.NewLine}{ex.Message}";
                }
            }
            NewEvent(LogEvent.EventCategory.Warning, message);

            if(Eudora.Net.Properties.Settings.Default.EnableErrorReporting)
            {
                IssueReporter.ReportException(ex);
            }
        }

        public static void DumpToFile()
        {
            try
            {
                string filename = $"EudoraEventLog_{DateTime.Now:ddMMyyyy-hhmm}.txt";
                string fullPath = Path.Combine(Path.GetTempPath(), filename);
                string json = JsonSerializer.Serialize(Events, IoUtil.JsonWriterOptions);
                File.WriteAllText(fullPath, json);
                using Process fileopener = new();
                fileopener.StartInfo.FileName = "explorer";
                fileopener.StartInfo.Arguments = "\"" + fullPath + "\"";
                fileopener.Start();
            }
            catch(Exception ex)
            {
                LogException(ex);
            }
        }
    }
}
