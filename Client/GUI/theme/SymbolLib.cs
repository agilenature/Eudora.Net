using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfThemer;


namespace Eudora.Net.GUI.theme
{
    internal static class SymboLib
    {
        public static void Build()
        {
            string category = "light";

            List<string> symbolNames =
                [
                    "add",
                    "add_attachment",
                    "add_image",
                    "arrow_back",
                    "arrow_drop_down",
                    "arrow_forward",
                    "arrow_right",
                    "attach_email",
                    "attach_file",
                    "bookmark",
                    "bookmark_add",
                    "bookmarks",
                    "cancel",
                    "check",
                    "check_circle",
                    "chevron_left",
                    "chevron_right",
                    "close",
                    "contact",
                    "content_copy",
                    "content_cut",
                    "content_paste",
                    "delete",
                    "draft",
                    "drafts",
                    "edit",
                    "edit2",
                    "email",
                    "expand_circle",
                    "favorite",
                    "file_open",
                    "find",
                    "folder",
                    "format_align_center",
                    "format_align_left",
                    "format_align_right",
                    "format_bold",
                    "format_italic",
                    "format_underlined",
                    "group",
                    "help",
                    "home",
                    "image",
                    "inbox",
                    "language",
                    "library_add",
                    "link",
                    "login",
                    "mail",
                    "mark_as_unread2",
                    "mark_email_unread",
                    "menu",
                    "menu_book",
                    "minimize",
                    "more_horiz",
                    "more_vert",
                    "person",
                    "person_add",
                    "question_mark",
                    "redo",
                    "refresh",
                    "remove",
                    "search",
                    "send",
                    "settings",
                    "share",
                    "sort",
                    "star",
                    "sync",
                    "toggle_off",
                    "toggle_on",
                    "undo",
                    "zoom_in",
                    "zoom_out"
                ];

            Theme.eThemeType themeType = Theme.eThemeType.Light;
            for (int i = 0; i < 3; i++)
            {
                if(i == 0)
                {
                    themeType = Theme.eThemeType.Light;
                    category = "dark";
                }
                else
                if(i == 1)
                {
                    themeType = Theme.eThemeType.Dark;
                    category = "light";
                }
                else if(i == 2)
                {
                    themeType = Theme.eThemeType.Undefined;
                    category = "dark";
                }
                string path = $"pack://application:,,,/GUI/theme/symbols/{category}/";

                foreach(var symbol in symbolNames)
                {
                    WpfThemer.ThemeSymbolManager.AddSymbol(symbol, new Uri($"{path}{symbol}.png"), themeType);
                }
            }
        }
    }
}
