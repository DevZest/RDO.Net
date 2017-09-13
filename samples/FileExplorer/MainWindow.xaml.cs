using DevZest.Data;
using DevZest.Data.Presenters;
using DevZest.Data.Views;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace FileExplorer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private abstract class ListManager
        {
            public static ListManager Create(ListManager oldValue, ListMode mode)
            {
                if (oldValue != null)
                {
                    if (oldValue.ListMode == mode)
                        return oldValue;
                    else
                        oldValue.DataPresenter.DetachView();
                }

                if (mode == ListMode.LargeIcon)
                    return new LargeIconListManager();
                else if (mode == ListMode.Details)
                    return new DetailsListManager();
                else
                    throw new ArgumentException("Invalid ListMode", nameof(mode));
            }

            public abstract ListMode ListMode { get; }

            public abstract void ShowOrRefreshAsync(DataView dataView, string currentFolder);

            public abstract FolderContent _ { get; }

            public abstract DataPresenter DataPresenter { get; }

            private abstract class ListManagerBase<T> : ListManager
                where T : FolderContent, new()
            {
                public sealed override FolderContent _
                {
                    get { return GetDataPresenter()._; }
                }

                public sealed override DataPresenter DataPresenter
                {
                    get { return GetDataPresenter(); }
                }

                protected abstract DataPresenter<T> GetDataPresenter();

                public sealed override void ShowOrRefreshAsync(DataView dataView, string currentFolder)
                {
                    GetDataPresenter().ShowOrRefreshAsync(dataView, (CancellationToken ct) => FolderContent.GetFolderContentsAsync<T>(currentFolder, ct));
                }
            }

            private sealed class LargeIconListManager : ListManagerBase<LargeIconListItem>
            {
                private readonly LargeIconList _largeIconList = new LargeIconList();
                protected override DataPresenter<LargeIconListItem> GetDataPresenter()
                {
                    return _largeIconList;
                }

                public override ListMode ListMode
                {
                    get { return ListMode.LargeIcon; }
                }
            }

            private sealed class DetailsListManager : ListManagerBase<DetailsListItem>
            {
                private readonly DetailsList _detailsList = new DetailsList();

                protected override DataPresenter<DetailsListItem> GetDataPresenter()
                {
                    return _detailsList;
                }

                public override ListMode ListMode
                {
                    get { return ListMode.Details; }
                }
            }
        }

        public static readonly DependencyProperty ListModeProperty = DependencyProperty.Register(nameof(ListMode), typeof(ListMode),
            typeof(MainWindow), new FrameworkPropertyMetadata(OnListModeChanged));

        private FolderTree _folderTree = new FolderTree();
        private ListManager _listManager;

        public MainWindow()
        {
            InitializeComponent();
            ListMode = ListMode.LargeIcon;
            Debug.Assert(_listManager != null);
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, ExecuteCloseCommand));
            _folderTree.ViewRefreshed += OnFolderTreeViewRefreshed;
            _folderTree.Show(_foldersTreeView, Folder.GetLogicalDrives());
        }

        public ListMode ListMode
        {
            get { return (ListMode)GetValue(ListModeProperty); }
            set { SetValue(ListModeProperty, value); }
        }

        private static void OnListModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((MainWindow)d).SetListManager((ListMode)e.NewValue);
        }

        private void SetListManager(ListMode value)
        {
            bool loadData = _listManager != null;
            _listManager = ListManager.Create(_listManager, value);
            if (loadData)
                _listManager.ShowOrRefreshAsync(_folderContentListView, CurrentFolder);
        }

        private void ExecuteCloseCommand(object sener, ExecutedRoutedEventArgs e)
        {
            Close();
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
                _listManager.ShowOrRefreshAsync(_folderContentListView, _currentFolder);
            }
        }

        private void OnFolderTreeViewRefreshed(object sender, EventArgs e)
        {
            var currentRow = _folderTree.CurrentRow;
            var _ = _folderTree._;
            CurrentFolder = currentRow == null ? null : currentRow.GetValue(_.Path);
        }
    }
}
