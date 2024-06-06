using System.Windows;

namespace Eudora.Net.GUI.Themes
{
    class ThemeManager
    {
        public static void SetTheme(Uri themeUri)
        {
            ResourceDictionary rd = new()
            {
                Source = themeUri
            };

            Application.Current.Resources.Clear();
            Application.Current.Resources.MergedDictionaries.Add(rd);
        }
    }
}
