using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MailKit;

namespace Eudora.Net.Core
{
    internal static class IssueReporter
    {
        public static void ReportException(Exception ex)
        {
            try
            {
                string template = GHelpers.LoadResourceFileAsString("Eudora.Net.HtmlTemplates.ExceptionReport.html");
                template.Replace("@Model.ExceptionType", ex.GetType().Name);
                template.Replace("@Model.ExceptionMessage", ex.Message);
                template.Replace("@Model.ExceptionStackTrace", ex.StackTrace);

                Data.EmailMessage email = new();
                email.Subject = "Eudora.Net Exception Report";
                email.Body = template;
                email.Addresses_To.Add(new Data.EmailAddress("Eudora.Net Support", ""));
                email.SenderAddress = new Data.EmailAddress("Eudora.Net Support", "");
            }
            catch(Exception e)
            {
                Logger.LogException(e);
            }
        }

        public static void ReportFeedback(string feedback)
        {
            try
            {
                // The text originates in a Windows TextBox
                feedback = feedback.Replace("\r\n", "<br/>");

                string template = GHelpers.LoadResourceFileAsString("Eudora.Net.HtmlTemplates.FeedbackReport.html");
                template.Replace("@Model.Feedback", feedback);

                Data.EmailMessage email = new();
                email.Subject = "Eudora.Net Feedback";
                email.Body = template;
                email.Addresses_To.Add(new Data.EmailAddress("Eudora.Net Support", ""));
                email.SenderAddress = new Data.EmailAddress("Eudora.Net Support", "");
            }
            catch(Exception ex)
            {
                Logger.LogException(ex);
            }
        }
    }
}
