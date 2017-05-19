using System;
using System.Diagnostics;
using System.Windows;

namespace FileExplorer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private _FoldersTree _foldersTree = new _FoldersTree();
        private _LargeIconsList _largeIconList;

        public MainWindow()
        {
            InitializeComponent();
            _foldersTree.ViewRefreshed += OnFolderTreeViewRefreshed;
            _foldersTree.Show(_foldersTreeView, Folder.GetLogicalDrives());
            new _LargeIconsList().Show(_folderContentListView, FolderContent.GetFolderContents(@"C:\"));
        }

        private string _currentFolder;
        private string CurrentFolder
        {
            get { return _currentFolder; }
            set
            {
                if (_currentFolder == value)
                    return;

                _currentFolder = value;
                Debug.WriteLine(string.Format("CurrentFolder={0}", CurrentFolder));
            }
        }

        private void OnFolderTreeViewRefreshed(object sender, EventArgs e)
        {
            var currentRow = _foldersTree.CurrentRow;
            var _ = _foldersTree._;
            CurrentFolder = currentRow == null ? null : currentRow.GetValue(_.Path);
        }
    }
}
