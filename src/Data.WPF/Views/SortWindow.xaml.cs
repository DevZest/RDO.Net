using DevZest.Data.Presenters;
using DevZest.Data.Views.Primitives;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DevZest.Data.Views
{
    /// <summary>
    /// Interaction logic for SortWindow.xaml
    /// </summary>
    internal partial class SortWindow : Window
    {
        private sealed class Presenter : DataPresenter<Sorting>
        {
            public Presenter(DataPresenter target)
            {
                _target = target;
            }

            private DataPresenter _target;
            public DataPresenter Target
            {
                get { return _target; }
            }

            private IEnumerable DirectionSelection
            {
                get
                {
                    return new[]
                    {
                        new { Value = SortDirection.Ascending, Display = UserMessages.SortWindow_EnumAscending },
                        new { Value = SortDirection.Descending, Display = UserMessages.SortWindow_EnumDescending }
                    };
                }
            }

            private IEnumerable ColumnHeaderSelection
            {
                get { return GetColumnHeaders(_target).Select(x => new { Value = x.Column, Display = x.Content }); }
            }

            protected override void BuildTemplate(TemplateBuilder builder)
            {
                builder
                    .GridColumns("20", "*", "100")
                    .GridRows("Auto", "Auto")
                    .Layout(Orientation.Vertical)
                    .WithVirtualRowPlacement(VirtualRowPlacement.Tail)
                    .AddBinding(1, 0, this.BindToTextBlock(_.Column.DisplayName))
                    .AddBinding(2, 0, this.BindToTextBlock(_.Direction.DisplayName))
                    .AddBinding(0, 1, _.BindToRowHeader().WithStyle(RowHeader.Styles.Flat))
                    .AddBinding(1, 1, _.Column.BindToComboBox(ColumnHeaderSelection, "Value", "Display"))
                    .AddBinding(2, 1, _.Direction.BindToComboBox(DirectionSelection, "Value", "Display"));
            }
        }

        public static RoutedUICommand MoveUp { get { return ComponentCommands.MoveUp; } }
        public static RoutedUICommand MoveDown { get { return ComponentCommands.MoveDown; } }
        public static RoutedUICommand Delete { get { return ApplicationCommands.Delete; } }
        public static RoutedUICommand Apply { get; private set; } = new RoutedUICommand();
        public static RoutedUICommand Cancel { get { return ApplicationCommands.Close; } }

        private Presenter _presenter;
        private DataSet<Sorting> Sortings
        {
            get { return _presenter.DataSet; }
        }

        public SortWindow()
        {
            InitializeComponent();
            CommandBindings.Add(new CommandBinding(MoveUp, ExecMoveUp, CanExecMoveUp));
            CommandBindings.Add(new CommandBinding(MoveDown, ExecMoveDown, CanExecMoveDown));
            CommandBindings.Add(new CommandBinding(Delete, ExecDelete, CanExecDelete));
            CommandBindings.Add(new CommandBinding(Apply, ExecApply, CanExecApply));
        }

        private static IReadOnlyList<ColumnHeader> GetColumnHeaders(DataPresenter dataPresenter)
        {
            List<ColumnHeader> result = null;
            var bindings = dataPresenter.Template.ScalarBindings;
            for (int i = 0; i < bindings.Count; i++)
            {
                var columnHeader = bindings[i][0] as ColumnHeader;
                if (columnHeader != null && columnHeader.Column != null && columnHeader.CanSort)
                {
                    if (result == null)
                        result = new List<ColumnHeader>();
                    result.Add(columnHeader);
                }
            }

            if (result == null)
                return Array<ColumnHeader>.Empty;
            else
                return result;
        }

        private static DataSet<Sorting> GetSortings(ColumnHeader.ISortService sortService)
        {
            Debug.Assert(sortService != null);
            return Sorting.Convert(sortService.DataPresenter.DataSet.Model, sortService.OrderBy);
        }

        public void Show(DataPresenter target)
        {
            var sortings = GetSortings(target.GetService<ColumnHeader.ISortService>());
            _presenter = new Presenter(target);
            _presenter.Show(_dataView, sortings);
            ShowDialog();
        }

        private void CanExecMoveUp(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = CanMoveUp;
        }

        private bool CanMoveUp
        {
            get { return _presenter.CurrentRow != null && !_presenter.CurrentRow.IsVirtual && _presenter.CurrentRow.Index > 0; }
        }

        private void ExecMoveUp(object sender, ExecutedRoutedEventArgs e)
        {
            _presenter.CurrentRow.DataRow.Move(-1);
        }

        private void CanExecMoveDown(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = CanMoveDown;
        }

        private bool CanMoveDown
        {
            get { return _presenter.CurrentRow != null && !_presenter.CurrentRow.IsVirtual && _presenter.CurrentRow.Index < _presenter.DataSet.Count - 1; }
        }

        private void ExecMoveDown(object sender, ExecutedRoutedEventArgs e)
        {
            _presenter.CurrentRow.DataRow.Move(1);
        }

        private void CanExecDelete(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = CanDelete;
        }

        private bool CanDelete
        {
            get { return _presenter.CurrentRow != null && !_presenter.CurrentRow.IsVirtual; }
        }

        private void ExecDelete(object sender, ExecutedRoutedEventArgs e)
        {
            _presenter.CurrentRow.Delete();
        }

        private void CanExecApply(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = CanApply;
        }

        private bool CanApply
        {
            get { return _presenter.CanSubmitInput; }
        }

        private void ExecApply(object sender, ExecutedRoutedEventArgs e)
        {
            bool success = _presenter.SubmitInput();
            if (!success)
                return;
            var target = _presenter.Target;
            var sortService = target.GetService<ColumnHeader.ISortService>();
            sortService.OrderBy = Sorting.Convert(Sortings);
            this.Close();
        }
    }
}
