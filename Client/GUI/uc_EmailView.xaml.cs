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

        private void Btn_InsertAttachment_Click(object sender, RoutedEventArgs e)
        {
            EmailMessage? message = DataContext as EmailMessage;
            if (message is null)
            {
                Logger.NewEvent(LogEvent.EventCategory.Warning, "An EmailMessage was not set as the DataContext");
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
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        private void Btn_InsertImage_Click(object sender, RoutedEventArgs e)
        {
            EmailMessage? message = DataContext as EmailMessage;
            if (message is null)
            {
                Logger.NewEvent(LogEvent.EventCategory.Warning, "An EmailMessage was not set as the DataContext");
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
                Logger.LogException(ex);
            }
        }

        private void Webview_NavigationCompleted(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            //Logger.NewEvent(LogEvent.EventCategory.Debug, "Webview_NavigationCompleted");
            bool designMode = (Message.Status == EmailMessage.MessageStatus.Draft);
            Editor.Document.DesignMode = designMode;
            if (designMode)
            {
                //Editor.Document.SetInitialCursorPosition();
            }

            //Editor.Webview.Focus();
            //Dispatcher.InvokeAsync(() => Editor.Webview.InnerFocus());
        }

        private void BuildComboBoxes()
        {
            cb_Priority.ItemsSource = Enum.GetValues(typeof(PostOffice.eMailPriority));
            cb_Priority.SelectedIndex = 2; // Index 2: Value: Normal

            cb_Signature.ItemsSource = SignatureManager.Collection;

            cb_Stationery.ItemsSource = StationeryManager.Collection;
        }

        /////////////////////////////
        #endregion Construction
        ///////////////////////////////////////////////////////////



        ///////////////////////////////////////////////////////////
        #region Events
        /////////////////////////////

        public bool IsEditable()
        {
            return (Message.Status == EmailMessage.MessageStatus.Draft);
        }

        public override void MdiActivated()
        {
            base.MdiActivated();

            if (Message is not null)
            {
                if (Message.Status == EmailMessage.MessageStatus.Draft)
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
            wnd.btn_Attach.Click += Btn_Attach_Click;
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
                if (Message.Status == EmailMessage.MessageStatus.Draft)
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

            wnd.btn_Attach.Click -= Btn_Attach_Click;
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
            bool messageIsEditable = (Message.Status == EmailMessage.MessageStatus.Draft);

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
                string tempPath = TempFileManager.CreateTempFileFromStringContent(message.Body);
                if (string.IsNullOrEmpty(tempPath))
                {
                    Logger.NewEvent(LogEvent.EventCategory.Warning, "Failed to create temp file from message body");
                    return;
                }
                Editor.Navigate(tempPath);

                bool messageIsEditable = (Message.Status == EmailMessage.MessageStatus.Draft);
                MainToolbar.IsEnabled = messageIsEditable;
                ToolbarSend.IsEnabled = messageIsEditable;
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
            Editor.Document.BodyStyle.Value = stationery.Style.Value;
            Editor.Document.SetBodyStyle();
        }

        private async void ApplySignature(Signature signature)
        {
            await Editor.Document.ExecuteScriptAsync(JsDepot.js_SetEmailSignature, [signature.Content]);
        }

        private void Btn_Forward_Click(object sender, RoutedEventArgs e)
        {
            var messageOut = PostOffice.Instance.CreateMessage_Forward(Message);
            MainWindow.Instance?.ShowMailMessage(messageOut);
        }

        private void Btn_ReplyAll_Click(object sender, RoutedEventArgs e)
        {
            var messageOut = PostOffice.Instance.CreateMessage_ReplyAll(Message);
            MainWindow.Instance?.ShowMailMessage(messageOut);
        }

        private void Btn_Reply_Click(object sender, RoutedEventArgs e)
        {
            var messageOut = PostOffice.Instance.CreateMessage_Reply(Message);
            MainWindow.Instance?.ShowMailMessage(messageOut);
        }

        private void Btn_Attach_Click(object sender, RoutedEventArgs e)
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

            Message.Attachments.Add(new EmailAttachment(ofd.SafeFileName, ofd.FileName));
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

        private void btn_Send_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() => PostOffice.Instance.SendMessage(Message));
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

        

        /////////////////////////////
        #endregion Utility
        ///////////////////////////////////////////////////////////
    }
}