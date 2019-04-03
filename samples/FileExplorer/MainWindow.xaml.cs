using System;
using System.Windows;
using System.Windows.Input;

namespace FileExplorer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static readonly DependencyProperty DirectoryListModeProperty = DependencyProperty.Register(nameof(DirectoryListMode), typeof(DirectoryListMode),
            typeof(MainWindow), new FrameworkPropertyMetadata(OnDirectoryListModeChanged));

        private DirectoryTreePresenter _directoryTreePresenter = new DirectoryTreePresenter();
        private IDirectoryListPresenter _directoryListPresenter;
        private CurrentDirectoryPresenter _currentDirectoryPresenter;

        public MainWindow()
        {
            InitializeComponent();
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, ExecuteCloseCommand));
            _directoryTreePresenter.Show(_directoryTreeView, DirectoryTreeItem.GetLogicalDrives());
            _currentDirectoryPresenter = new CurrentDirectoryPresenter(_currentDirectoryBarView, _directoryTreePresenter);
            DirectoryListMode = DirectoryListMode.LargeIcon;
        }

        public DirectoryListMode DirectoryListMode
        {
            get { return (DirectoryListMode)GetValue(DirectoryListModeProperty); }
            set { SetValue(DirectoryListModeProperty, value); }
        }

        private static void OnDirectoryListModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((MainWindow)d).SetDirectoryList((DirectoryListMode)e.NewValue);
        }

        private IDirectoryListPresenter CreateDirectoryList(IDirectoryListPresenter oldValue, DirectoryListMode mode)
        {
            if (oldValue != null)
            {
                if (oldValue.Mode == mode)
                    return oldValue;
                else
                    oldValue.Dispose();
            }

            if (mode == DirectoryListMode.LargeIcon)
                return new LargeIconListPresenter(_directoryListView, _directoryTreePresenter);
            else if (mode == DirectoryListMode.Details)
                return new DetailsListPresenter(_directoryListView, _directoryTreePresenter);
            else
                throw new ArgumentException("Invalid DirectoryListMode", nameof(mode));
        }

        private void SetDirectoryList(DirectoryListMode value)
        {
            _directoryListPresenter = CreateDirectoryList(_directoryListPresenter, value);
        }

        private void ExecuteCloseCommand(object sener, ExecutedRoutedEventArgs e)
        {
            Close();
        }
    }
}
