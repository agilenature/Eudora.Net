using Eudora.Net.Core;
using Shell32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Eudora.Net.GUI
{
    ///////////////////////////////////////////////////////////
    #region UX Helper classes
    /////////////////////////////

    class FileNode : INotifyPropertyChanged
    {
        ///////////////////////////////////////////////////////////
        #region INotifyPropertyChanged
        /////////////////////////////

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SetField<TField>(ref TField field, TField value, string propertyName)
        {
            if (EqualityComparer<TField>.Default.Equals(field, value))
            {
                return;
            }

            field = value;
            OnPropertyChanged(propertyName);
        }

        /////////////////////////////
        #endregion INotifyPropertyChanged
        ///////////////////////////////////////////////////////////


        ///////////////////////////////////////////////////////////
        #region Properties
        /////////////////////////////

        private string _DisplayName = string.Empty;
        public string DisplayName
        {
            get => _DisplayName;
            set => SetField(ref _DisplayName, value, nameof(DisplayName));
        }

        private string _Path = string.Empty;
        public string Path
        {
            get => _Path;
            set => SetField(ref _Path, value, nameof(Path));
        }

        private BitmapSource? _ImageSource;
        public BitmapSource? ImageSource
        {
            get => _ImageSource;
            set => SetField(ref _ImageSource, value, nameof(ImageSource));
        }

        private long _Size = 0;
        public long Size
        {
            get => _Size;
            set => SetField(ref _Size, value, nameof(Size));
        }

        private DateTime _Modified = DateTime.Now;
        public DateTime Modified
        {
            get => _Modified;
            set => SetField(ref _Modified, value, nameof(Modified));
        }

        private string _FileType = "File";
        public string FileType
        {
            get => _FileType;
            set => SetField(ref _FileType, value, nameof(FileType));
        }
            

        /////////////////////////////
        #endregion Properties
        ///////////////////////////////////////////////////////////
        
        public void Execute()
        {
            try
            {
                using Process fileopener = new();
                fileopener.StartInfo.FileName = "explorer";
                fileopener.StartInfo.Arguments = "\"" + Path + "\"";
                fileopener.Start();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }
    }

    class FolderNode : INotifyPropertyChanged
    {
        ///////////////////////////////////////////////////////////
        #region INotifyPropertyChanged
        /////////////////////////////

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SetField<TField>(ref TField field, TField value, string propertyName)
        {
            if (EqualityComparer<TField>.Default.Equals(field, value))
            {
                return;
            }

            field = value;
            OnPropertyChanged(propertyName);
        }

        /////////////////////////////
        #endregion INotifyPropertyChanged
        ///////////////////////////////////////////////////////////


        ///////////////////////////////////////////////////////////
        #region Properties
        /////////////////////////////
        
        static readonly FolderNode PlaceholderFolder = new FolderNode();
        static readonly FileNode PlaceholderFile = new FileNode();

        private string _DisplayName = string.Empty;
        public string DisplayName
        {
            get => _DisplayName;
            set => SetField(ref _DisplayName, value, nameof(DisplayName));
        }

        private string _Path = string.Empty;
        public string Path
        {
            get => _Path;
            set => SetField(ref _Path, value, nameof(Path));
        }

        private BitmapSource? _ImageSource;
        public BitmapSource? ImageSource
        {
            get => _ImageSource;
            set => SetField(ref _ImageSource, value, nameof(ImageSource));
        }

        private BitmapSource? _ImageSourceOpen;
        public BitmapSource? ImageSourceOpen
        {
            get => _ImageSourceOpen;
            set => SetField(ref _ImageSourceOpen, value, nameof(ImageSourceOpen));
        }

        private bool _IsExpanded = false;
        public bool IsExpanded
        {
            get => _IsExpanded;
            set => SetField(ref _IsExpanded, value, nameof(IsExpanded));
        }

        private bool _IsSelected = false;
        public bool IsSelected
        {
            get => _IsSelected;
            set => SetField(ref _IsSelected, value, nameof(IsSelected));
        }

        private ObservableCollection<FolderNode> _Subfolders = [];
        public ObservableCollection<FolderNode> Subfolders
        {
            get => _Subfolders;
        }

        private ObservableCollection<FileNode> _Files = [];
        public ObservableCollection<FileNode> Files
        {
            get => _Files;
        }

        /////////////////////////////
        #endregion Properties
        ///////////////////////////////////////////////////////////
        
        public FolderNode()
        {
            Subfolders.Add(PlaceholderFolder);
            Files.Add(PlaceholderFile);
            PropertyChanged += FolderNode_PropertyChanged;
        }

        private void FolderNode_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(IsExpanded) && IsExpanded == true && HasPlaceholderFolder())
            {
                LoadSubfolders();
            }

            if(e.PropertyName == nameof(IsSelected) && IsSelected == true && HasPlaceholderFile())
            { 
                LoadFiles(); 
            }
        }

        bool HasPlaceholderFolder()
        {
            return Subfolders.Contains(PlaceholderFolder);
        }

        bool HasPlaceholderFile()
        {
            return Files.Contains(PlaceholderFile);
        }

        void LoadSubfolders()
        {
            try
            {
                if (HasPlaceholderFolder())
                {
                    Subfolders.Clear();
                }
                DirectoryInfo di = new(Path);
                var dis = di.GetDirectories();
                foreach (var d in dis)
                {
                    FolderNode node = new()
                    {
                        DisplayName = d.Name,
                        Path = d.FullName,
                        ImageSource = IconCache.GetBitmapSourceFromFolder(d.FullName)
                    };
                    Subfolders.Add(node);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        void LoadFiles()
        {
            try
            {
                if (HasPlaceholderFile())
                {
                    Files.Clear();
                }
                DirectoryInfo di = new(Path);
                var fileInfos = di.GetFiles();
                foreach (FileInfo fileInfo in fileInfos)
                {
                    FileNode node = new()
                    {
                        DisplayName = fileInfo.Name,
                        Path = fileInfo.FullName,
                        ImageSource = IconCache.GetBitmapSourceFromFile(fileInfo.FullName),
                        Size = fileInfo.Length,
                        FileType = GWin32Native.GetFileType(fileInfo.FullName),
                        Modified = fileInfo.LastWriteTime
                    };
                    Files.Add(node);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }            
        }
    }

    /////////////////////////////
    #endregion UX Helper classes
    ///////////////////////////////////////////////////////////
    

    /// <summary>
    /// Interaction logic for uc_FileBrowserTab.xaml
    /// </summary>
    public partial class uc_FileBrowserTab : uc_TabBase
    {
        ///////////////////////////////////////////////////////////
        #region Properties
        /////////////////////////////

        private ObservableCollection<FolderNode> Folders { get; } = [];
        private ObservableCollection<FileNode> Files { get; set; } = [];

        /////////////////////////////
        #endregion Properties
        ///////////////////////////////////////////////////////////


        public uc_FileBrowserTab()
        {
            InitializeComponent();
            InitGui();
        }

        FolderNode MakeFolderNode(Environment.SpecialFolder folder, string displayName)
        {
            string? folderName = Enum.GetName(folder);
            if (folderName is null) folderName = string.Empty;
            string folderPath = Environment.GetFolderPath((Environment.SpecialFolder)Enum.Parse(typeof(Environment.SpecialFolder), folderName));

            FolderNode node = new()
            {
                DisplayName = displayName,
                Path = folderPath,
                ImageSource = IconCache.GetBitmapSourceFromFolder(displayName)
            };
            return node;
        }


        private void InitGui()
        {
            treeView.DataContext = Folders;            
            FolderNode ThisPC = new()
            {
                DisplayName = "This PC",
                Path = string.Empty,
                ImageSource = IconCache.GetBitmapSourceFromFolder("This PC")
            };
            ThisPC.Subfolders.Clear();
            ThisPC.Files.Clear();
            Folders.Add(ThisPC);

            // Selected "Quick Access" folders
            ThisPC.Subfolders.Add(MakeFolderNode(Environment.SpecialFolder.DesktopDirectory, "Desktop"));
            ThisPC.Subfolders.Add(MakeFolderNode(Environment.SpecialFolder.MyDocuments, "Documents"));
            ThisPC.Subfolders.Add(new()
            {
                DisplayName = "Downloads",
                Path = GWin32Native.GetDownloadsFolder(),
                ImageSource = IconCache.GetBitmapSourceFromFolder(GWin32Native.GetDownloadsFolder())
            });
            ThisPC.Subfolders.Add(MakeFolderNode(Environment.SpecialFolder.MyPictures, "Pictures"));
            ThisPC.Subfolders.Add(MakeFolderNode(Environment.SpecialFolder.MyVideos, "Videos"));

            // Drives
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                ThisPC.Subfolders.Add(new()
                {
                    DisplayName = drive.Name,
                    Path = drive.Name,
                    ImageSource = IconCache.GetBitmapSourceFromFolder(drive.Name)
                });
            }


            ThisPC.IsExpanded = true;
        }

        private void PopulateFolders(Folder folder, TreeViewItem treeNode)
        {
            Shell32.Shell shell = new Shell();
            
            foreach(ShellFolderItem item in folder.Items())
            {
                if (item.IsFolder)
                {
                    TreeViewItem tvi = new TreeViewItem() { Header = item.Name };
                    treeNode.Items.Add(tvi);
                    Shell32.Folder itemFolder = item.GetFolder;
                    
                    PopulateFolders(itemFolder, tvi);
                }
            }
        }

        private void AddQuickAccessItems()
        {
            // Add "Quick Access" node
            TreeViewItem quickAccessNode = new TreeViewItem { Header = "Quick Access" };
            treeView.Items.Add(quickAccessNode);

            // Get special folders and add them as child nodes to "Quick Access"
            foreach (string specialFolder in Enum.GetNames(typeof(Environment.SpecialFolder)))
            {
                string folderPath = Environment.GetFolderPath((Environment.SpecialFolder)Enum.Parse(typeof(Environment.SpecialFolder), specialFolder));

                if (!string.IsNullOrEmpty(folderPath) && Directory.Exists(folderPath))
                {
                    TreeViewItem folderNode = new TreeViewItem { Header = specialFolder };
                    quickAccessNode.Items.Add(folderNode);
                }
            }
        }

        private void AddThisPCAndDrives()
        {
            // Add "This PC" node
            TreeViewItem thisPCNode = new TreeViewItem { Header = "This PC" };
            treeView.Items.Add(thisPCNode);

            // Get drives and add them as child nodes to "This PC"
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                TreeViewItem driveNode = new TreeViewItem { Header = drive.Name };
                thisPCNode.Items.Add(driveNode);
            }
        }

        private void treeView_SelectedItemChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<object> e)
        {
            if(treeView.SelectedItem is FolderNode node)
            {
                Files = node.Files;
                //listView.DataContext = Files;
                dgfiles.DataContext = Files;
            }
        }

        private void DataGridRow_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if(dgfiles.SelectedItem is FileNode node)
            {
                node.Execute();
            }
        }
    }
}
