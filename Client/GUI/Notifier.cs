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
        private static ToastNotifierCompat? TN = default;


        static Notifier()
        {
        }

        public static void NotifyGeneral(string title, string message)
        {
            try
            {
                TN ??= ToastNotificationManagerCompat.CreateToastNotifier();

                var toastContent = new ToastContentBuilder()
                    .AddHeader(GeneralNotificationID, "Eudora.Net", "")
                    .AddText(title)
                    .AddText(message);

                ToastNotification toast = new(toastContent.GetXml())
                {
                    Tag = GeneralNotificationID
                };

                TN.Show(toast);
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
                TN ??= ToastNotificationManagerCompat.CreateToastNotifier();

                var toastContent = new ToastContentBuilder()
                    .AddHeader(EmailNotificationID, "Eudora.Net: New Email", "")
                    .AddText(message.SenderAddress.DisplayString)
                    .AddText(message.Subject);

                ToastNotification toast = new(toastContent.GetXml())
                {
                    Tag = EmailNotificationID
                };

                TN.Show(toast);
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
            }
        }
    }
}
