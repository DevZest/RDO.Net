using DevZest.Data;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace Movies.WPF
{
    public partial class MainWindow : Window, MainWindow.Presenter.IFilter
    {
        public static class Commands
        {
            public static RoutedUICommand New { get { return ApplicationCommands.New; } }
            public static RoutedUICommand Open { get { return ApplicationCommands.Open; } }
            public static RoutedUICommand Delete { get { return ApplicationCommands.Delete; } }
            public static RoutedUICommand Refresh { get { return NavigationCommands.Refresh; } }
        }

        private readonly Presenter _presenter;

        public MainWindow()
        {
            InitializeComponent();
            InitializeCommandBindings();
            _presenter = new Presenter(this);
            _presenter.ShowAsync(_dataView);
        }

        string Presenter.IFilter.Text
        {
            get { return _textBoxSearch.Text; }
            set { _textBoxSearch.Text = value; }
        }

        private void InitializeCommandBindings()
        {
            CommandBindings.Add(new CommandBinding(Commands.New, New));
            CommandBindings.Add(new CommandBinding(Commands.Open, Open, CanOpen));
            CommandBindings.Add(new CommandBinding(Commands.Delete, Delete, CanDelete));
            CommandBindings.Add(new CommandBinding(Commands.Refresh, Refresh, CanRefresh));
        }

        private void ShowMovieDetailWindow(DataSet<Movie> movie)
        {
            Debug.Assert(movie.Count == 1);
            var result = MovieDetailWindow.Show(movie, this);
            if (result)
                Refresh(true);
        }

        private void New(object sender, ExecutedRoutedEventArgs e)
        {
            var movie = DataSet<Movie>.Create();
            movie.AddRow();
            ShowMovieDetailWindow(movie);
        }

        private Movie _
        {
            get { return _presenter._; }
        }

        private async void Open(object sender, ExecutedRoutedEventArgs e)
        {
            var ID = _presenter.CurrentRow.GetValue(_.ID).Value;
            var movie = await App.ExecuteAsync(db => db.GetMovieAsync(ID));
            ShowMovieDetailWindow(movie);
        }

        private void CanOpen(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _presenter.CurrentRow != null;
        }

        private async void Delete(object sender, ExecutedRoutedEventArgs e)
        {
            var selectedRows = _presenter.SelectedRows;
            var json = _presenter.DataSet.Filter(JsonFilter.PrimaryKeyOnly).ToJsonString(_presenter.SelectedDataRows, false);
            var keys = DataSet<Movie.Key>.ParseJson(json);
            await App.ExecuteAsync(db => db.Movie.DeleteAsync(keys, (s, _) => s.Match(_)));
            Refresh(false);
        }

        private void CanDelete(object sender, CanExecuteRoutedEventArgs e)
        {
            var selectedRows = _presenter.SelectedRows;
            e.CanExecute = selectedRows != null && selectedRows.Count > 0;
        }

        private void Refresh(object sender, ExecutedRoutedEventArgs e)
        {
            Refresh(false);
        }

        private void Refresh(bool clearFilter)
        {
            _presenter.RefreshAsync(clearFilter);
        }

        private void CanRefresh(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _presenter.DataSet != null;
        }
    }
}
