using DevZest.Data;
using System.Windows.Media;
using System.IO;

namespace FileExplorer
{
    public sealed class LargeIconListItem : FolderContent
    {
        public Column<ImageSource> LargeIcon { get; private set; }

        protected override void CreateLocalColumns()
        {
            base.CreateLocalColumns();
            LargeIcon = CreateLocalColumn<ImageSource>();
        }

        protected override void Initialize(DataRow x, DirectoryInfo directoryInfo)
        {
            base.Initialize(x, directoryInfo);
            LargeIcon[x] = Win32.GetDirectoryIcon(directoryInfo.FullName, false);
        }

        protected override void Initialize(DataRow x, FileInfo fileInfo)
        {
            base.Initialize(x, fileInfo);
            LargeIcon[x] = Win32.GetFileIcon(fileInfo.FullName, false);
        }
    }
}
