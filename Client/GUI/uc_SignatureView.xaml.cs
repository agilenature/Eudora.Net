using Eudora.Net.Core;
using Eudora.Net.Data;
using Eudora.Net.ExtensionMethods;
using System.Windows;

namespace Eudora.Net.GUI
{
    /// <summary>
    /// Interaction logic for uc_SignatureView.xaml
    /// </summary>
    public partial class uc_SignatureView : ChildWindowBase
    {
        public uc_SignatureView(Signature signature)
        {
            InitializeComponent();
            DataContextChanged += Uc_SignatureView_DataContextChanged;
            DataContext = signature;
            //Editor.Document.PropertyChanged += Document_PropertyChanged;
            Editor.EditorIsReady += Editor_EditorIsReady;
            Editor.DocumentLoaded += Editor_DocumentLoaded;
            Editor.ToolbarEmbedEnabled = false;
            Editor.ToolbarBackgroundEnabled = false;
        }

        public override void MdiActivated()
        {
            base.MdiActivated();
            if (Editor is not null)
            {
                Editor.Visibility = System.Windows.Visibility.Visible;
            }

            if (MainWindow.Instance != null)
            {
                MainWindow.Instance.Menu_File_Print.Click += Menu_File_Print_Click;

                MainWindow.Instance.Menu_Edit.IsEnabled = true;
                MainWindow.Instance.Menu_Edit.EnableAllSubitems();
                MainWindow.Instance.Menu_Edit_Cut.Click += Cut;
                MainWindow.Instance.Menu_Edit_Copy.Click += Copy;
                MainWindow.Instance.Menu_Edit_Paste.Click += Paste;
                MainWindow.Instance.Menu_Edit_Undo.Click += Undo;
                MainWindow.Instance.Menu_Edit_Redo.Click += Redo;
                MainWindow.Instance.Menu_Edit_Clear.Click += Delete;
            }
        }

        public override void MdiDeactivated()
        {
            base.MdiDeactivated();
            if (Editor is not null)
            {
                Editor.Visibility = System.Windows.Visibility.Hidden;
            }

            if (MainWindow.Instance != null)
            {
                MainWindow.Instance.Menu_File_Print.Click -= Menu_File_Print_Click;

                MainWindow.Instance.Menu_Edit_Cut.Click -= Cut;
                MainWindow.Instance.Menu_Edit_Copy.Click -= Copy;
                MainWindow.Instance.Menu_Edit_Paste.Click -= Paste;
                MainWindow.Instance.Menu_Edit_Undo.Click -= Undo;
                MainWindow.Instance.Menu_Edit_Redo.Click -= Redo;
                MainWindow.Instance.Menu_Edit_Clear.Click -= Delete;
            }
        }

        private void Editor_EditorIsReady(object? sender, EventArgs e)
        {
            var temp = DataContext;
            DataContext = null;
            DataContext = temp;
        }

        private void Document_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName is null) return;

            //if (e.PropertyName.Equals(nameof(Editor.Document.BodyInnerHTML), StringComparison.CurrentCultureIgnoreCase))
            {
                ((Signature)DataContext).Content = Editor.Document.BodyInnerHTML;
            }
        }

        private void Editor_DocumentLoaded(object? sender, EventArgs e)
        {
            Editor.Document.DesignMode = true;
        }

        private void Uc_SignatureView_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is Signature signature)
            {
                Title = signature.Name;
                string tempPath = TempFileManager.CreateTempFileFromStringContent(signature.Content);
                if (string.IsNullOrEmpty(tempPath))
                {
                    Logger.NewEvent(LogEvent.EventCategory.Warning, "Failed to create temp file from signature content");
                    return;
                }
                Editor.Navigate(tempPath);
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
    }
}
