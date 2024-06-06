using System.Collections.ObjectModel;
using Eudora.Net.Data;

namespace Eudora.Net.Core
{
    internal class EmailFilterReport(string subject, string filter, DateTime timestamp)
    {
        public string Subject { get; set; } = subject;
        public string Filter { get; set; } = filter;
        public DateTime Timestamp { get; set; } = timestamp;
    }


    internal static class EmailFilterReporter
    {
        public static ObservableCollection<EmailFilterReport> Reports { get; set; } = [];

        public static void NewReport(EmailMessage messge, EmailFilter filter)
        {
            Reports.Add(new(messge.Subject, filter.Name, DateTime.Now));
        }
    }
}
