using DevZest.Data;
using System;
using System.IO;
using System.Windows.Media;

namespace FileExplorer
{
    public sealed class DetailListItem : FolderContent
    {
        public Column<DateTime> LastWriteTime { get; private set; }

        public Column<string> FileType { get; private set; }

        public Column<long> FileSize { get; private set; }

        public Column<ImageSource> SmallIcon { get; private set; }

        protected override void CreateLocalColumns()
        {
            base.CreateLocalColumns();
            LastWriteTime = CreateLocalColumn<DateTime>();
            FileType = CreateLocalColumn<string>();
            FileSize = CreateLocalColumn<long>();
            SmallIcon = CreateLocalColumn<ImageSource>();
        }

        protected override void Initialize(DataRow x, DirectoryInfo directoryInfo)
        {
            base.Initialize(x, directoryInfo);
            LastWriteTime[x] = directoryInfo.LastWriteTime;
        }

        protected override void Initialize(DataRow x, FileInfo fileInfo)
        {
            base.Initialize(x, fileInfo);
            LastWriteTime[x] = fileInfo.LastWriteTime;
            SmallIcon[x] = Win32.GetFileIcon(fileInfo.FullName, true);
            FileType[x] = Win32.GetFileType(fileInfo.FullName);
            FileSize[x] = Win32.GetFileSize(fileInfo.FullName);
        }
    }
}
