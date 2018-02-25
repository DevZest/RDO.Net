using DevZest.Data.Presenters;
using DevZest.Data.Views;
using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace FileExplorer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private abstract class DirectoryListManager
        {
            public static DirectoryListManager Create(DirectoryListManager oldValue, DirectoryListMode mode)
            {
                if (oldValue != null)
                {
                    if (oldValue.DirectoryListMode == mode)
                        return oldValue;
                    else
                        oldValue.DataPresenter.DetachView();
                }

                if (mode == DirectoryListMode.LargeIcon)
                    return new LargeIconListManager();
                else if (mode == DirectoryListMode.Details)
                    return new DetailsListManager();
                else
                    throw new ArgumentException("Invalid DirectoryListMode", nameof(mode));
            }

            public abstract DirectoryListMode DirectoryListMode { get; }

            public abstract void ShowOrRefreshAsync(DataView dataView, string currentFolder);

            public abstract DirectoryItem _ { get; }

            public abstract DataPresenter DataPresenter { get; }

            private abstract class DirectoryListManagerBase<T> : DirectoryListManager
                where T : DirectoryItem, new()
            {
                public sealed override DirectoryItem _
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
                    GetDataPresenter().ShowOrRefreshAsync(dataView, (CancellationToken ct) => DirectoryItem.GetDirectoryItemsAsync<T>(currentFolder, ct));
                }
            }

            private sealed class LargeIconListManager : DirectoryListManagerBase<LargeIconListItem>
            {
                private readonly LargeIconList _largeIconList = new LargeIconList();
                protected override DataPresenter<LargeIconListItem> GetDataPresenter()
                {
                    return _largeIconList;
                }

                public override DirectoryListMode DirectoryListMode
                {
                    get { return DirectoryListMode.LargeIcon; }
                }
            }

            private sealed class DetailsListManager : DirectoryListManagerBase<DetailsListItem>
            {
                private readonly DetailsList _detailsList = new DetailsList();

                protected override DataPresenter<DetailsListItem> GetDataPresenter()
                {
                    return _detailsList;
                }

                public override DirectoryListMode DirectoryListMode
                {
                    get { return DirectoryListMode.Details; }
                }
            }
        }

        public static readonly DependencyProperty DirectoryListModeProperty = DependencyProperty.Register(nameof(DirectoryListMode), typeof(DirectoryListMode),
            typeof(MainWindow), new FrameworkPropertyMetadata(OnDirectoryListModeChanged));

        private DirectoryTree _directoryTree = new DirectoryTree();
        private DirectoryListManager _listManager;

        public MainWindow()
        {
            InitializeComponent();
            DirectoryListMode = DirectoryListMode.LargeIcon;
            Debug.Assert(_listManager != null);
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, ExecuteCloseCommand));
            _directoryTree.ViewRefreshed += OnDirectoryTreeViewRefreshed;
            _directoryTree.Show(_foldersTreeView, DirectoryTreeItem.GetLogicalDrives());
        }

        public DirectoryListMode DirectoryListMode
        {
            get { return (DirectoryListMode)GetValue(DirectoryListModeProperty); }
            set { SetValue(DirectoryListModeProperty, value); }
        }

        private static void OnDirectoryListModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((MainWindow)d).SetListManager((DirectoryListMode)e.NewValue);
        }

        private void SetListManager(DirectoryListMode value)
        {
            bool loadData = _listManager != null;
            _listManager = DirectoryListManager.Create(_listManager, value);
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

        private void OnDirectoryTreeViewRefreshed(object sender, EventArgs e)
        {
            var currentRow = _directoryTree.CurrentRow;
            var _ = _directoryTree._;
            CurrentFolder = currentRow == null ? null : currentRow.GetValue(_.Path);
        }
    }
}
