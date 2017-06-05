using DevZest.Data;
using System;
using System.IO;
using System.Windows.Media;

namespace FileExplorer
{
    public sealed class DetailsListItem : FolderContent
    {
        public Column<ImageSource> SmallIcon { get; private set; }
        public Column<long?> FileSize { get; private set; }
        public Column<string> FileType { get; private set; }
        public Column<DateTime> DateModified { get; private set; }

        protected override void CreateLocalColumns()
        {
            base.CreateLocalColumns();
            SmallIcon = CreateLocalColumn<ImageSource>();
            FileSize = CreateLocalColumn<long?>();
            FileType = CreateLocalColumn<string>();
            DateModified = CreateLocalColumn<DateTime>();
        }

        protected override void Initialize(DataRow x, DirectoryInfo directoryInfo)
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
