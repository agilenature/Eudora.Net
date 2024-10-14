using Eudora.Net.Core;
using Microsoft.Win32;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Eudora.Net.Data;
using Microsoft.Web.WebView2.Wpf;
using Eudora.Net.Javascript;
using Eudora.Net.ExtensionMethods;
using System.Windows.Input;
using System.Text.Encodings.Web;
using System.Reflection.Metadata;

namespace Eudora.Net.GUI
{
    /// <summary>
    /// EmailView: A container for the following:
    /// - Viewing & editing email, either HTML or Plain Text formats
    /// - Email commands that do not depend on format (send, etc.)
    /// </summary>
    public partial class uc_EmailView : ChildWindowBase
    {
        private EmailMessage Message = new();
        private bool IsNewOutgoing = false;


        ///////////////////////////////////////////////////////////
        #region Construction
        /////////////////////////////

        public uc_EmailView()
        {
            CommonCtor();
        }

        public uc_EmailView(EmailMessage message)
        {
            CommonCtor();
            DataContext = message;
            UpdateDataContext();
        }

        public uc_EmailView(EmailMessage message, bool newOutgoing = false)
        {
            IsNewOutgoing = newOutgoing;
            CommonCtor();
            DataContext = message;
            UpdateDataContext();
        }

        private void CommonCtor()
        {
            InitializeComponent();
            BuildComboBoxes();
            DataContextChanged += Uc_EmailView_DataContextChanged;
            Editor.Document.PropertyChanged += Document_PropertyChanged;
            Editor.Webview.NavigationCompleted += Webview_NavigationCompleted;
            UpdateTitle();

            Editor.btn_InsertImage.Click += Btn_InsertImage_Click;
            Editor.btn_InsertAttachment.Click += Btn_InsertAttachment_Click;
        }

        private void Toolbar_Loaded(object sender, RoutedEventArgs e)
        {
            if(sender is not ToolBar toolBar) return;

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

        private void Btn_InsertAttachment_Click(object sender, RoutedEventArgs e)
        {
            EmailMessage? message = DataContext as EmailMessage;
            if (message is null)
            {
                Logger.Debug("An EmailMessage was not set as the DataContext");
                return;
            }

            try
            {
                OpenFileDialog ofd = new();
                ofd.Filter = "All files (*.*)|*.*";
                ofd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                ofd.ValidateNames = true;
                ofd.CheckFileExists = true;
                ofd.CheckPathExists = true;
                ofd.Multiselect = false;
                var result = ofd.ShowDialog();
                if (result is null || result == false)
                {
                    return;
                }

                message.Attachments.Add(new EmailAttachment(ofd.SafeFileName, ofd.FileName));
                AttachmentsBar.DataContext = message.Attachments;
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
            }
        }

        private void Btn_InsertImage_Click(object sender, RoutedEventArgs e)
        {
            EmailMessage? message = DataContext as EmailMessage;
            if (message is null)
            {
                Logger.Debug("An EmailMessage was not set as the DataContext");
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
                Editor.Document.InsertInlineImage(img);
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
            }
        }

        private void Webview_NavigationCompleted(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            //Logger.NewEvent(LogEvent.EventCategory.Debug, "Webview_NavigationCompleted");
            bool designMode = (Message.Status == EmailEnums.MessageStatus.Draft);
            Editor.Document.DesignMode = designMode;
            if (designMode)
            {
                //Editor.Document.SetInitialCursorPosition();
                if (IsNewOutgoing)
                {
                    IsNewOutgoing = false;
                    var personality = PersonalityManager.Datastore.Data.Where(p => p.Id == Message.PersonalityID).FirstOrDefault();
                    if (personality is not null)
                    {
                        var signature = SignatureManager.Datastore.Data.Where(s => s.Name == personality.DefaultSignature).FirstOrDefault();
                        if(signature is not null)
                        {
                            cb_Signature.SelectedItem = signature;
                        }

                        var stationery = StationeryManager.Datastore.Data.Where(s => s.Name == personality.DefaultStationery).FirstOrDefault();
                        if (stationery is not null)
                        {
                            cb_Stationery.SelectedItem = stationery;
                        }
                    }
                }
            }

            //Editor.Webview.Focus();
            //Dispatcher.InvokeAsync(() => Editor.Webview.InnerFocus());
        }

        private void BuildComboBoxes()
        {
            cb_Priority.ItemsSource = Enum.GetValues(typeof(PostOffice.eMailPriority));
            cb_Priority.SelectedIndex = 2; // Index 2: Value: Normal

            cb_Signature.ItemsSource = SignatureManager.Datastore.Data;

            cb_Stationery.ItemsSource = StationeryManager.Datastore.Data;
        }

        /////////////////////////////
        #endregion Construction
        ///////////////////////////////////////////////////////////



        ///////////////////////////////////////////////////////////
        #region Events
        /////////////////////////////

        public bool IsEditable()
        {
            return (Message.Status == EmailEnums.MessageStatus.Draft);
        }

        public override void MdiActivated()
        {
            base.MdiActivated();

            if (Message is not null)
            {
                if (Message.Status == EmailEnums.MessageStatus.Draft)
                {
                    EudoraStatistics.UserIsWritingEmail(true);
                }
                else
                {
                    EudoraStatistics.UserIsReadingEmail(true);
                }
            }

            var wnd = MainWindow.Instance;
            if(wnd is null) return;
            wnd.btn_Reply.Click += Btn_Reply_Click;
            wnd.btn_ReplyAll.Click += Btn_ReplyAll_Click;
            wnd.btn_Forward.Click += Btn_Forward_Click;

            wnd.Menu_Edit.IsEnabled = true;
            wnd.Menu_Edit.EnableAllSubitems();
            wnd.Menu_Edit_Cut.Click += Cut;
            wnd.Menu_Edit_Copy.Click += Copy;
            wnd.Menu_Edit_Paste.Click += Paste;
            wnd.Menu_Edit_Undo.Click += Undo;
            wnd.Menu_Edit_Redo.Click += Redo;
            wnd.Menu_Edit_Clear.Click += Delete;

            // Print support
            wnd.Menu_File_Print.IsEnabled = true;
            wnd.Menu_File_Print.Click += Menu_File_Print_Click;            

            if (Editor is not null)
            {
                Editor.Visibility = Visibility.Visible;
                //Editor.Focus();
            }
        }

        public override void MdiDeactivated()
        {
            base.MdiDeactivated();

            if (Message is not null)
            {
                if (Message.Status == EmailEnums.MessageStatus.Draft)
                {
                    EudoraStatistics.UserIsWritingEmail(false);
                }
                else
                {
                    EudoraStatistics.UserIsReadingEmail(false);
                }
            }

            var wnd = MainWindow.Instance;
            if (wnd is null) return;

            wnd.btn_Reply.Click -= Btn_Reply_Click;
            wnd.btn_ReplyAll.Click -= Btn_ReplyAll_Click;
            wnd.btn_Forward.Click -= Btn_Forward_Click;
            wnd.Menu_File_Print.Click -= Menu_File_Print_Click;
                
            wnd.Menu_Edit_Cut.Click -= Cut;
            wnd.Menu_Edit_Copy.Click -= Copy;
            wnd.Menu_Edit_Paste.Click -= Paste;
            wnd.Menu_Edit_Undo.Click -= Undo;
            wnd.Menu_Edit_Redo.Click -= Redo;
            wnd.Menu_Edit_Clear.Click -= Delete;

            // Print support
            wnd.Menu_File_Print.IsEnabled = false;
            wnd.Menu_File_Print.Click -= Menu_File_Print_Click;

            if (Editor is not null)
            {
                Editor.Visibility = Visibility.Hidden;
            }
        }

        public void Cut(object sender, RoutedEventArgs e)
        {
            Editor.Webview.CoreWebView2.ExecuteScriptAsync("window.getSelection().toString();").ContinueWith((Task<string> task) =>
            {
                if (task.IsCompletedSuccessfully)
                {
                    string result = task.Result;
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Clipboard.SetText(result);
                        Editor.Webview.CoreWebView2.ExecuteScriptAsync("document.execCommand('Delete');");
                    });                    
                }
            });
        }

        public void Copy(object sender, RoutedEventArgs e)
        {
            Editor.Webview.CoreWebView2.ExecuteScriptAsync("window.getSelection().toString();").ContinueWith((Task<string> task) =>
            {
                if (task.IsCompletedSuccessfully)
                {
                    string result = task.Result;
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Clipboard.SetText(result);
                    });
                }
            });
        }

        public void Paste(object sender, RoutedEventArgs e)
        {
            string text = string.Empty;
            Application.Current.Dispatcher.Invoke(() =>
            {
                text = Clipboard.GetText();
            });
            Editor.Webview.ExecuteScriptAsync($"document.execCommand('insertText', false, {text});");
        }

        public void SelectAll(object sender, RoutedEventArgs e)
        {
            Editor.Webview.ExecuteScriptAsync("document.execCommand('SelectAll');");
        }

        public void Undo(object sender, RoutedEventArgs e)
        {
            Editor.Webview.ExecuteScriptAsync("document.execCommand('Undo');");
        }

        public void Redo(object sender, RoutedEventArgs e)
        {
            Editor.Webview.ExecuteScriptAsync("document.execCommand('Redo');");
        }

        public void Delete(object sender, RoutedEventArgs e)
        {
            Editor.Webview.ExecuteScriptAsync("document.execCommand('Delete');");
        }

        private void Menu_File_Print_Click(object sender, RoutedEventArgs e)
        {
            Editor.Print();
        }

        private void Document_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.PropertyName)) return;

            if (e.PropertyName.Equals(nameof(Editor.Document.BodyInnerHTML), StringComparison.CurrentCultureIgnoreCase) ||
                e.PropertyName.Equals(nameof(Editor.Document.BodyOuterHTML), StringComparison.CurrentCultureIgnoreCase))
            {
                Message.Body = Editor.Document.BodyOuterHTML;
            }
        }

        private void Editor_DocumentLoaded(object? sender, EventArgs e)
        {
            bool messageIsEditable = (Message.Status == EmailEnums.MessageStatus.Draft);

            if (messageIsEditable)
            {
                Editor.Document.DesignMode = true;
            }            
        }

        private void Uc_EmailView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var message = DataContext as EmailMessage;
            if (message is not null)
            {
                Message = message;
                string tempPath = TempFileManager.CreateHtmlFileFromStringContent(message.Body);
                if (string.IsNullOrEmpty(tempPath))
                {
                    Logger.Debug("Failed to create temp file from message body");
                    return;
                }
                Editor.Navigate(tempPath);

                bool messageIsEditable = (Message.Status == EmailEnums.MessageStatus.Draft);
                MainToolbar.IsEnabled = messageIsEditable;
                HeaderPanel.IsEnabled = messageIsEditable;
                Editor.ToolbarsVisible = messageIsEditable;
                AttachmentsBar.IsEnabled = true;
                AttachmentsBar.DataContext = Message.Attachments;

                if (messageIsEditable)
                {
                    EudoraStatistics.UserIsWritingEmail(true);
                }
                else
                {
                    EudoraStatistics.UserIsReadingEmail(true);
                }

                // Propagate the new datacontext to the child UserControls
                UpdateDataContext();
            }
        }

        private void ApplyStationery(Stationery stationery)
        {
            Editor.Document.BodyStyle.Value = stationery.Style;
            Editor.Document.SetBodyStyle();
        }

        private async void ApplySignature(Signature signature)
        {
            await Editor.Document.ExecuteScriptAsync(JsDepot.js_SetEmailSignature, [signature.Content]);
        }

        private void Btn_Forward_Click(object sender, RoutedEventArgs e)
        {
            var messageOut = PostOffice.CreateMessage_Forward(Message);
            MainWindow.Instance?.ShowMailMessage(messageOut);
        }

        private void Btn_ReplyAll_Click(object sender, RoutedEventArgs e)
        {
            var messageOut = PostOffice.CreateMessage_ReplyAll(Message);
            MainWindow.Instance?.ShowMailMessage(messageOut);
        }

        private void Btn_Reply_Click(object sender, RoutedEventArgs e)
        {
            var messageOut = PostOffice.CreateMessage_Reply(Message);
            MainWindow.Instance?.ShowMailMessage(messageOut);
        }

        private void tb_Subject_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateTitle();
        }

        private void cb_Signature_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cb_Signature.SelectedItem is Signature signature)
            {
                ApplySignature(signature);
            }
        }

        private void cb_Stationery_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(cb_Stationery.SelectedItem is Stationery stationery)
            {
                ApplyStationery(stationery);
            }
        }

        private void cb_Priority_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Message.Priority = (PostOffice.eMailPriority)cb_Priority.SelectedValue;
        }

        private async void btn_Send_Click(object sender, RoutedEventArgs e)
        {
            await PostOffice.SendMessage(Message);
            Window.Close();
        }

        /////////////////////////////
        #endregion Events
        ///////////////////////////////////////////////////////////


        ///////////////////////////////////////////////////////////
        #region Internal
        /////////////////////////////

        private async void LoadAndApplyStationery(Stationery stationery)
        {
            
        }


        /////////////////////////////
        #endregion Internal
        ///////////////////////////////////////////////////////////


        ///////////////////////////////////////////////////////////
        #region Utility
        /////////////////////////////

        /// <summary>
        /// Because the total EmailView is a nested series of UserControls,
        /// the DataContext change must be propagated down the tree.
        /// </summary>
        private void UpdateDataContext()
        {
            Editor.DataContext = DataContext;
            AttachmentsBar.DataContext = DataContext;
        }

        /// <summary>
        /// 
        /// </summary>
        private void UpdateTitle()
        {
            Title = Message.Subject;
            if (string.IsNullOrEmpty(Title)) Title = "New Message";
            MainWindow.Instance?.UpdateMessageViewTitle(Message, Title);
        }

        private void MainToolbar_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void ToolbarSend_Loaded(object sender, RoutedEventArgs e)
        {

        }



        /////////////////////////////
        #endregion Utility
        ///////////////////////////////////////////////////////////
    }
}