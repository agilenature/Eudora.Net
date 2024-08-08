using Eudora.Net.Core;
using Microsoft.Toolkit.Uwp.Notifications;
using Eudora.Net.Data;
using Windows.UI.Notifications;

namespace Eudora.Net.GUI
{
    internal static class Notifier
    {
        private static readonly string GeneralNotificationID = "Eudora.Net.Notify.General";
        private static readonly string EmailNotificationID = "Eudora.Net.Notify.Email";


        static Notifier()
        {
        }

        public static void NotifyGeneral(string title, string message)
        {
            try
            {
                var toastContent = new ToastContentBuilder()
                    .AddHeader(GeneralNotificationID, "Eudora.Net", "")
                    .AddText(title)
                    .AddText(message);

                ToastNotification toast = new ToastNotification(toastContent.GetXml())
                {
                    Tag = GeneralNotificationID
                };

                ToastNotificationManager.CreateToastNotifier().Show(toast);
                new ToastContentBuilder()
                
                .Show();
            }
            catch(Exception ex)
            {
                Logger.Exception(ex);
            }
        }

        public static void NotifyNewEmail(EmailMessage message)
        {
            try
            {
                var toastContent = new ToastContentBuilder()
                    .AddHeader(EmailNotificationID, "Eudora.Net - New Email", "")
                    .AddText(message.SenderAddress.DisplayString)
                    .AddText(message.Subject);

                ToastNotification toast = new ToastNotification(toastContent.GetXml())
                {
                    Tag = EmailNotificationID
                };

                ToastNotificationManager.CreateToastNotifier().Show(toast);
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
            }
        }
    }
}
