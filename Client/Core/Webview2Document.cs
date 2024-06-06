using Eudora.Net.Data;
using Eudora.Net.ExtensionMethods;
using Eudora.Net.Javascript;
using System.ComponentModel;
using System.Diagnostics;

//using System.Drawing;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;



namespace Eudora.Net.Core
{

    /// <summary>
    /// Serves the same purpose as the old MSHTML HtmlDocument class.
    /// The direct interaction between browser content and designer UX happens
    /// via this class.
    /// 
    /// Communication between the app and the browser is done via
    /// WebView2's built-in mechanisms of ExecuteScriptAsync on the C# side
    /// and WebMessage on the browser side.
    /// </summary>
    public class Webview2Document : INotifyPropertyChanged
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

        private Microsoft.Web.WebView2.Wpf.WebView2 Webview;

        // string interpolation consts to avoid a whole category of bugs
        private static readonly string token_StyleAttribute = "style";
        private static readonly string token_innerHTML = "innerHTML";
        private static readonly string token_outerHTML = "outerHTML";
        private static readonly string token_tagName = "tagName";
        private static readonly string token_id = "id";
        private static readonly string token_On = "on";
        private static readonly string token_Off = "off";

        // webmessage ids
        private static readonly string msgid = "msgid";
        private static readonly string msgid_MouseClick = "MouseClick";
        private static readonly string msgid_KeyDown = "KeyDown";
        private static readonly string msgid_BodyChanged = "BodyChanged";
        private static readonly string msgid_SelectionChange = "SelectionChange";

        // HTML document properties
        private FontFamily _ActiveFont;
        //private FontFamily _ActiveFont = Fonts.SystemFontFamilies.Where(
        //    font => font.Source.Equals("arial", StringComparison.CurrentCultureIgnoreCase)).First();
        private string _ActiveFontSize = StyleAttribute.font_medium;
        private Color _ActiveFontColor = Colors.Black;
        private bool _ActiveFontBold = false;
        private bool _ActiveFontItalic = false;
        private bool _ActiveFontUnderline = false;
        private bool _ActiveAlignmentLeft = true;
        private bool _ActiveAlignmentCenter = false;
        private bool _ActiveAlignmentRight = false;
        private Color _ActiveBackgroundColor = Colors.White;
        

        private bool _DesignMode = false;
        private string _BodyInnerHTML = string.Empty;
        private string _BodyOuterHTML = string.Empty;
        private StyleAttribute _BodyStyle = new();
        private Point _BrowserCursorPosition = new(0, 0);
        private string _CursorElementType = string.Empty;
        private string _CursorElementId = string.Empty;
        private string _CursorElementInnerHTML = string.Empty;
        private string _CursorElementOuterHTML = string.Empty;
        private StyleAttribute _CursorElementStyle = new();

        /////////////////////////////
        #endregion Fields
        ///////////////////////////////////////////////////////////


        ///////////////////////////////////////////////////////////
        #region Properties
        /////////////////////////////

        public bool DesignMode
        {
            get => _DesignMode;
            set
            {
                if (SetField(ref _DesignMode, value, nameof(DesignMode)))
                {
                    ExecuteScript(JsDepot.js_DesignMode, [_DesignMode ? token_On : token_Off]);
                }
            }
        }

        public FontFamily ActiveFont
        {
            get => _ActiveFont;
            set
            {
                if (SetField(ref _ActiveFont, value, nameof(ActiveFont)))
                {
                    if (CursorElementStyle.Font != ActiveFont)
                    {
                        SetFont(value);
                    }
                }
            }
        }

        public string ActiveFontSize
        {
            get => _ActiveFontSize;
            set
            {
                if(SetField(ref _ActiveFontSize, value, nameof(ActiveFontSize)))
                {
                    if (CursorElementStyle.FontSize != ActiveFontSize)
                    {
                        SetFontSize(value);
                    }
                }
            }
        }
        
        public Color ActiveFontColor
        {
            get => _ActiveFontColor;
            set
            {
                if (SetField(ref _ActiveFontColor, value, nameof(ActiveFontColor)))
                {
                    if (CursorElementStyle.FontColor != ActiveFontColor)
                    {
                        SetFontColor(value);
                    }
                }
            }
        }

        public bool ActiveFontBold
        {
            get => _ActiveFontBold;
            set
            {
                if (SetField(ref _ActiveFontBold, value, nameof(ActiveFontBold)))
                {
                    if(CursorElementStyle.FontWeight != (value ? StyleAttribute.value_bold : StyleAttribute.value_normal))
                    {
                        ToggleBold(value);
                    }
                }
            }
        }

        public bool ActiveFontItalic
        {
            get => _ActiveFontItalic;
            set
            {
                if (SetField(ref _ActiveFontItalic, value, nameof(ActiveFontItalic)))
                {
                    if (CursorElementStyle.FontStyle != (value ? StyleAttribute.value_italic : StyleAttribute.value_normal))
                    {
                        ToggleItalic(value);
                    }
                }
            }
        }

        public bool ActiveFontUnderline
        {
            get => _ActiveFontUnderline;
            set
            {
                if(SetField(ref _ActiveFontUnderline, value, nameof(ActiveFontUnderline)))
                {
                    if (CursorElementStyle.TextDecoration != (value ? StyleAttribute.value_underline : StyleAttribute.value_none))
                    {
                        ToggleUnderline(value);
                    }
                }
            }
        }
        
        public bool ActiveAlignmentLeft
        {
            get => _ActiveAlignmentLeft;
            set
            {
                if (SetField(ref _ActiveAlignmentLeft, value, nameof(ActiveAlignmentLeft)))
                {
                    if (value)
                    {
                        if(CursorElementStyle.TextAlignment != StyleAttribute.alignLeft)
                        {
                            ApplyTextAlignment(StyleAttribute.alignLeft);
                        }
                        ActiveAlignmentCenter = false;
                        ActiveAlignmentRight = false;
                    }
                }
            }
        }

        public bool ActiveAlignmentCenter
        {
            get => _ActiveAlignmentCenter;
            set
            {
                if (SetField(ref _ActiveAlignmentCenter, value, nameof(ActiveAlignmentCenter)))
                {
                    if (value)
                    {
                        if (CursorElementStyle.TextAlignment != StyleAttribute.alignCenter)
                        {
                            ApplyTextAlignment(StyleAttribute.alignCenter);
                        }
                        ActiveAlignmentLeft = false;
                        ActiveAlignmentRight = false;
                    }
                }
            }
        }

        public bool ActiveAlignmentRight
        {
            get => _ActiveAlignmentRight;
            set
            {
                if (SetField(ref _ActiveAlignmentRight, value, nameof(ActiveAlignmentRight)))
                {
                    if (value)
                    {
                        if (CursorElementStyle.TextAlignment != StyleAttribute.alignRight)
                        {
                            ApplyTextAlignment(StyleAttribute.alignRight);
                        }
                        ActiveAlignmentLeft = false;
                        ActiveAlignmentCenter = false;
                    }
                }
            }
        }

        public Color ActiveBackgroundColor
        {
            get => _ActiveBackgroundColor;
            set => SetField(ref _ActiveBackgroundColor, value, nameof(ActiveBackgroundColor));
        }

        public string BodyInnerHTML
        {
            get => _BodyInnerHTML;
            set => SetField(ref _BodyInnerHTML, value, nameof(BodyInnerHTML));
        }
        
        public string BodyOuterHTML
        {
            get => _BodyOuterHTML;
            set => SetField(ref _BodyOuterHTML, value, nameof(BodyOuterHTML));
        }
        
        public StyleAttribute BodyStyle
        {
            get => _BodyStyle;
            set => SetField(ref _BodyStyle, value, nameof(BodyStyle));
        }
        
        public Point BrowserCursorPosition
        {
            get => _BrowserCursorPosition;
            set => SetField(ref _BrowserCursorPosition, value, nameof(BrowserCursorPosition));
        }
        
        public string CursorElementType
        {
            get => _CursorElementType;
            set => SetField(ref _CursorElementType, value, nameof(CursorElementType));
        }
        
        public string CursorElementId
        {
            get => _CursorElementId;
            set => SetField(ref _CursorElementId, value, nameof(CursorElementId));
        }
        
        public string CursorElementInnerHTML
        {
            get => _CursorElementInnerHTML;
            set => SetField(ref _CursorElementInnerHTML, value, nameof(CursorElementInnerHTML));
        }
        
        public string CursorElementOuterHTML
        {
            get => _CursorElementOuterHTML;
            set => SetField(ref _CursorElementOuterHTML, value, nameof(CursorElementOuterHTML));
        }

        public StyleAttribute CursorElementStyle
        {
            get => _CursorElementStyle;
            set => SetField(ref _CursorElementStyle, value, nameof(CursorElementStyle));
        }

        /////////////////////////////
        #endregion Properties
        ///////////////////////////////////////////////////////////



        ///////////////////////////////////////////////////////////
        #region Construction
        /////////////////////////////

        static Webview2Document()
        {
            
        }

        public Webview2Document(Microsoft.Web.WebView2.Wpf.WebView2 wv2)
        {
            Webview = wv2;
            Webview.NavigationCompleted += Webview_NavigationCompleted;
            Webview.WebMessageReceived += Webview_WebMessageReceived;

            CursorElementStyle.PropertyChanged += CursorElementStyle_PropertyChanged;
        }

        private void CursorElementStyle_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            //if(CursorElementId is null || CursorElementId.Length == 0)
            //{
            //    //return;
            //}
            if (e.PropertyName is null) return;
            string property = e.PropertyName.ToLower();
            switch (property)
            {
                //case "font":
                //    ActiveFont = CursorElementStyle.Font;
                //    break;
                //case "fontsize":
                //    ActiveFontSize = CursorElementStyle.FontSize;
                //    break;
                //case "fontcolor":
                //    ActiveFontColor = CursorElementStyle.FontColor;
                //    break;
                //case "fontweight":
                //    ActiveFontBold = CursorElementStyle.FontWeight.Equals(StyleAttribute.value_bold, StringComparison.CurrentCultureIgnoreCase);
                //    break;
                //case "fontstyle":
                //    ActiveFontItalic = CursorElementStyle.FontStyle.Equals(StyleAttribute.value_italic, StringComparison.CurrentCultureIgnoreCase);
                //    break;
                //case "textdecoration":
                //    ActiveFontUnderline = CursorElementStyle.TextDecoration.Equals(StyleAttribute.value_underline, StringComparison.CurrentCultureIgnoreCase);
                //    break;
                //case "textalignment":
                //    ActiveAlignmentLeft = CursorElementStyle.TextAlignment.Equals(StyleAttribute.alignLeft, StringComparison.CurrentCultureIgnoreCase);
                //    ActiveAlignmentCenter = CursorElementStyle.TextAlignment.Equals(StyleAttribute.alignCenter, StringComparison.CurrentCultureIgnoreCase);
                //    ActiveAlignmentRight = CursorElementStyle.TextAlignment.Equals(StyleAttribute.alignRight, StringComparison.CurrentCultureIgnoreCase);
                //    break;
                case "value":
                    if (string.IsNullOrEmpty(CursorElementStyle.Value))
                    {
                        //ActiveFontBold = false;
                        //ActiveFontItalic = false;
                        //ActiveFontUnderline = false;
                        //ActiveAlignmentLeft = true;
                        //ActiveAlignmentCenter = false;
                        //ActiveAlignmentRight = false;
                        //ActiveFont = BodyStyle.Font;
                        //ActiveFontColor = BodyStyle.FontColor;
                        //ActiveFontSize = BodyStyle.FontSize;
                    }
                    else
                    {
                        //Debug.WriteLine($"value changed 1. ActiveFont is currently {ActiveFont}");
                        ActiveFont = CursorElementStyle.Font;
                        //Debug.WriteLine($"value changed 2. ActiveFont is now {ActiveFont}");
                        ActiveFontSize = CursorElementStyle.FontSize;
                        ActiveFontColor = CursorElementStyle.FontColor;
                        ActiveFontBold = CursorElementStyle.FontWeight.Equals(StyleAttribute.value_bold, StringComparison.CurrentCultureIgnoreCase);
                        ActiveFontItalic = CursorElementStyle.FontStyle.Equals(StyleAttribute.value_italic, StringComparison.CurrentCultureIgnoreCase);
                        ActiveFontUnderline = CursorElementStyle.TextDecoration.Equals(StyleAttribute.value_underline, StringComparison.CurrentCultureIgnoreCase);
                        ActiveAlignmentLeft = CursorElementStyle.TextAlignment.Equals(StyleAttribute.alignLeft, StringComparison.CurrentCultureIgnoreCase);
                        ActiveAlignmentCenter = CursorElementStyle.TextAlignment.Equals(StyleAttribute.alignCenter, StringComparison.CurrentCultureIgnoreCase);
                        ActiveAlignmentRight = CursorElementStyle.TextAlignment.Equals(StyleAttribute.alignRight, StringComparison.CurrentCultureIgnoreCase);
                    }
                    break;
            }
        }

        private async void Webview_NavigationCompleted(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            //ExecuteScript(JsDepot.js_InstallMouseListener);
            //ExecuteScript(JsDepot.js_InstallKeyboardListener);
            ExecuteScript(JsDepot.js_InstallBodyObserver);
            await GetBodyStyle();            
        }

        /////////////////////////////
        #endregion Construction
        ///////////////////////////////////////////////////////////


        ///////////////////////////////////////////////////////////
        #region Interface
        /////////////////////////////

        //
        // Selection
        //
        public async void SetInitialCursorPosition()
        {
            ExecuteScript(JsDepot.js_SetInitialCursorPos);
        }

        public async Task<bool> IsTextSelected()
        {
            bool result = false;

            var json = await ExecuteScriptAsync(JsDepot.js_CheckForTextSelection);
            if (json is not null)
            {
                try
                {
                    string? boolString = json.RootElement.GetString();
                    if (boolString is null)
                    {
                        return false;
                    }
                    result = bool.Parse(boolString);
                }
                catch (Exception ex)
                {
                    Logger.LogException(ex);
                    return false;
                }
            }

            return result;
        }

        public async Task<bool> IsSelectionWithinAnElement()
        {
            bool result = false;

            if (CursorElementType.Equals("body", StringComparison.CurrentCultureIgnoreCase) ||
                CursorElementType.Equals("html", StringComparison.CurrentCultureIgnoreCase))
            {
                return false;
            }

            return result;
        }

        // Explicit resampling of body.innerHTML
        public async Task ResampleDocument()
        {
            //JsonDocument doc = await ExecuteScriptAsync(JsDepot.js_GetBodyInnerHTML);
            ////if(doc.RootElement.GetString() TryGetProperty("innerHTML", out JsonElement jsel))
            //{
            //    string? innerHTML = doc.RootElement.GetString();
            //    if(innerHTML is not null && !string.IsNullOrEmpty(innerHTML))
            //    {
            //        BodyInnerHTML = innerHTML;
            //    }
            //}
        }

        //
        // Styling
        //
        public async void SetFont(FontFamily fontFamily)
        {
            Debug.WriteLine("SetFont");
            string newValue = $@"{fontFamily}";

            bool selection = await IsTextSelected();
            if (selection)
            {
                string id = CursorElementId;
                if (string.IsNullOrEmpty(id))
                {
                    id = NewId();
                    await ExecuteScriptAsync(JsDepot.js_CreateSpanForSelection, [id]);
                    _CursorElementId = id;
                }
                await ExecuteScriptAsync(JsDepot.js_SetElementStyleProperty, [id, StyleAttribute.property_FontFamily, newValue]);
                _CursorElementStyle.Font = fontFamily;
                _ActiveFont = fontFamily;
            }
            else
            {
                //await ExecuteScriptAsync(JsDepot.js_SetBodyStyleProperty, [StyleAttribute.property_FontFamily, newValue]);
            }
            await ResampleDocument();
        }

        public async void SetFontSize(string fontSize)
        {
            Debug.WriteLine("SetFontSize");
            string newValue = fontSize;

            bool selection = await IsTextSelected();
            if (selection)
            {
                string id = CursorElementId;
                if (string.IsNullOrEmpty(id))
                {
                    id = NewId();
                    await ExecuteScriptAsync(JsDepot.js_CreateSpanForSelection, [id]);
                    _CursorElementId = id;
                }
                await ExecuteScriptAsync(JsDepot.js_SetElementStyleProperty, [id, StyleAttribute.property_FontSize, newValue]);
                _CursorElementStyle.FontSize = fontSize;
                _ActiveFontSize = fontSize;
            }
            else
            {
                //await ExecuteScriptAsync(JsDepot.js_SetBodyStyleProperty, [StyleAttribute.property_FontSize, newValue]);
            }
            await ResampleDocument();
        }

        public async void SetFontColor(Color color)
        {
            Debug.WriteLine("SetFontColor");
            string newValue = $@"{color.ToJavascriptRGB()}";

            bool selection = await IsTextSelected();
            if (selection)
            {
                string id = CursorElementId;
                if (string.IsNullOrEmpty(id))
                {
                    id = NewId();
                    await ExecuteScriptAsync(JsDepot.js_CreateSpanForSelection, [id]);
                    _CursorElementId = id;
                }
                await ExecuteScriptAsync(JsDepot.js_SetElementStyleProperty, [id, StyleAttribute.property_FontColor, newValue]);
                _CursorElementStyle.FontColor = color;
                _ActiveFontColor = color;
            }
            else
            {
                //await ExecuteScriptAsync(JsDepot.js_SetBodyStyleProperty, [StyleAttribute.property_FontColor, newValue]);
            }
            await ResampleDocument();
        }

        public async void ToggleBold(bool value)
        {
            Debug.WriteLine("ToggleBold");
            string newValue = value ? StyleAttribute.value_bold : StyleAttribute.value_normal;

            bool selection = await IsTextSelected();
            if(selection)
            {
                string id = CursorElementId;
                if (string.IsNullOrEmpty(id))
                {
                    id = NewId();
                    await ExecuteScriptAsync(JsDepot.js_CreateSpanForSelection, [id]);
                    _CursorElementId = id;
                }
                await ExecuteScriptAsync(JsDepot.js_SetElementStyleProperty, [id, StyleAttribute.property_FontWeight, newValue]);
                _CursorElementStyle.FontWeight = newValue;
                _ActiveFontBold = value;
            }
            else
            {
                //await ExecuteScriptAsync(JsDepot.js_SetBodyStyleProperty, [StyleAttribute.property_FontWeight, newValue]);
            }
            await ResampleDocument();
        }

        public async void ToggleItalic(bool value)
        {
            Debug.WriteLine("ToggleItalic");
            string newValue = value ? StyleAttribute.value_italic : StyleAttribute.value_normal;

            bool selection = await IsTextSelected();
            if (selection)
            {
                string id = CursorElementId;
                if (string.IsNullOrEmpty(id))
                {
                    id = NewId();
                    await ExecuteScriptAsync(JsDepot.js_CreateSpanForSelection, [id]);
                    _CursorElementId = id;
                }
                await ExecuteScriptAsync(JsDepot.js_SetElementStyleProperty, [id, StyleAttribute.property_FontStyle, newValue]);
                _CursorElementStyle.FontStyle = newValue;
                _ActiveFontItalic = value;
            }
            else
            {
                //await ExecuteScriptAsync(JsDepot.js_SetBodyStyleProperty, [StyleAttribute.property_FontStyle, newValue]);
            }
            await ResampleDocument();
        }

        public async void ToggleUnderline(bool value)
        {
            Debug.WriteLine("ToggleUnderline");
            string newValue = value ? StyleAttribute.value_underline : StyleAttribute.value_none;

            bool selection = await IsTextSelected();
            if (selection)
            {
                string id = CursorElementId;
                if (string.IsNullOrEmpty(id))
                {
                    id = NewId();
                    await ExecuteScriptAsync(JsDepot.js_CreateSpanForSelection, [id]);
                    _CursorElementId = id;
                }
                await ExecuteScriptAsync(JsDepot.js_SetElementStyleProperty, [id, StyleAttribute.property_TextDecoration, newValue]);
                _CursorElementStyle.TextDecoration = newValue;
                _ActiveFontUnderline = value;
            }
            else
            {
                //await ExecuteScriptAsync(JsDepot.js_SetBodyStyleProperty, [StyleAttribute.property_TextDecoration, newValue]);
            }
            await ResampleDocument();
        }

        public async void ApplyTextAlignment(string textAlignment)
        {
            Debug.WriteLine("ApplyTextAlignment");
            bool selection = await IsTextSelected();
            if (selection)
            {
                string id = CursorElementId;
                if (string.IsNullOrEmpty(id))
                {
                    id = NewId();
                    await ExecuteScriptAsync(JsDepot.js_CreateDivForSelection, [id]);
                    _CursorElementId = id;
                }
                await ExecuteScriptAsync(JsDepot.js_SetElementStyleProperty, [id, StyleAttribute.property_TextAlign, textAlignment]);
                _CursorElementStyle.TextAlignment = textAlignment;
                _ActiveAlignmentLeft = textAlignment.Equals(StyleAttribute.alignLeft, StringComparison.CurrentCultureIgnoreCase);
                _ActiveAlignmentCenter = textAlignment.Equals(StyleAttribute.alignCenter, StringComparison.CurrentCultureIgnoreCase);
                _ActiveAlignmentRight = textAlignment.Equals(StyleAttribute.alignRight, StringComparison.CurrentCultureIgnoreCase);
            }
            await ResampleDocument();
        }

        public async Task GetBodyStyle()
        {
            var json = await ExecuteScriptAsync(JsDepot.js_GetBodyAttribute, [token_StyleAttribute]);
            if (json != null)
            {
                try
                {
                    string? value = json.RootElement.GetString();
                    if(value is not null)
                    {
                        BodyStyle.Value = value;
                    }
                }
                catch(Exception ex)
                {
                    Logger.LogException(ex);
                }
            }
            await ResampleDocument();
        }

        public async void SetBodyStyle()
        {
            ExecuteScript(JsDepot.js_SetBodyAttribute, [token_StyleAttribute, BodyStyle.Value]);
            await ResampleDocument();
        }

        public async void SetBodyStyleProperty(string property, string value)
        {
            ExecuteScript(JsDepot.js_SetBodyStyleProperty, [property, value]);
            await ResampleDocument();
        }

        public async void InsertElement(string tag, string outerHTML)
        {
            await ExecuteScriptAsync(JsDepot.js_InsertElement, [tag, outerHTML]);
            await ResampleDocument();
        }

        public async void InsertInlineImage(EmbeddedImage img)
        {
            ExecuteScript(JsDepot.js_InsertInlineImage, [img.Alt, img.HTMLSource]);
            await ResampleDocument();
        }

        public async void SetElementInnerHTML(string elementID, string html)
        {
            ExecuteScript(JsDepot.js_SetElementInnerHTML, [elementID, html]);
            await ResampleDocument();
        }

        /////////////////////////////
        #endregion Interface
        ///////////////////////////////////////////////////////////


        ///////////////////////////////////////////////////////////
        #region Private impl
        /////////////////////////////

        private static string NewId()
        {
            return Guid.NewGuid().ToString();
        }

        public void ExecuteScript(string script, string[]? parameters = null)
        {
            try
            {
                if (parameters is not null)
                {
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        string token = $"@PARAM{i}";
                        script = script.Replace(token, parameters[i]);
                    }
                }
                Webview.ExecuteScriptAsync(script);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        public async Task<JsonDocument> ExecuteScriptAsync(string script, string[]? parameters = null)
        {
            string result = string.Empty;
            try
            {
                if (parameters is not null)
                {
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        string token = $"@PARAM{i}";
                        script = script.Replace(token, parameters[i]);
                    }
                }
                result = await Webview.ExecuteScriptAsync(script);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return JsonDocument.Parse(result);
        }

        /////////////////////////////
        #endregion Private impl
        ///////////////////////////////////////////////////////////


        ///////////////////////////////////////////////////////////
        #region Web-message handling
        /////////////////////////////

        private void Webview_WebMessageReceived(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs e)
        {
            var message = JsonDocument.Parse(e.WebMessageAsJson);

            try
            {
                bool success = message.RootElement.TryGetProperty(msgid, out JsonElement value);
                if (success)
                {
                    string? messageID = value.GetString();
                    if (messageID is not null)
                    {
                        // On cursor position changed, update this document with:
                        // current element at cursor pos
                        if (messageID.Equals(msgid_MouseClick, StringComparison.CurrentCultureIgnoreCase))
                        {
                            ProcessMouseClick(message);
                        }

                        // On keyDown, update this document with:
                        // current element at cursor pos
                        if (messageID.Equals(msgid_KeyDown, StringComparison.CurrentCultureIgnoreCase))
                        {
                            ProcessKeyDown(message);
                        }

                        // Update this document's Body property with:
                        // innerHtml of html body
                        if(messageID.Equals(msgid_BodyChanged, StringComparison.CurrentCultureIgnoreCase))
                        {
                            ProcessBodyChanged(message);
                        }

                        // selectionchange event
                        if(messageID.Equals(msgid_SelectionChange, StringComparison.CurrentCultureIgnoreCase))
                        {
                            ProcessSelectionChange(message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        private void ProcessMouseClick(JsonDocument message)
        {
            try
            {
                string? tagName = message.RootElement.GetProperty(token_tagName).GetString();
                if (tagName is not null)
                {
                    CursorElementType = tagName;
                }

                string? id = message.RootElement.GetProperty(token_id).GetString();
                if (id is not null)
                {
                    CursorElementId = id;
                }

                string? outerHTML = message.RootElement.GetProperty(token_outerHTML).GetString();
                if (outerHTML is not null)
                {
                    CursorElementOuterHTML = outerHTML;
                }

                string? innerHTML = message.RootElement.GetProperty(token_innerHTML).GetString();
                if (innerHTML is not null)
                {
                    CursorElementInnerHTML = innerHTML;
                }

                string? style = message.RootElement.GetProperty(token_StyleAttribute).GetString();
                if (style is not null)
                {
                    CursorElementStyle.Value = style;
                }
                else
                {
                    CursorElementStyle.Value = string.Empty;
                }

                if (message.RootElement.TryGetProperty("x", out JsonElement x))
                {
                    if (x.TryGetInt32(out int newx))
                    {
                        Point point = BrowserCursorPosition;
                        point.X = newx;
                        BrowserCursorPosition = point;
                    }
                }
                if (message.RootElement.TryGetProperty("y", out JsonElement y))
                {
                    if (y.TryGetInt32(out int newy))
                    {
                        Point point = BrowserCursorPosition;
                        point.Y = newy;
                        BrowserCursorPosition = point;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        private void ProcessKeyDown(JsonDocument message)
        {
            try
            {
                string? tagName = message.RootElement.GetProperty(token_tagName).GetString();
                if (tagName is not null)
                {
                    CursorElementType = tagName;
                }

                string? id = message.RootElement.GetProperty(token_id).GetString();
                if (id is not null)
                {
                    CursorElementId = id;
                }

                string? outerHTML = message.RootElement.GetProperty(token_outerHTML).GetString();
                if (outerHTML is not null)
                {
                    CursorElementOuterHTML = outerHTML;
                }

                string? innerHTML = message.RootElement.GetProperty(token_innerHTML).GetString();
                if (innerHTML is not null)
                {
                    CursorElementInnerHTML = innerHTML;
                }

                string? style = message.RootElement.GetProperty(token_StyleAttribute).GetString();
                if (style is not null)
                {
                    CursorElementStyle.Value = style;
                }
                else
                {
                    CursorElementStyle.Value = string.Empty;
                }

                if (message.RootElement.TryGetProperty("x", out JsonElement x))
                {
                    if (x.TryGetInt32(out int newx))
                    {
                        Point point = BrowserCursorPosition;
                        point.X = newx;
                        BrowserCursorPosition = point;
                    }
                }
                if (message.RootElement.TryGetProperty("y", out JsonElement y))
                {
                    if (y.TryGetInt32(out int newy))
                    {
                        Point point = BrowserCursorPosition;
                        point.Y = newy;
                        BrowserCursorPosition = point;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        private void ProcessBodyChanged(JsonDocument message)
        {
            try
            {
                string? outerHTML = message.RootElement.GetProperty(token_outerHTML).GetString();
                if (outerHTML is not null)
                {
                    BodyOuterHTML = outerHTML;
                    //OnPropertyChanged(nameof(BodyOuterHTML));
                }

                string? innerHTML = message.RootElement.GetProperty(token_innerHTML).GetString();
                if(innerHTML is not null)
                {
                    BodyInnerHTML = innerHTML;
                    //OnPropertyChanged(nameof(BodyInnerHTML));
                }

                string? style = message.RootElement.GetProperty(token_StyleAttribute).GetString();
                if(style is not null)
                {
                    BodyStyle.Value = style;
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        private void ProcessSelectionChange(JsonDocument message)
        {
            try
            {
                _CursorElementId = string.Empty;
                _CursorElementStyle.Value = string.Empty;


                string? tagName = message.RootElement.GetProperty(token_tagName).GetString();
                if (tagName is not null)
                {
                    CursorElementType = tagName;
                }

                string? id = message.RootElement.GetProperty(token_id).GetString();
                if (id is not null)
                {
                    CursorElementId = id;
                }

                string? outerHTML = message.RootElement.GetProperty(token_outerHTML).GetString();
                if (outerHTML is not null)
                {
                    CursorElementOuterHTML = outerHTML;
                }

                string? innerHTML = message.RootElement.GetProperty(token_innerHTML).GetString();
                if (innerHTML is not null)
                {
                    CursorElementInnerHTML = innerHTML;
                }

                string? style = message.RootElement.GetProperty(token_StyleAttribute).GetString();
                if (style is not null)
                {
                    CursorElementStyle.Value = style;
                }

                // Cursor pos is a pair of values
                Point point = _BrowserCursorPosition;
                JsonElement position = message.RootElement.GetProperty("position");
                point.X = position.GetProperty("x").GetDouble();
                point.Y = position.GetProperty("y").GetDouble();
                BrowserCursorPosition = point;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        /////////////////////////////
        #endregion Web-message handling
        ///////////////////////////////////////////////////////////
    }
}
