using Eudora.Net.Core;
using Microsoft.Web.WebView2.Wpf;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Eudora.Net.ExtensionMethods
{
    /// <summary>
    /// A markup extension for easily databinding an enum
    /// Usage:
    /// <ComboBox ItemsSource="{Binding Source={c:EnumValues {x:Type src:MyEnum}}}"/>
    /// </summary>
    public class EnumValuesExtension : MarkupExtension
    {
        private readonly Type _enumType;

        public EnumValuesExtension(Type enumType)
        {
            if (enumType == null)
            {
                throw new ArgumentNullException("enumType");
            }
            if (!enumType.IsEnum)
            {
                throw new ArgumentException("Argument enumType must derive from type Enum.");
            }

            _enumType = enumType;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Enum.GetValues(_enumType);
        }
    }


    /// <summary>
    /// Extension methods for built-in types
    /// </summary>
    public static class Extensions
    {
        public const uint GW_CHILD = 5;

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

        [DllImport("user32.dll")]
        public static extern IntPtr SetFocus(IntPtr hWnd);


        /// <summary>
        /// Enable & Disable UIElements in a Grid (i.e the Eudora main toolbar)
        /// </summary>
        /// <param name="grid"></param>
        public static void EnbleAllControls(this Grid grid)
        {
            foreach (var child in grid.Children)
            {
                if (child is UIElement element)
                {
                    element.IsEnabled = true;
                }
            }
        }

        public static void DisableAllControls(this Grid grid)
        {
            foreach (var child in grid.Children)
            {
                if (child is UIElement element)
                {
                    element.IsEnabled = false;
                }
            }
        }


        /// <summary>
        /// Enable & Disable UIElements in a StackPanel
        /// </summary>
        /// <param name="panel"></param>
        public static void EnbleAllControls(this StackPanel panel)
        {
            foreach(var child in panel.Children)
            {
                if(child is UIElement element)
                {
                    element.IsEnabled = true;
                }
            }
        }

        public static void DisableAllControls(this StackPanel panel)
        {
            foreach (var child in panel.Children)
            {
                if (child is UIElement element)
                {
                    element.IsEnabled = false;
                }
            }
        }


        /// <summary>
        /// Enable all items and subitems in a menu
        /// </summary>
        public static void EnableAllSubitems(this MenuItem item)
        {
            foreach (var i in item.Items)
            {
                if (i is MenuItem subitem)
                {
                    subitem.IsEnabled = true;
                    EnableAllSubitems(subitem);
                }
            }
        }
        public static void EnableAllItems(this Menu menu)
        {
            foreach (var item in menu.Items)
            {
                if (item is MenuItem mi)
                {
                    mi.IsEnabled = true;
                    EnableAllSubitems(mi);
                }
            }
        }

        /// <summary>
        /// Disable all items and subitems in a menu
        /// </summary>
        public static void DisableAllSubitems(this MenuItem item)
        {
            foreach (var i in item.Items)
            {
                if (i is MenuItem subitem)
                {
                    subitem.IsEnabled = false;
                    DisableAllSubitems(subitem);
                }
            }
        }
        public static void DisableAllItems(this Menu menu)
        {
            foreach (var item in menu.Items)
            {
                if (item is MenuItem mi)
                {
                    mi.IsEnabled = false;
                    DisableAllSubitems(mi);
                }
            }
        }


        /// <summary>
        /// Try to set a WebView2's focus to the inner Chromium widget/window
        /// </summary>
        /// <param name="webView2"></param>
        public static void InnerFocus(this WebView2 webView2)
        {
            var chromeWidgetWin0 = GetWindow(webView2.Handle, GW_CHILD);
            SetFocus(chromeWidgetWin0);
        }

        /// <summary>
        /// Turn an html/js color into a Windows.Media.Color
        /// </summary>
        /// <param name="incolor"></param>
        /// <param name="rgb"></param>
        /// <returns></returns>
        public static Color FromJavascriptRGB(this Color incolor, string rgb)
        {
            Color color = new();

            if(rgb.Trim().StartsWith("rgb("))
            {
                try
                {
                    int start = rgb.IndexOf('(') + 1;
                    int end = rgb.IndexOf(')');
                    int substringLength = end - start;
                    string working = rgb.Substring(start, substringLength);
                    string[] values = working.Split(",", StringSplitOptions.TrimEntries);
                    if (values.Length == 3)
                    {
                        color.R = Convert.ToByte(values[0]);
                        color.G = Convert.ToByte(values[1]);
                        color.B = Convert.ToByte(values[2]);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Exception(ex);
                }
            }

            return color;
        }

        /// <summary>
        /// Parse a Windows.Media.Color into a javascript-compatible RBG string
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static string ToJavascriptRGB(this Color color)
        {
            return $"rgb({color.R}, {color.G}, {color.B})";
        }

        /// <summary>
        /// Extends System.Drawing.Icon with a method that converts the icon image
        /// to a WPF-compatible ImageSource.
        /// </summary>
        /// <param name="icon"></param>
        /// <returns></returns>
        public static ImageSource ToImageSoure(this System.Drawing.Icon icon)
        {
            ImageSource imgsrc = Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            return imgsrc;
        }


        /// <summary>
        /// An explicit call for programatically sorting a DataGrid control by column;
        /// The effect is the same as if the user clicked on the column header.
        /// Note: Column sorting must be enabled for this to do anything
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="columnIndex"></param>
        /// <param name="direction"></param>
        public static void Sort(this DataGrid grid, int columnIndex, ListSortDirection direction)
        {
            try
            {
                var column = grid.Columns[columnIndex];
                if (column is null) return;

                grid.Items.SortDescriptions.Clear();
                grid.Items.SortDescriptions.Add(new SortDescription(column.SortMemberPath, direction));

                foreach (var col in grid.Columns)
                {
                    col.SortDirection = null;
                }
                column.SortDirection = direction;
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
            }

            grid.Items.Refresh();
        }


        /// <summary>
        /// Extension method for List<T>, where the add only occurs if the value is unique
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="value"></param>
        public static void AddUnique<T>(this List<T> collection, T value)
        {
            if (!collection.Contains(value))
            {
                collection.Add(value);
            }
        }

        /// <summary>
        /// AddRange with per-element filtering of duplicates
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collectionDest"></param>
        /// <param name="collectionSrc"></param>
        public static void AddUniqueRange<T>(this List<T> collectionDest, List<T> collectionSrc)
        {
            foreach(var t in collectionSrc)
            {
                if(!collectionDest.Contains(t))
                {
                    collectionDest.Add(t);
                }
            }
        }

        /// <summary>
        /// Extension method, alternate version of Add for ObservableCollection<T>.
        /// If the given value is already in the collection, a duplicate is not added.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="value"></param>
        public static void AddUnique<T>(this ObservableCollection<T> collection, T value)
        {
            if(!collection.Contains(value))
            {
                collection.Add(value);
            }
        }


        /// <summary>
        /// Adds AddRange to ObservableCollection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collectionDest"></param>
        /// <param name="collectionSource"></param>
        public static void AddRange<T>(this ObservableCollection<T> collectionDest, ObservableCollection<T> collectionSource)
        {
            foreach(var t in collectionSource)
            {
                collectionDest.Add(t);
            }
        }

        /// <summary>
        /// Adds AddRange to ObservableCollection, filtering out duplicates
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collectionDest"></param>
        /// <param name="collectionSource"></param>
        public static void AddRangeUnique<T>(this ObservableCollection<T> collectionDest, ObservableCollection<T> collectionSource)
        {
            foreach (var t in collectionSource)
            {
                collectionDest.AddUnique(t);
            }
        }


        /// <summary>
        /// Find the child controls of a control (or other visual, like Window)
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="recurse"></param>
        /// <returns></returns>
        public static IEnumerable<Visual> GetChildren(this Visual parent, bool recurse = true)
        {
            if (parent != null)
            {
                int count = VisualTreeHelper.GetChildrenCount(parent);
                for (int i = 0; i < count; i++)
                {
                    // Retrieve child visual at specified index value.
                    var child = VisualTreeHelper.GetChild(parent, i) as Visual;
                    if (child != null)
                    {
                        yield return child;

                        if (recurse)
                        {
                            foreach (var grandChild in child.GetChildren(true))
                            {
                                yield return grandChild;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Find the parent node of a given TreeViewItem.
        /// Usage: node.ParentNode()
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static TreeViewItem? ParentNode(this TreeViewItem item)
        {
            try
            {
                var parent = VisualTreeHelper.GetParent(item as DependencyObject);
                while ((parent as TreeViewItem) == null)
                {
                    parent = VisualTreeHelper.GetParent(parent);
                }
                return parent as TreeViewItem;
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
                return null;
            }
        }
    }
}
