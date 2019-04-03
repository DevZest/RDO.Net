using DevZest.Data;
using System.Windows.Media;
using System.IO;

namespace FileExplorer
{
    public sealed class LargeIconListItem : DirectoryItem
    {
        static LargeIconListItem()
        {
            RegisterLocalColumn((LargeIconListItem _) => _.LargeIcon);
        }

        public LocalColumn<ImageSource> LargeIcon { get; private set; }

        public override void Initialize(DataRow x, DirectoryInfo directoryInfo)
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
