using DevZest.Data;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FileExplorer
{
    public abstract class DirectoryItem : Model
    {
        public Column<string> Path { get; private set; }

        public Column<string> DisplayName { get; private set; }

        public Column<DirectoryItemType> Type { get; private set; }

        protected sealed override void OnInitializing()
        {
            CreateLocalColumns();
            base.OnInitializing();
        }

        protected virtual void CreateLocalColumns()
        {
            Path = CreateLocalColumn<string>();
            DisplayName = CreateLocalColumn<string>();
            Type = CreateLocalColumn<DirectoryItemType>();
        }

        protected virtual void Initialize(DataRow x, DirectoryInfo directoryInfo)
        {
            Path[x] = directoryInfo.FullName;
            DisplayName[x] = directoryInfo.Name;
            Type[x] = DirectoryItemType.Directory;
        }

        protected virtual void Initialize(DataRow x, FileInfo fileInfo)
        {
            Path[x] = fileInfo.FullName;
            DisplayName[x] = fileInfo.Name;
            Type[x] = DirectoryItemType.File;
        }

        public static async Task<DataSet<T>> GetDirectoryItemsAsync<T>(string path)
            where T : DirectoryItem, new()
        {
            return await Task.Run(() => GetDirectoryItems<T>(path, CancellationToken.None));
        }

        public static async Task<DataSet<T>> GetDirectoryItemsAsync<T>(string path, CancellationToken ct)
            where T : DirectoryItem, new()
        {
            return await Task.Run(() => GetDirectoryItems<T>(path, ct));
        }

        public static DataSet<T> GetDirectoryItems<T>(string path)
            where T : DirectoryItem, new()
        {
            return GetDirectoryItems<T>(path, CancellationToken.None);
        }

        private static DataSet<T> GetDirectoryItems<T>(string path, CancellationToken ct)
            where T : DirectoryItem, new()
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
