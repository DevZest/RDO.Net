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
        private _LargeIconsList _largeIconsList = new _LargeIconsList();

        public MainWindow()
        {
            InitializeComponent();
            _foldersTree.ViewRefreshed += OnFolderTreeViewRefreshed;
            _foldersTree.ShowAsync(_foldersTreeView, Folder.GetLogicalDrivesAsync);
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
                ShowCurrentFolderContents();
            }
        }

        private void ShowCurrentFolderContents()
        {
            string currentFolder;
            do
            {
                currentFolder = CurrentFolder;
                _largeIconsList.ShowAsync(_folderContentListView, () => FolderContent.GetFolderContentsAsync(currentFolder));
            }
            while (currentFolder != CurrentFolder);
        }

        private void OnFolderTreeViewRefreshed(object sender, EventArgs e)
        {
            var currentRow = _foldersTree.CurrentRow;
            var _ = _foldersTree._;
            CurrentFolder = currentRow == null ? null : currentRow.GetValue(_.Path);
        }
    }
}
