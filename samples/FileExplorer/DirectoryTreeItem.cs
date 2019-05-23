using DevZest.Data;
using System;
using System.IO;

namespace FileExplorer
{
    public class DirectoryTreeItem : Model
    {
        static DirectoryTreeItem()
        {
            RegisterLocalColumn((DirectoryTreeItem _) => _.Path);
            RegisterChildModel((DirectoryTreeItem _) => _.SubFolders);
        }

        public LocalColumn<string> Path { get; private set; }

        public DirectoryTreeItem SubFolders { get; private set; }

        public static DataSet<DirectoryTreeItem> GetLogicalDrives()
        {
            var result = DataSet<DirectoryTreeItem>.Create();
            foreach (string s in Directory.GetLogicalDrives())
                AddRow(result, s);
            return result;
        }

        public static void Expand(DataRow dataRow)
        {
            var folders = (DataSet<DirectoryTreeItem>)dataRow.DataSet;
            var children = folders._.SubFolders.GetChildDataSet(dataRow);
            if (children.Count == 1 && children._.Path[0] == null)
            {
                var path = folders._.Path[dataRow];
                children.Clear();
                foreach (string s in GetSubDirectories(path))
                    AddRow(children, s);
            }
        }

        public static string[] GetSubDirectories(string path)
        {
            try
            {
                return Directory.GetDirectories(path);
            }
            catch (UnauthorizedAccessException)
            {
                return new string[] { };
            }
        }

        private static void AddRow(DataSet<DirectoryTreeItem> folders, string path)
        {
            var dataRow = folders.AddRow((_, x) => _.Path[x] = path);
            folders._.SubFolders.GetChildDataSet(dataRow).AddRow();
        }
    }
}
