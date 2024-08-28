using System.Windows;
using Eudora.Net.Core;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;
using Eudora.Net.Data;
using Microsoft.Win32;
using System.Net.Mail;
using Microsoft.Web.WebView2.Wpf;
using System.Xml.Linq;
using Eudora.Net.Javascript;
using Eudora.Net.ExtensionMethods;
using System.Diagnostics;
using sbux.wpf.Themer;

namespace Eudora.Net.GUI
{

    public partial class uc_HtmlEditor : UserControl
    {
        ///////////////////////////////////////////////////////////
        #region Events
        /////////////////////////////

        public event EventHandler? EditorIsReady;
        private void RaiseEditorReadyEvent()
        {
            EditorIsReady?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler? DocumentLoaded;
        private void RaiseDocumentLoadedEvent()
        {
            DocumentLoaded?.Invoke(this, EventArgs.Empty);
        }

        /////////////////////////////
        #endregion Events
        ///////////////////////////////////////////////////////////



        ///////////////////////////////////////////////////////////
        #region Fields
        /////////////////////////////

        private bool _ToolbarsVisible = true;
        private bool _ToolbarFontEnabled = true;
        private bool _ToolbarFontStyleEnabled = true;
        private bool _ToolbarAlignmentEnabled = true;
        private bool _ToolbarBackgroundEnabled = true;
        private bool _ToolbarEmbedEnabled = true;

        /////////////////////////////
        #endregion Fields
        ///////////////////////////////////////////////////////////



        ///////////////////////////////////////////////////////////
        #region Properties
        /////////////////////////////

        public Webview2Document Document { get; private set; }
        public WebView2 Webview { get; private set; }

        public bool ToolbarsVisible
        {
            get => _ToolbarsVisible;
            set
            {
                _ToolbarsVisible = value;
                Toolbars.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                if(!value)
                {
                    RowToolbars.Height = new GridLength(0);
                }
            }
        }


        public bool ToolbarFontEnabled
        {
            get => _ToolbarFontEnabled;
            set
            {
                _ToolbarFontEnabled = value;
                ToolbarFont.IsEnabled = value;
                ToolbarFont.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            }    
            
        }
        
        public bool ToolbarFontStyleEnabled
        {
            get => _ToolbarFontStyleEnabled;
            set
            {
                _ToolbarFontStyleEnabled = value;
                ToolbarTextStyle.IsEnabled = value;
                ToolbarTextStyle.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public bool ToolbarAlignmentEnabled
        {
            get => _ToolbarAlignmentEnabled;
            set
            {
                _ToolbarAlignmentEnabled = value;
                ToolbarAlignment.IsEnabled = value;
                ToolbarAlignment.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public bool ToolbarBackgroundEnabled
        {
            get => _ToolbarBackgroundEnabled;
            set
            {
                _ToolbarBackgroundEnabled = value;
                ToolbarBackground.IsEnabled = value;
                ToolbarBackground.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public bool ToolbarEmbedEnabled
        {
            get => _ToolbarEmbedEnabled;
            set
            {
                _ToolbarEmbedEnabled = value;
                ToolbarEmbedInsert.IsEnabled = value;
                ToolbarEmbedInsert.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        /////////////////////////////
        #endregion Properties
        ///////////////////////////////////////////////////////////

        public uc_HtmlEditor()
        {
            Webview = Webview2Allocator.Get();
            ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;
            ThemeManager_ThemeChanged(null, null);
            Document = new(Webview);
            DataContext = Document;

            //Document.CursorElementStyle.PropertyChanged += CursorElementStyle_PropertyChanged;
           // Document.PropertyChanged += CursorElementStyle_PropertyChanged;

            InitializeComponent();

            MainGrid.Children.Add(Webview);
            Grid.SetRow(Webview, 1);
            //Webview.Margin = new Thickness(2);
            Webview.Focusable = true;

            Webview.CoreWebView2.NavigationStarting += Webview_NavigationStarting;
            Webview.CoreWebView2.NavigationCompleted += Webview_NavigationCompleted;
            Webview.CoreWebView2.DocumentTitleChanged += CoreWebView2_DocumentTitleChanged;
            Webview.CoreWebView2.ServerCertificateErrorDetected += CoreWebView2_ServerCertificateErrorDetected;
            Webview.CoreWebView2.StatusBarTextChanged += CoreWebView2_StatusBarTextChanged;
            Webview.CoreWebView2.DOMContentLoaded += CoreWebView2_DOMContentLoaded;
            RaiseEditorReadyEvent();

            //cb_Font.DataContext = this;
            cb_Font.ItemsSource = Fonts.SystemFontFamilies;
            //cb_Font.SelectedItem = Document.ActiveFont;
            
            //cb_FontSize.DataContext = this;
            cb_FontSize.ItemsSource = StyleAttribute.FontSizes;
            //cb_FontSize.SelectedItem = Document.ActiveFontSize;

            //cb_BackgroundRepeat.DataContext = StyleAttribute.BackgroundRepeats;
            //cb_BackgroundRepeat.SelectedIndex = 0;

            debugGrid.DataContext = Document;

            DataContextChanged += Uc_HtmlEditor_DataContextChanged;

#if !DEBUG
            debugGrid.Visibility = Visibility.Collapsed;
            RowDebug.Height = new GridLength(0);
#endif
        }

        private void ThemeManager_ThemeChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            //var dynamicResourceExtension = new DynamicResourceExtension("ForegroundNormalBrushKey");
            //var value = dynamicResourceExtension.ProvideValue(null);
            //Style style = new();
            //style.Setters.Add(new Setter(ForegroundProperty, dynamicResourceExtension));
            //style.Setters.Add(new Setter(fore, dynamicResourceExtension));
            //style.Setters.Add(new Setter(BackgroundProperty, new SolidColorBrush(Colors.Orange)));
            //Webview.Style = style;
        }

        private void Toolbar_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is not ToolBar toolBar) return;

            var overflowGrid = toolBar.Template.FindName("OverflowGrid", toolBar) as FrameworkElement;
            if (overflowGrid != null)
            {
                overflowGrid.Visibility = Visibility.Collapsed;
            }
            var mainPanelBorder = toolBar.Template.FindName("MainPanelBorder", toolBar) as FrameworkElement;
            if (mainPanelBorder != null)
            {
                mainPanelBorder.Margin = new Thickness();
            }
        }

        private void Uc_HtmlEditor_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Toolbars.DataContext = Document;
            DataContext = Document;
        }

        private void CursorElementStyle_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            //// This has to run on the GUI thread
            //if (!App.Current.Dispatcher.CheckAccess())
            //{
            //    App.Current.Dispatcher.BeginInvoke(new Action(() => CursorElementStyle_PropertyChanged(sender, e)));
            //    return;
            //}

            //StyleAttribute style = Document.CursorElementStyle;

            ////cb_Font.SelectedItem = style.Font;
            ////cb_FontSize.SelectedItem = style.FontSize;
            ////cp_Font.SelectedColor = style.FontColor;
            
            //btn_Bold.IsChecked = style.FontWeight.Equals(StyleAttribute.value_bold, StringComparison.CurrentCultureIgnoreCase);
            //btn_Italic.IsChecked = style.FontStyle.Equals(StyleAttribute.value_italic, StringComparison.CurrentCultureIgnoreCase);
            //btn_Underlined.IsChecked = style.TextDecoration.Equals(StyleAttribute.value_underline, StringComparison.CurrentCultureIgnoreCase);

            //btn_AlignLeft.IsChecked = style.TextAlignment.Equals(StyleAttribute.alignLeft, StringComparison.CurrentCultureIgnoreCase);
            //btn_AlignCenter.IsChecked = style.TextAlignment.Equals(StyleAttribute.alignCenter, StringComparison.CurrentCultureIgnoreCase);
            //btn_AlignRight.IsChecked = style.TextAlignment.Equals(StyleAttribute.alignRight, StringComparison.CurrentCultureIgnoreCase);
        }

        private void Uc_HtmlEditor_GotFocus(object sender, RoutedEventArgs e)
        {
            //Webview.Focus();
            //Dispatcher.InvokeAsync(() => Webview.InnerFocus());
        }

        public void Print()
        {
            try
            {
                Webview.CoreWebView2.ShowPrintUI(Microsoft.Web.WebView2.Core.CoreWebView2PrintDialogKind.System);
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
            }
        }

        public void Navigate(string url)
        {
            try
            {
                Webview.CoreWebView2.Navigate(url);
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
            }
        }

        public void NavigateToString(string url)
        {
            try
            {
                Webview.CoreWebView2.NavigateToString(url);
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
            }
        }

        private bool IsControlKeyDown()
        {
            return (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl));
        }

        private void UserControl_PreviewKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // toggle BOLD
            switch (e.Key)
            {
                // toggle BOLD
                case Key.B:
                    if (IsControlKeyDown())
                    {
                        btn_Bold.IsChecked = !btn_Bold.IsChecked;
                    }
                    break;

                // toggle ITALIC
                case Key.I:
                    if (IsControlKeyDown())
                    {
                        btn_Italic.IsChecked = !btn_Italic.IsChecked;
                    }
                    break;

                // toggle UNDERLINE
                case Key.U:
                    if (IsControlKeyDown())
                    {
                        btn_Underlined.IsChecked = !btn_Underlined.IsChecked;
                    }
                    break;
            }
        }

        ///////////////////////////////////////////////////////////
        #region WebView2 Handlers
        /////////////////////////////

        private void CoreWebView2_DOMContentLoaded(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2DOMContentLoadedEventArgs e)
        {
        }

        private void CoreWebView2_StatusBarTextChanged(object? sender, object e)
        {

        }

        private void CoreWebView2_ServerCertificateErrorDetected(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2ServerCertificateErrorDetectedEventArgs e)
        {

        }

        private void CoreWebView2_DocumentTitleChanged(object? sender, object e)
        {

        }

        private void Webview_NavigationCompleted(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            RaiseDocumentLoadedEvent();
        }

        private void Webview_NavigationStarting(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs e)
        {
        }

        /////////////////////////////
        #endregion WebView2 Handlers
        ///////////////////////////////////////////////////////////
        
        

        ///////////////////////////////////////////////////////////
        #region Document toolbar
        /////////////////////////////

        private void cp_Background_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (cp_Background.SelectedColor is Color color)
            {
                Document.SetBodyStyleProperty("background-color", color.ToJavascriptRGB());
            }
        }

        private async void btn_BackgroundImage_Click(object sender, RoutedEventArgs e)
        {
            EmailMessage? message = DataContext as EmailMessage;
            if (message is null)
            {
                Logger.Warning("An EmailMessage was not set as the DataContext");
                return;
            }

            try
            {
                OpenFileDialog ofd = new();
                ofd.Filter = GHelpers.MakeImageFilterString();
                ofd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                var result = ofd.ShowDialog();
                ofd.ValidateNames = true;
                ofd.CheckFileExists = true;
                ofd.CheckPathExists = true;
                ofd.Multiselect = false;
                if (result is null || result == false)
                {
                    return;
                }

                EmbeddedImage img = new()
                {
                    Alt = ofd.SafeFileName,
                    Source = ofd.FileName,
                    CIDSource = $@"cid:{ofd.SafeFileName}",
                    HTMLSource = ofd.FileName.Replace("\\", "/")
                };
                message.InlineImages.Add(img);
                //Document.SetBodyStyleProperty("background-image", img.HTMLSource);
                var res = await Webview.ExecuteScriptAsync($@"document.body.background = '{img.HTMLSource}'");
                Debug.WriteLine(res);
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
            }
        }

        private async void cb_BackgroundRepeat_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (cb_BackgroundRepeat.SelectedItem is string repeat)
            //{
            //    var res = await Webview.ExecuteScriptAsync($@"document.body.style.backgroundRepeat = '{repeat}'");
            //    Debug.WriteLine(res);
            //}
        }

        /////////////////////////////
        #endregion Document toolbar
        ///////////////////////////////////////////////////////////



        ///////////////////////////////////////////////////////////
        #region Email Toolbar
        /////////////////////////////

        private void btn_InsertImage_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void btn_InsertAttachment_Click(object sender, RoutedEventArgs e)
        {
            
        }

        /////////////////////////////
        #endregion Email Toolbar
        ///////////////////////////////////////////////////////////
    }
    
}
