using Eudora.Net.Core;
using Eudora.Net.Data;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace Eudora.Net.GUI
{
    /// <summary>
    /// Interaction logic for uc_AttachmentsBar.xaml
    /// </summary>
    public partial class uc_AttachmentsBar : UserControl
    {
        public uc_AttachmentsBar()
        {
            InitializeComponent();
            DataContextChanged += Uc_AttachmentsBar_DataContextChanged;
        }

        private void RefreshAttachments()
        {
            if (DataContext is ObservableCollection<EmailAttachment> attachments)
            {
                Toolbar.Items.Clear();
                foreach (var attachment in attachments)
                {
                    AttachmentButton button = new(attachment);
                    button.Click += AttachmentButton_Click;
                    Toolbar.Items.Add(button);
                }
            }
        }

        private void Uc_AttachmentsBar_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is null)
            {
                IsEnabled = false;
                return;
            }
            
            if(DataContext is ObservableCollection<EmailAttachment> attachments)
            {
                IsEnabled = true;
                attachments.CollectionChanged += Attachments_CollectionChanged;
                RefreshAttachments();
            }
        }

        private void Attachments_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RefreshAttachments();
        }

        private void AttachmentButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is AttachmentButton button)
                {
                    // Fix for microsoft \/ nonsense
                    button.Attachment.Path = button.Attachment.Path.Replace("/", "\\");

                    // shell execute the attachment file path
                    using Process fileopener = new Process();
                    fileopener.StartInfo.FileName = "explorer";
                    fileopener.StartInfo.Arguments = $"{button.Attachment.Path}";
                    fileopener.Start();
                }
            }
            catch(Exception ex)
            {
                Logger.Exception(ex);
            }
        }
    }
}
