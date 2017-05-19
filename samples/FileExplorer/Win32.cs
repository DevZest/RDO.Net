using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FileExplorer
{
    internal class Win32
    {
        [StructLayout(LayoutKind.Sequential)]
        struct SHFILEINFO
        {
            public IntPtr hIcon;
            public IntPtr iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        };

        static class FILE_ATTRIBUTE
        {
            public const uint FILE_ATTRIBUTE_NORMAL = 0x80;
        }

        static class SHGFI
        {
            public const uint SHGFI_TYPENAME = 0x000000400;
            public const uint SHGFI_USEFILEATTRIBUTES = 0x000000010;
        }

        public const uint SHGFI_ICON = 0x100;
        public const uint SHGFI_LARGEICON = 0x0;    // 'Large icon
        public const uint SHGFI_SMALLICON = 0x1;    // 'Small icon

        [DllImport("shell32.dll")]
        static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

        [DllImport("user32")]
        public static extern int DestroyIcon(IntPtr hIcon);

        public static long GetFileSize(string fullpath)
        {
            return new FileInfo(fullpath).Length;
        }

        public static string GetFileType(string fullpath)
        {
            uint dwFileAttributes = FILE_ATTRIBUTE.FILE_ATTRIBUTE_NORMAL;
            uint uFlags = (uint)(SHGFI.SHGFI_TYPENAME | SHGFI.SHGFI_USEFILEATTRIBUTES);
            SHFILEINFO shinfo = new SHFILEINFO();
            IntPtr n = Win32.SHGetFileInfo(fullpath, dwFileAttributes, ref shinfo, (uint)Marshal.SizeOf(shinfo), uFlags);

            return shinfo.szTypeName;
        }

        public static ImageSource GetIcon(string fullpath, bool isSmall)
        {
            IntPtr hResult;
            SHFILEINFO shinfo = new SHFILEINFO();

            hResult = SHGetFileInfo(fullpath, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo), SHGFI_ICON | (isSmall ? SHGFI_SMALLICON : SHGFI_LARGEICON));
            return GetIcon(shinfo.hIcon);
        }

        private static ImageSource GetIcon(IntPtr intPtr)
        {
            using (Icon icon = Icon.FromHandle(intPtr))
            {
                Debug.WriteLine(string.Format("IconSize={0}, {1}", icon.Width, icon.Height));
                return Imaging.CreateBitmapSourceFromHIcon(icon.Handle, new Int32Rect(0, 0, icon.Width, icon.Height), BitmapSizeOptions.FromEmptyOptions());
            }
        }
    }
}
