using DevZest.Data;
using System;
using System.IO;
using System.Windows.Media;

namespace FileExplorer
{
    public sealed class DetailsListItem : DirectoryItem
    {
        static DetailsListItem()
        {
            RegisterLocalColumn((DetailsListItem _) => _.SmallIcon);
            RegisterLocalColumn((DetailsListItem _) => _.FileSize);
            RegisterLocalColumn((DetailsListItem _) => _.FileType);
            RegisterLocalColumn((DetailsListItem _) => _.DateModified);
        }

        public LocalColumn<ImageSource> SmallIcon { get; private set; }
        public LocalColumn<long?> FileSize { get; private set; }
        public LocalColumn<string> FileType { get; private set; }
        public LocalColumn<DateTime> DateModified { get; private set; }

        public override void Initialize(DataRow x, DirectoryInfo directoryInfo)
        {
            base.Initialize(x, directoryInfo);
            SmallIcon[x] = Win32.GetDirectoryIcon(directoryInfo.FullName, true);
            FileType[x] = "Folder";
            DateModified[x] = directoryInfo.LastWriteTime;
        }

        protected override void Initialize(DataRow x, FileInfo fileInfo)
        {
            base.Initialize(x, fileInfo);
            SmallIcon[x] = Win32.GetFileIcon(fileInfo.FullName, true);
            FileSize[x] = Win32.GetFileSize(fileInfo.FullName);
            FileType[x] = Win32.GetFileType(fileInfo.FullName);
            DateModified[x] = fileInfo.LastWriteTime;
        }
    }
}
