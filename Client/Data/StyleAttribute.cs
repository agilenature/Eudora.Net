using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Xml.Linq;
using Eudora.Net.Core;
using Eudora.Net.ExtensionMethods;

namespace Eudora.Net.Data
{
    public class StyleAttribute : INotifyPropertyChanged
    {
        ///////////////////////////////////////////////////////////
        #region INotifyPropertyChanged
        /////////////////////////////

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // A one-shot property setter which only changes the field and invokes ProperyChanged
        // if the new value is actually new.
        // Returns true if the backing field changes, false otherwise.
        protected bool SetField<TField>(ref TField field, TField value, string propertyName)
        {
            if (EqualityComparer<TField>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        /////////////////////////////
        #endregion INotifyPropertyChanged
        ///////////////////////////////////////////////////////////



        ///////////////////////////////////////////////////////////
        #region Fields
        /////////////////////////////

        public const string property_BackgroundColor = @"background-color";
        public const string property_BackgroundImage = @"background-image";
        public const string property_BackgroundRepeat = @"background-repeat";
        public const string property_FontFamily = @"font-family";
        public const string property_FontSize = @"font-size";
        public const string property_FontStyle = @"font-style";
        public const string property_FontColor = @"color";
        public const string property_FontWeight = @"font-weight";
        public const string property_TextAlign = @"text-align";
        public const string property_TextDecoration = @"text-decoration";

        public const string value_normal = @"normal";
        public const string value_none = @"none";
        public const string value_bold = @"bold";
        public const string value_italic = @"italic";
        public const string value_underline = @"underline";

        public const string font_xxsmall = @"xx-small";
        public const string font_xsmall = @"x-small";
        public const string font_small = @"small";
        public const string font_medium = @"medium";
        public const string font_large = @"large";
        public const string font_xlarge = @"x-large";
        public const string font_xxlarge = @"xx-large";
        public static readonly ObservableCollection<string> FontSizes =
            [
            font_xxsmall,
            font_xsmall,
            font_small,
            font_medium,
            font_large,
            font_xlarge,
            font_xxlarge
            ];

        public static readonly string alignLeft = @"left";
        public static readonly string alignRight = @"right";
        public static readonly string alignCenter = @"center";
        public static readonly string alignJustify = @"justify";
        public static readonly string alignStart = @"start";
        public static readonly string alignEnd = @"end";
        public static readonly ObservableCollection<string> TextAlignments =
            [
            alignLeft,
            alignRight,
            alignCenter,
            alignJustify,
            alignStart,
            alignEnd
            ];

        public static readonly string bgRepeatNone = @"no-repeat";
        public static readonly string bgRepeat = @"repeat";
        public static readonly string bgRepeatX = @"repeat-x";
        public static readonly string bgRepeatY = @"repeat-y";
        public static readonly ObservableCollection<string> BackgroundRepeats =
            [
            bgRepeatNone,
            bgRepeat,
            bgRepeatX,
            bgRepeatY
            ];

        private string _Value = string.Empty;
        private Color _BackgroundColor = Colors.White;
        private object _BackgroundImage;
        private string _BackgroundRepeat = string.Empty;
        private FontFamily _Font = Fonts.SystemFontFamilies.First();
        private Color _FontColor = Colors.Black;
        private string _FontSize = font_medium;
        private string _FontStyle = value_normal;
        private string _FontWeight = value_normal;
        private string _TextAlignment = alignLeft;
        private string _TextDecoration = value_none;

        /////////////////////////////
        #endregion Fields
        ///////////////////////////////////////////////////////////



        ///////////////////////////////////////////////////////////
        #region Properties
        /////////////////////////////

        public string Value
        {
            get => _Value;
            set
            {
                ParseValue(value);
                SetField(ref _Value, value, nameof(Value));
            }
        }

        public Color BackgroundColor
        {
            get => _BackgroundColor;
            set => SetField(ref _BackgroundColor, value, nameof(BackgroundColor));
        }

        public object BackgroundImage
        {
            get => _BackgroundImage;
            set => SetField(ref _BackgroundImage, value, nameof(BackgroundImage));
        }

        public string BackgroundRepeat
        {
            get => _BackgroundRepeat;
            set => SetField(ref _BackgroundRepeat, value, nameof(BackgroundRepeat));
        }

        public FontFamily Font
        {
            get => _Font;
            set => SetField(ref _Font, value, nameof(Font));
        }

        public Color FontColor
        {
            get => _FontColor;
            set => SetField(ref _FontColor, value, nameof(FontColor));
        }

        public string FontSize
        {
            get => _FontSize;
            set => SetField(ref _FontSize, value, nameof(FontSize));
        }

        public string FontStyle
        {
            get => _FontStyle;
            set => SetField(ref _FontStyle, value, nameof(FontStyle));
        }

        public string FontWeight
        {
            get => _FontWeight;
            set => SetField(ref _FontWeight, value, nameof(FontWeight));
        }

        public string TextAlignment
        {
            get => _TextAlignment;
            set => SetField(ref _TextAlignment, value, nameof(TextAlignment));
        }

        public string TextDecoration
        {
            get => _TextDecoration;
            set => SetField(ref _TextDecoration, value, nameof(TextDecoration));
        }

        /////////////////////////////
        #endregion Properties
        ///////////////////////////////////////////////////////////

        static StyleAttribute()
        {

        }

        public StyleAttribute()
        {

        }

        public void SetDefaults()
        {
            //_Value = string.Empty;
            BackgroundColor = Colors.White;
            //BackgroundImage = null;
            BackgroundRepeat = string.Empty;
            Font = Fonts.SystemFontFamilies.Where(
                font => font.Source.Equals("arial", StringComparison.CurrentCultureIgnoreCase)).First();
            FontColor = Colors.Black;
            FontSize = font_medium;
            FontStyle = value_normal;
            FontWeight = value_normal;
            TextAlignment = alignLeft;
            TextDecoration = value_none;
        }

        private async void ParseValue(string value)
        {
            try
            {
                SetDefaults();
                Color color = new();
                string[] components = value.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                foreach (string component in components)
                {
                    string[] pair = component.Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    if (pair.Length == 2)
                    {
                        //Debug.WriteLine($@"{pair[0]} : {pair[1]}");
                        switch (pair[0])
                        {
                            case property_BackgroundColor:
                                BackgroundColor = color.FromJavascriptRGB(pair[1]);
                                break;

                            case property_BackgroundImage:
                                BackgroundImage = pair[1];
                                break;

                            case property_BackgroundRepeat:
                                BackgroundRepeat = pair[1];
                                break;

                            case property_FontFamily:
                                if (new FontFamilyConverter().ConvertFrom(pair[1]) is FontFamily font)
                                {
                                    Font = font;
                                }
                                break;

                            case property_FontSize:
                                FontSize = pair[1];
                                break;

                            case property_FontStyle:
                                FontStyle = pair[1];
                                break;

                            case property_FontWeight:
                                FontWeight = pair[1];
                                break;

                            case property_FontColor:
                                FontColor = color.FromJavascriptRGB(pair[1]);
                                break;

                            case property_TextAlign:
                                TextAlignment = pair[1];
                                break;

                            case property_TextDecoration:
                                TextDecoration = pair[1];
                                break;

                            default:
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }
    }
}
