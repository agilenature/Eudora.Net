//
// IWin32Native
// Serves as the interface to the native Windows APIs that aren't
// neatly (or at all) exposed to .Net
//

using System.Drawing;
using System.Runtime.InteropServices;



namespace Eudora.Net.Core
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SHFILEINFO
    {
        public IntPtr hIcon;
        public int iIcon;
        public uint dwAttributes;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szDisplayName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
        public string szTypeName;
    };

    class GWin32Native
    {
        private const uint FILE_ATTRIBUTE_DIRECTORY = 0x00000010;
        private const uint FILE_ATTRIBUTE_NORMAL = 0x00000080;

        private const uint SHGFI_USEFILEATTRIBUTES = 0x000000010;
        private const uint SHGFI_TYPENAME = 0x000000400;
        private const uint SHGFI_ICON = 0x000000100;
        private const uint SHGFI_LARGEICON = 0x0;
        private const uint SHGFI_SMALLICON = 0x000000001;

        private static readonly Guid DownloadsFolder = new("374DE290-123F-4565-9164-39C4925E467B");

        
        [DllImport("shell32.dll")]
        public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, out SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

        [DllImport("Shell32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr SHILCreateFromPath([MarshalAs(UnmanagedType.LPWStr)] string pszPath, out IntPtr ppIdl, ref uint rgflnOut);

        [DllImport("shell32", CharSet = CharSet.Unicode, ExactSpelling = true, PreserveSig = false)]
        private static extern string SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)] Guid rfid, uint dwFlags, nint hToken = 0);


        // Because for some reason, the Downloads folder isn't in the list of
        // SpecialFolders presented to .Net
        public static string GetDownloadsFolder()
        {
            return SHGetKnownFolderPath(DownloadsFolder, 0);
        }

        public static string GetFileType(string fullPath)
        {
            SHFILEINFO info = new();
            uint dwFileAttributes = FILE_ATTRIBUTE_NORMAL;
            uint uFlags = (uint)(SHGFI_TYPENAME | SHGFI_USEFILEATTRIBUTES);

            SHGetFileInfo(fullPath, dwFileAttributes, out info, (uint)Marshal.SizeOf(info), uFlags);
            return info.szTypeName;
        }

        public static Icon? GetFolderIcon(string folder)
        {
            uint flags = SHGFI_ICON | SHGFI_USEFILEATTRIBUTES;
            uint attributes = FILE_ATTRIBUTE_NORMAL | FILE_ATTRIBUTE_DIRECTORY;

            int success = (int)SHGetFileInfo(folder, attributes, out SHFILEINFO shfi, (uint)Marshal.SizeOf(typeof(SHFILEINFO)), flags);

            if (success == 0)
            {
                return null;
            }

            return Icon.FromHandle(shfi.hIcon);
        }

        public static Icon? GetIconOfPath(string path, bool isDirectoryOrDrive, bool isSmallIcon = false)
        {
            uint flags = SHGFI_ICON | SHGFI_USEFILEATTRIBUTES;
            if (isSmallIcon)
            {
                flags |= SHGFI_SMALLICON;
            }

            uint attributes = FILE_ATTRIBUTE_NORMAL;
            if (isDirectoryOrDrive)
            {
                attributes |= FILE_ATTRIBUTE_DIRECTORY;
            }

            int success = (int)SHGetFileInfo(path, attributes, out SHFILEINFO shfi, (uint)Marshal.SizeOf(typeof(SHFILEINFO)), flags);

            if (success == 0)
            {
                return null;
            }

            return Icon.FromHandle(shfi.hIcon);
        }
    }
}
