using DevZest.Data;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FileExplorer
{
    public abstract class DirectoryItem : Model
    {
        static DirectoryItem()
        {
            RegisterLocalColumn((DirectoryItem _) => _.Path);
            RegisterLocalColumn((DirectoryItem _) => _.DisplayName);
            RegisterLocalColumn((DirectoryItem _) => _.Type);
        }

        public LocalColumn<string> Path { get; private set; }

        public LocalColumn<string> DisplayName { get; private set; }

        public LocalColumn<DirectoryItemType> Type { get; private set; }

        private const string InvalidChars = @"\/:*?""<>|";
        private const string DisplayInvalidChars = @"\ / : * ? "" < > |";
        private static bool ContainsInvalidChar(string value)
        {
            for (int i = 0; i < InvalidChars.Length; i++)
            {
                var invalidChar = InvalidChars[i];
                if (value.Contains(invalidChar))
                    return true;
            }
            return false;
        }

        protected override IDataValidationErrors Validate(DataRow dataRow)
        {
            var result = base.Validate(dataRow);
            var displayName = DisplayName[dataRow];
            if (string.IsNullOrEmpty(displayName))
                result = result.Add(new DataValidationError("A file name can't be empty.", DisplayName));
            else if (ContainsInvalidChar(displayName))
                result = result.Add(new DataValidationError(string.Format("A file name can't contain any of the following characters: {0}", DisplayInvalidChars), DisplayName));
            return result;
        }

        public virtual void Initialize(DataRow x, DirectoryInfo directoryInfo)
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
            var result = DataSet<T>.Create();
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
