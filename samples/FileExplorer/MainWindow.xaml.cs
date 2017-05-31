using DevZest.Data;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
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
                ShowCurrentFolderContentsAsync();
            }
        }

        private bool _isShowingCurrentFolderContents;
        private async void ShowCurrentFolderContentsAsync()
        {
            if (_isShowingCurrentFolderContents)
                return;
            _isShowingCurrentFolderContents = true;
            await _largeIconsList.ShowAsync(_folderContentListView, GetCurrentFolderContentsAsync);
            _isShowingCurrentFolderContents = false;
        }

        private async Task<DataSet<FolderContent>> GetCurrentFolderContentsAsync()
        {
            string currentFolder;
            DataSet<FolderContent> result;
            do
            {
                currentFolder = CurrentFolder;
                result = await FolderContent.GetFolderContentsAsync(currentFolder);
            }
            while (currentFolder != CurrentFolder);
            return result;
        }

        private void OnFolderTreeViewRefreshed(object sender, EventArgs e)
        {
            var currentRow = _foldersTree.CurrentRow;
            var _ = _foldersTree._;
            CurrentFolder = currentRow == null ? null : currentRow.GetValue(_.Path);
        }
    }
}
