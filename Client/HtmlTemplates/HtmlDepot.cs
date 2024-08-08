using Eudora.Net.Core;
using Eudora.Net.Data;
using System.IO;
using System.Reflection;

namespace Eudora.Net.HtmlTemplates
{
    internal static class HtmlDepot
    {
        // html templates source
        private static readonly string HtmlDocument = "Eudora.Net.HtmlTemplates.HtmlDocument.html";
        private static readonly string Email = "Eudora.Net.HtmlTemplates.Email.html";
        private static readonly string EmailReply = "Eudora.Net.HtmlTemplates.EmailReply.html";
        private static readonly string EmailForward = "Eudora.Net.HtmlTemplates.EmailForward.html";
        private static readonly string Signature = "Eudora.Net.HtmlTemplates.Signature.html";
        private static readonly string Stationery = "Eudora.Net.HtmlTemplates.Stationery.html";

        // loaded templates by name
        public static string html_Document { get; private set; } = string.Empty;
        public static string html_Email { get; private set; } = string.Empty;
        public static string html_Reply { get; private set; } = string.Empty;
        public static string html_Forward { get; private set; } = string.Empty;
        public static string html_Signature { get; private set; } = string.Empty;
        public static string html_Stationery { get; private set; } = string.Empty;

        // template tokens (for search & replace)
        //private static readonly string token_BodyInsert = "{BODY_INSERT}";
        private static readonly string token_Date = "{DATE}";
        private static readonly string token_Sender = "{SENDER}";
        private static readonly string token_Subject = "{SUBJECT}";
        private static readonly string token_Addressee = "{ADDRESSEE}";
        private static readonly string token_QuoteBody = "{QUOTEBODY}";


        static HtmlDepot()
        {
            html_Document = LoadResource(HtmlDocument);
            html_Email = LoadResource(Email);
            html_Reply = LoadResource(EmailReply);
            html_Forward = LoadResource(EmailForward);
            html_Signature = LoadResource(Signature);
            html_Stationery = LoadResource(Stationery);
        }

        static string LoadResource(string resourceName)
        {
            string fileString = GHelpers.LoadResourceFileAsString(resourceName);
            return fileString;
        }

        public static string MakeDocument_Signature()
        {
            return string.Empty;
        }

        public static string MakeDocument_Stationery()
        {
            return string.Empty;
        }

        public static string MakeDocument_Email()
        {
            return string.Empty;
        }

        public static string MakeBlankEmail(EmailMessage inMessage)
        {
            return html_Email;
        }

        public static string MakeNonBlankEmail(EmailMessage inMessage)
        {
            string email = string.Empty;



            return email;
        }

        public static string MakeEmailReply(EmailMessage inMessage)
        {
            string email = string.Empty;

            email = html_Reply;
            email = email.Replace(token_Date, inMessage.Date.ToString("dddd, dd MMMM yyyy"));
            email = email.Replace(token_Sender, inMessage.SenderAddress.DisplayString);
            email = email.Replace(token_QuoteBody, inMessage.Body);

            return email;
        }

        public static string MakeEmailForward(EmailMessage inMessage)
        {
            string email = string.Empty;

            Personality? personality = PersonalityManager.FindPersonality(inMessage.PersonalityID);
            if (personality is null)
            {
                Logger.Debug($"Failed to find Personality with id: {inMessage.PersonalityID}");
                return email;
            }

            email = html_Forward;
            email = email.Replace(token_Date, inMessage.Date.ToString("dddd, dd MMMM yyyy"));
            email = email.Replace(token_Sender, inMessage.SenderAddress.DisplayString);
            email = email.Replace(token_Addressee, personality.ToEmailAddress().DisplayString);
            email = email.Replace(token_Subject, inMessage.Subject);
            email = email.Replace(token_QuoteBody, inMessage.Body);

            return email;
        }
    }
}
