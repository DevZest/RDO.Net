using DevZest.Data;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media;

namespace FileExplorer
{
    public class FolderContent : Model
    {
        public Column<string> Path { get; private set; }

        public Column<string> DisplayName { get; private set; }

        public Column<FolderContentType> Type { get; private set; }

        public Column<DateTime> LastWriteTime { get; private set; }

        public Column<ImageSource> SmallIcon { get; private set; }

        public Column<ImageSource> LargeIcon { get; private set; }

        public Column<string> FileType { get; private set; }

        public Column<long> FileSize { get; private set; }

        protected override void OnInitializing()
        {
            Path = CreateLocalColumn<string>();
            DisplayName = CreateLocalColumn<string>();
            Type = CreateLocalColumn<FolderContentType>();
            LastWriteTime = CreateLocalColumn<DateTime>();
            SmallIcon = CreateLocalColumn<ImageSource>();
            LargeIcon = CreateLocalColumn<ImageSource>();
            FileType = CreateLocalColumn<string>();
            FileSize = CreateLocalColumn<long>();
            base.OnInitializing();
        }

        public static async Task<DataSet<FolderContent>> GetFolderContentsAsync(string path)
        {
            return await Task.Run(() => GetFolderContents(path));
        }

        public static DataSet<FolderContent> GetFolderContents(string path)
        {
            var result = DataSet<FolderContent>.New();
            if (string.IsNullOrEmpty(path))
                return result;

            foreach (var folder in Folder.GetSubDirectories(path))
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(folder);
                result.AddRow((_, x) =>
                {
                    _.Path[x] = folder;
                    _.DisplayName[x] = directoryInfo.Name;
                    _.Type[x] = FolderContentType.Folder;
                    _.LastWriteTime[x] = directoryInfo.LastWriteTime;
                    _.SmallIcon[x] = Win32.GetDirectoryIcon(folder, true);
                    _.LargeIcon[x] = Win32.GetDirectoryIcon(folder, false);
                });
            }

            foreach (var file in GetFiles(path))
            {
                FileInfo fileInfo = new FileInfo(file);
                result.AddRow((_, x) =>
                {
                    _.Path[x] = file;
                    _.DisplayName[x] = fileInfo.Name;
                    _.Type[x] = FolderContentType.File;
                    _.LastWriteTime[x] = fileInfo.LastWriteTime;
                    _.SmallIcon[x] = Win32.GetFileIcon(file, true);
                    _.LargeIcon[x] = Win32.GetFileIcon(file, false);
                    _.FileType[x] = Win32.GetFileType(file);
                    _.FileSize[x] = Win32.GetFileSize(file);
                });
            }

            return result;
        }

        private static string[] GetFiles(string folder)
        {
            try
            {
                return Directory.GetFiles(folder);
            }
            catch (UnauthorizedAccessException)
            {
                return new string[] { };
            }
        }
    }
}
