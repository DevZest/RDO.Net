using DevZest.Data;
using System;
using System.IO;
using System.Threading.Tasks;

namespace FileExplorer
{
    public class Folder : Model
    {
        static Folder()
        {
            RegisterChildModel((Folder x) => x.SubFolders);
        }

        public Column<string> Path { get; private set; }

        public Folder SubFolders { get; private set; }

        protected override void OnInitializing()
        {
            Path = CreateLocalColumn<string>();
            base.OnInitializing();
        }

        public static DataSet<Folder> GetLogicalDrives()
        {
            var result = DataSet<Folder>.New();
            foreach (string s in Directory.GetLogicalDrives())
                AddRow(result, s);
            return result;
        }

        public static void Expand(DataRow dataRow)
        {
            var folders = (DataSet<Folder>)dataRow.DataSet;
            var children = dataRow.Children(folders._.SubFolders);
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

        private static void AddRow(DataSet<Folder> folders, string path)
        {
            var dataRow = folders.AddRow((_, x) => _.Path[x] = path);
            dataRow.Children(folders._.SubFolders).AddRow();
        }
    }
}
