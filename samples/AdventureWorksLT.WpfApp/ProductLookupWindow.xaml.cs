using DevZest.Data.Presenters;
using DevZest.Data.Views;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace DevZest.Samples.AdventureWorksLT
{
    /// <summary>
    /// Interaction logic for ProductLookupWindow.xaml
    /// </summary>
    public partial class ProductLookupWindow : Window
    {
        public static class Commands
        {
            public static RoutedUICommand SelectCurrent { get { return ApplicationCommands.Open; } }
            public static RoutedUICommand Refresh { get { return NavigationCommands.Refresh; } }
            public static RoutedUICommand Search { get { return SearchBox.Commands.Search; } }
            public static RoutedUICommand ClearSearch { get { return SearchBox.Commands.ClearSearch; } }
            public static RoutedUICommand Close { get { return ApplicationCommands.Close; } }
        }

        public ProductLookupWindow()
        {
            InitializeComponent();
        }

        private Presenter _presenter;
        private ForeignKeyBox _foreignKeyBox;
        private Product.PK PK
        {
            get { return (Product.PK)_foreignKeyBox.ForeignKey; }
        }
        private Product.Lookup Lookup
        {
            get { return (Product.Lookup)_foreignKeyBox.Lookup; }
        }

        public void Show(Window ownerWindow, ForeignKeyBox foreignKeyBox, int? currentProductID)
        {
            Debug.Assert(ownerWindow != null);
            Debug.Assert(foreignKeyBox != null);

            Owner = ownerWindow;
            _foreignKeyBox = foreignKeyBox;
            _presenter = new Presenter(_dataView, currentProductID);
            InitializeCommandBindings();
            ShowDialog();
        }

        private void InitializeCommandBindings()
        {
            CommandBindings.Add(new CommandBinding(Commands.SelectCurrent, SelectCurrent, CanSelectCurrent));
            CommandBindings.Add(new CommandBinding(Commands.Refresh, Refresh, CanRefresh));
            CommandBindings.Add(new CommandBinding(Commands.Search, Search, CanRefresh));
            CommandBindings.Add(new CommandBinding(Commands.ClearSearch, ClearSearch, CanRefresh));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, Close));
        }

        private RowPresenter CurrentRow
        {
            get { return _presenter.CurrentRow; }
        }

        private Product _
        {
            get { return _presenter._; }
        }

        private void SelectCurrent(object sender, ExecutedRoutedEventArgs e)
        {
            if (_presenter.CurrentProductID != CurrentRow.GetValue(_.ProductID))
                _foreignKeyBox.EndLookup(CurrentRow.MakeValueBag(PK, Lookup));
            Close();
        }

        private void CanSelectCurrent(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _presenter.CurrentRow != null;
        }

        private void Refresh(object sender, ExecutedRoutedEventArgs e)
        {
            _presenter.RefreshAsync();
        }

        private void CanRefresh(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _presenter.DataSet != null;
        }

        private void Search(object sender, ExecutedRoutedEventArgs e)
        {
            _presenter.SearchText = _searchBox.SearchText;
        }

        private void ClearSearch(object sender, ExecutedRoutedEventArgs e)
        {
            _presenter.SearchText = null;
        }

        private void Close(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }
    }
}
