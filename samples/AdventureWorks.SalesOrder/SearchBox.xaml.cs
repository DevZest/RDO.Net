using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AdventureWorks.SalesOrders
{
    public enum SearchBoxState
    {
        Empty,
        Search,
        ClearSearch
    }

    /// <summary>
    /// Interaction logic for SearchBox.xaml
    /// </summary>
    public partial class SearchBox : UserControl
    {
        public static readonly RoutedUICommand SearchCommand = new RoutedUICommand();
        public static readonly RoutedUICommand ClearSearchCommand = new RoutedUICommand();

        private static readonly DependencyPropertyKey StatePropertyKey = DependencyProperty.RegisterReadOnly(nameof(State), typeof(SearchBoxState), typeof(SearchBox),
            new FrameworkPropertyMetadata(SearchBoxState.Empty));
        public static readonly DependencyProperty StateProperty = StatePropertyKey.DependencyProperty;

        public SearchBoxState State
        {
            get { return (SearchBoxState)GetValue(StateProperty); }
            private set { SetValue(StatePropertyKey, value); }
        }

        public SearchBox()
        {
            InitializeComponent();
        }

        public string SearchText { get; private set; }

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var t = (TextBox)sender;
            t.SelectAll();
        }

        private void SearchTextBox_GotMouseCapture(object sender, MouseEventArgs e)
        {
            var t = (TextBox)sender;
            t.SelectAll();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (State == SearchBoxState.Search)
                ExecuteSearchCommand();
            else if (State == SearchBoxState.ClearSearch)
                ExecuteClearSearchCommand();
        }

        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key.Equals(Key.Enter))
            {
                if (State == SearchBoxState.Search)
                    e.Handled = ExecuteSearchCommand();
            }
            else if (e.Key.Equals(Key.Escape))
            {
                InputText = SearchText;
                _searchTextBox.SelectAll();
            }
        }

        private bool ExecuteSearchCommand()
        {
            if (SearchCommand.CanExecute(null, null))
            {
                SearchText = InputText;
                SearchCommand.Execute(null, null);
                RefreshState();
                return true;
            }
            return false;
        }

        private bool ExecuteClearSearchCommand()
        {
            if (ClearSearchCommand.CanExecute(null, null))
            {
                SearchText = InputText = null;
                RefreshState();
                ClearSearchCommand.Execute(null, null);
                return true;
            }
            return false;
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            RefreshState(); 
        }

        private void SearchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            InputText = SearchText;
        }

        private string InputText
        {
            get { return _searchTextBox.Text; }
            set { _searchTextBox.Text = value; }
        }

        private void RefreshState()
        {
            State = CoerceState();
        }

        private SearchBoxState CoerceState()
        {
            if (string.IsNullOrEmpty(InputText))
                return SearchBoxState.Empty;
            else if (InputText != SearchText)
                return SearchBoxState.Search;
            else
                return SearchBoxState.ClearSearch;
        }
    }
}