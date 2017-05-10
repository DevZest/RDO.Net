using DevZest.Data;
using System.IO;

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
            Path = DataSetContainer.CreateLocalColumn<string>(this);
            base.OnInitializing();
        }

        public static DataSet<Folder> GetLogicalDrives()
        {
            var result = DataSet<Folder>.New();
            foreach (string s in Directory.GetLogicalDrives())
                AddRow(result, s);
            return result;
        }

        public static void Expand(DataSet<Folder> folders, int ordinal)
        {
            var children = folders.Children(x => x.SubFolders, ordinal);
            if (children.Count == 1 && children._.Path[0] == null)
            {
                children.Clear();
                foreach (string s in Directory.GetDirectories(folders._.Path[ordinal]))
                    AddRow(children, s);
            }
        }

        private static void AddRow(DataSet<Folder> folders, string path)
        {
            var dataRow = folders.AddRow((_, x) => _.Path[x] = path);
            dataRow.Children(folders._.SubFolders).AddRow();
        }
    }
}
