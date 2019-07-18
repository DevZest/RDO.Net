using DevZest.Data;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace Movies.WPF
{
    public partial class MovieDetailWindow : Window
    {
        public static class Commands
        {
            public static readonly RoutedCommand Submit = new RoutedCommand(nameof(Submit), typeof(MovieDetailWindow));
        }

        public static bool Show(DataSet<Movie> data, Window ownerWindow)
        {
            return new MovieDetailWindow().ShowDialog(data, ownerWindow);
        }

        public MovieDetailWindow()
        {
            InitializeComponent();
            CommandBindings.Add(new CommandBinding(Commands.Submit, ExecSubmit, CanExecSubmit));
        }

        private Presenter _presenter;

        private async void ExecSubmit(object sender, ExecutedRoutedEventArgs e)
        {
            if (_presenter.IsEditing)
                _presenter.CurrentRow.EndEdit();

            if (!_presenter.SubmitInput())
                return;

            await _presenter.SaveToDbAsync();
            DialogResult = true;
            Close();
        }

        private void CanExecSubmit(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _presenter.CanSubmitInput;
        }

        private bool ShowDialog(DataSet<Movie> data, Window ownerWindow)
        {
            Debug.Assert(data.Count == 1);
            _presenter = new Presenter();
            _presenter.Show(_dataView, data);
            Owner = ownerWindow;
            Title = _presenter.IsNew ? "New Movie" : string.Format("Movie: {0}", _presenter.ID);
            return ShowDialog().Value;
        }
    }
}
