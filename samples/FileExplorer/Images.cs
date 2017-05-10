using System;
using System.Windows.Media.Imaging;

namespace FileExplorer
{
    static class Images
    {
        public static readonly BitmapImage DiskDrive = LoadImage("pack://application:,,,/Images/diskdrive.png");
        public static readonly BitmapImage Folder = LoadImage("pack://application:,,,/Images/folder.png");

        private static BitmapImage LoadImage(string uriString)
        {
            var uri = new Uri(uriString);
            return new BitmapImage(uri);
        }
    }
}
