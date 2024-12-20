﻿using Eudora.Net.Core;
using Eudora.Net.Data;
using Eudora.Net.ExtensionMethods;
using System.Windows;

namespace Eudora.Net.GUI
{
    /// <summary>
    /// Interaction logic for uc_StationeryView.xaml
    /// </summary>
    public partial class uc_StationeryView : ChildWindowBase
    {
        public uc_StationeryView(Stationery stationery)
        {
            InitializeComponent();
            DataContextChanged += Uc_StationeryView_DataContextChanged;
            DataContext = stationery;
            Editor.Document.PropertyChanged += Document_PropertyChanged;
            Editor.DocumentLoaded += Editor_DocumentLoaded;
            Editor.ToolbarFontStyleEnabled = false;
            Editor.ToolbarAlignmentEnabled = false;
            Editor.ToolbarEmbedEnabled = false;
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

        private async void Document_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName is null) return;

            if (e.PropertyName.Equals(nameof(Editor.Document.BodyInnerHTML), StringComparison.CurrentCultureIgnoreCase))
            {
                ((Stationery)DataContext).Content = Editor.Document.BodyOuterHTML;
            }
            if (e.PropertyName.Equals(nameof(Editor.Document.BodyStyle), StringComparison.CurrentCultureIgnoreCase))
            {
                ((Stationery)DataContext).Content = Editor.Document.BodyOuterHTML;
            }

            await Editor.Document.GetBodyStyle();
            ((Stationery)DataContext).Style = Editor.Document.BodyStyle.Value;
        }

        private void Editor_DocumentLoaded(object? sender, EventArgs e)
        {
            Editor.Document.DesignMode = true;
        }

        private void Uc_StationeryView_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if(DataContext is Stationery stationery)
            {
                stationery.PropertyChanged += Stationery_PropertyChanged;

                Title = stationery.Name;
                string tempPath = TempFileManager.CreateHtmlFileFromStringContent(stationery.Content);
                if (string.IsNullOrEmpty(tempPath))
                {
                    Logger.Debug("Failed to create temp file from stationery content");
                    return;
                }
                Editor.Navigate(tempPath);
            }
        }

        private async void Stationery_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (DataContext is Stationery stationery)
            {
                await Editor.Document.GetBodyStyle();
                stationery.Style = Editor.Document.BodyStyle.Value;
            }            
        }

        public void Cut(object sender, RoutedEventArgs e)
        {
            Editor.Webview.ExecuteScriptAsync("document.execCommand('Cut');");
        }

        public void Copy(object sender, RoutedEventArgs e)
        {
            Editor.Webview.ExecuteScriptAsync("document.execCommand('Copy');");
        }

        public void Paste(object sender, RoutedEventArgs e)
        {
            Editor.Webview.ExecuteScriptAsync("document.execCommand('Insert');");
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

        private void TextBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                if (DataContext is Stationery stationery)
                {
                    stationery.Name = tb_Name.Text;
                }
            }
        }
    }
}
