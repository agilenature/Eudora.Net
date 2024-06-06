using System.Windows.Input;

namespace Eudora.Net.GUI
{
    public static class Commands_EmailView
    {
        /// <summary>
        /// Set this draft email to HTML type
        /// </summary>
        public static readonly RoutedUICommand Command_Email_HTML = new RoutedUICommand
            (
                "HTML",
                "HTML",
                typeof(Commands_EmailView)
            );

        /// <summary>
        /// Set this draft email to Text type
        /// </summary>
        public static readonly RoutedUICommand Command_Email_Text = new RoutedUICommand
            (
                "Plain Text",
                "Plain Text",
                typeof(Commands_EmailView)
            );

        /// <summary>
        /// Send/queue this draft email
        /// </summary>
        public static readonly RoutedUICommand Command_Email_Send = new RoutedUICommand
            (
                "Send",
                "Send",
                typeof(Commands_EmailView)
            );
    }
}
