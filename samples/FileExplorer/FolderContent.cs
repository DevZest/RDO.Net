using DevZest.Data;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;

namespace FileExplorer
{
    public abstract class FolderContent : Model
    {
        public Column<string> Path { get; private set; }

        public Column<string> DisplayName { get; private set; }

        public Column<FolderContentType> Type { get; private set; }

        protected sealed override void OnInitializing()
        {
            CreateLocalColumns();
            base.OnInitializing();
        }

        protected virtual void CreateLocalColumns()
        {
            Path = CreateLocalColumn<string>();
            DisplayName = CreateLocalColumn<string>();
            Type = CreateLocalColumn<FolderContentType>();
        }

        protected virtual void Initialize(DataRow x, DirectoryInfo directoryInfo)
        {
            Path[x] = directoryInfo.FullName;
            DisplayName[x] = directoryInfo.Name;
            Type[x] = FolderContentType.Folder;
        }

        protected virtual void Initialize(DataRow x, FileInfo fileInfo)
        {
            Path[x] = fileInfo.FullName;
            DisplayName[x] = fileInfo.Name;
            Type[x] = FolderContentType.File;
        }

        public static async Task<DataSet<T>> GetFolderContentsAsync<T>(string path)
            where T : FolderContent, new()
        {
            return await Task.Run(() => GetFolderContents<T>(path, CancellationToken.None));
        }

        public static async Task<DataSet<T>> GetFolderContentsAsync<T>(string path, CancellationToken ct)
            where T : FolderContent, new()
        {
            return await Task.Run(() => GetFolderContents<T>(path, ct));
        }

        public static DataSet<T> GetFolderContents<T>(string path)
            where T : FolderContent, new()
        {
            return GetFolderContents<T>(path, CancellationToken.None);
        }

        private static DataSet<T> GetFolderContents<T>(string path, CancellationToken ct)
            where T : FolderContent, new()
        {
            var result = DataSet<T>.New();
            if (string.IsNullOrEmpty(path))
                return result;

            foreach (var folder in DirectoryTreeItem.GetSubDirectories(path))
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(folder);
                result.AddRow((_, x) => _.Initialize(x, directoryInfo));
                ct.ThrowIfCancellationRequested();
            }

            foreach (var file in GetFiles(path))
            {
                FileInfo fileInfo = new FileInfo(file);
                result.AddRow((_, x) => _.Initialize(x, fileInfo));
                ct.ThrowIfCancellationRequested();
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
