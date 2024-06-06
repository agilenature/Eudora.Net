using System.Windows;
using System.Windows.Controls;

namespace Eudora.Net.GUI
{
    public class BookmarkButton : Button
    {
        ///////////////////////////////////////////////////////////
        #region Dependency Properties
        /////////////////////////////

        public static readonly DependencyProperty DisplayTextProperty =
            DependencyProperty.Register("DisplayText", typeof(String), typeof(BookmarkButton), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty IconImageProperty =
            DependencyProperty.Register("IconImage", typeof(Uri), typeof(BookmarkButton), new PropertyMetadata(null));

        public static readonly DependencyProperty BookmarkProperty =
            DependencyProperty.Register("Bookmark", typeof(String), typeof(BookmarkButton), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty TooltipProperty =
            DependencyProperty.Register("Tooltip", typeof(String), typeof(BookmarkButton), new PropertyMetadata(string.Empty));

        /////////////////////////////
        #endregion Dependency Properties
        ///////////////////////////////////////////////////////////


        ///////////////////////////////////////////////////////////
        #region Properties
        /////////////////////////////

        public String DisplayText
        {
            get { return (String)GetValue(DisplayTextProperty); }
            set { SetValue(DisplayTextProperty, value); }
        }

        public Uri IconImage
        {
            get { return (Uri)GetValue(IconImageProperty); }
            set { SetValue(IconImageProperty, value); }
        }

        public String Bookmark
        {
            get { return (String)GetValue(BookmarkProperty); }
            set { SetValue(BookmarkProperty, value); }
        }

        public String Tooltip
        {
            get { return (String)GetValue(TooltipProperty); }
            set { SetValue(BookmarkProperty, value); }
        }

        /////////////////////////////
        #endregion Properties
        ///////////////////////////////////////////////////////////

        public BookmarkButton() : base()
        {
            MaxWidth = 125;
        }
    }
}
