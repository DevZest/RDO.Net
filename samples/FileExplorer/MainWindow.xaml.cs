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
        public static readonly DependencyProperty DirectoryListModeProperty = DependencyProperty.Register(nameof(DirectoryListMode), typeof(DirectoryListMode),
            typeof(MainWindow), new FrameworkPropertyMetadata(OnDirectoryListModeChanged));

        private DirectoryTree _directoryTree = new DirectoryTree();
        private IDirectoryList _directoryList;

        public MainWindow()
        {
            InitializeComponent();
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, ExecuteCloseCommand));
            _directoryTree.Show(_directoryTreeView, DirectoryTreeItem.GetLogicalDrives());
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

        private IDirectoryList CreateDirectoryList(IDirectoryList oldValue, DirectoryListMode mode)
        {
            if (oldValue != null)
            {
                if (oldValue.Mode == mode)
                    return oldValue;
                else
                    oldValue.Dispose();
            }

            if (mode == DirectoryListMode.LargeIcon)
                return new LargeIconList(_directoryListView, _directoryTree);
            else if (mode == DirectoryListMode.Details)
                return new DetailsList(_directoryListView, _directoryTree);
            else
                throw new ArgumentException("Invalid DirectoryListMode", nameof(mode));
        }

        private void SetDirectoryList(DirectoryListMode value)
        {
            _directoryList = CreateDirectoryList(_directoryList, value);
        }

        private void ExecuteCloseCommand(object sener, ExecutedRoutedEventArgs e)
        {
            Close();
        }
    }
}
