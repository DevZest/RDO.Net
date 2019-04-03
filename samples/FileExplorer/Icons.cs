using System;
using System.Windows.Media.Imaging;

namespace FileExplorer
{
    static class Icons
    {
        public static readonly BitmapImage DiskDrive = LoadIcon("pack://application:,,,/Icons/diskdrive.png");
        public static readonly BitmapImage Folder = LoadIcon("pack://application:,,,/Icons/folder.png");

        private static BitmapImage LoadIcon(string uriString)
        {
            var uri = new Uri(uriString);
            return new BitmapImage(uri);
        }
    }
}
