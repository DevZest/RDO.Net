using DevZest.Data.Annotations;
using DevZest.Data.Presenters;
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
        private sealed class Model : Data.Model
        {
            static Model()
            {
                RegisterLocalColumn((Model _) => _.SortBy);
                RegisterLocalColumn((Model _) => _.Order);
            }

            [Display(Name = nameof(UserMessages.SortModel_SortBy), ResourceType = typeof(UserMessages))]
            public Column<ColumnHeader> SortBy { get; private set; }

            [Display(Name = nameof(UserMessages.SortModel_Order), ResourceType = typeof(UserMessages))]
            public Column<SortDirection> Order { get; private set; }

            [ModelValidator]
            private DataValidationError ValidateRequiredColumnHeader(DataRow dataRow)
            {
                return SortBy[dataRow] == null
                    ? new DataValidationError(UserMessages.SortModel_InputRequired(SortBy.DisplayName), SortBy)
                    : null;
            }

            [ModelValidator]
            private DataValidationError ValidateDuplicateColumnHeader(DataRow dataRow)
            {
                var dataSet = DataSet;
                foreach (var other in dataSet)
                {
                    if (other == dataRow)
                        continue;
                    if (SortBy[dataRow] == SortBy[other])
                        return new DataValidationError(UserMessages.SortModel_DuplicateSortBy, SortBy);
                }
                return null;
            }

            [ModelValidator]
            private DataValidationError ValidateDirection(DataRow dataRow)
            {
                return Order[dataRow] == SortDirection.Unspecified
                    ? new DataValidationError(UserMessages.SortModel_InputRequired(Order.DisplayName), Order)
                    : null;
            }

        }

        private sealed class Presenter : DataPresenter<Model>
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
                get { return GetColumnHeaders(_target).Select(x => new { Value = x, Display = x.Content }); }
            }

            protected override void BuildTemplate(TemplateBuilder builder)
            {
                builder
                    .GridColumns("20", "*", "100")
                    .GridRows("Auto", "Auto")
                    .Layout(Orientation.Vertical)
                    .WithVirtualRowPlacement(VirtualRowPlacement.Tail)
                    .AddBinding(1, 0, this.BindToTextBlock(_.SortBy.DisplayName))
                    .AddBinding(2, 0, this.BindToTextBlock(_.Order.DisplayName))
                    .AddBinding(0, 1, _.BindToRowHeader().WithStyle(RowHeader.Styles.Flat))
                    .AddBinding(1, 1, _.SortBy.BindToComboBox(ColumnHeaderSelection, "Value", "Display"))
                    .AddBinding(2, 1, _.Order.BindToComboBox(DirectionSelection, "Value", "Display"))
                    .WithRowViewCancelEditGestures(new KeyGesture(Key.Escape))
                    .WithRowViewEndEditGestures(new KeyGesture(Key.Enter));
            }
        }

        public static RoutedUICommand MoveUp { get { return ComponentCommands.MoveUp; } }
        public static RoutedUICommand MoveDown { get { return ComponentCommands.MoveDown; } }
        public static RoutedUICommand Delete { get { return ApplicationCommands.Delete; } }
        public static RoutedUICommand Apply { get; private set; } = new RoutedUICommand();
        public static RoutedUICommand Cancel { get { return ApplicationCommands.Close; } }

        private Presenter _presenter;
        private DataSet<Model> _data;

        public SortWindow()
        {
            InitializeComponent();
            CommandBindings.Add(new CommandBinding(MoveUp, ExecMoveUp, CanExecMoveUp));
            CommandBindings.Add(new CommandBinding(MoveDown, ExecMoveDown, CanExecMoveDown));
            CommandBindings.Add(new CommandBinding(Delete, ExecDelete, CanExecDelete));
            CommandBindings.Add(new CommandBinding(Apply, ExecApply, CanExecApply));
            CommandBindings.Add(new CommandBinding(Cancel, ExecCancel));
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

        private static DataSet<Model> GetData(ColumnHeader.ISortService sortService)
        {
            var result = DataSet<Model>.New();
            if (sortService == null)
                return result;
            var dataPresenter = sortService.DataPresenter;
            var orderBy = sortService.OrderBy;
            if (orderBy == null || orderBy.Count == 0)
                return result;

            var columnHeaders = GetColumnHeaders(dataPresenter);
            for (int i = 0; i < orderBy.Count; i++)
            {
                var orderByItem = orderBy[i];
                result.AddRow((_, dataRow) =>
                {
                    var column = orderByItem.GetColumn(dataPresenter.DataSet.Model);
                    _.SortBy[dataRow] = columnHeaders.First(x => x.Column == column);
                    _.Order[dataRow] = orderByItem.Direction;
                });
            }

            return result;
        }

        public void Show(DataPresenter target)
        {
            _data = GetData(target.GetService<ColumnHeader.ISortService>());
            _presenter = new Presenter(target);
            _presenter.Show(_dataView, _data);
            Show();
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
            sortService.OrderBy = GetOrderBy(_data);
            this.Close();
        }

        private void ExecCancel(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }

        private static IReadOnlyList<IColumnComparer> GetOrderBy(DataSet<Model> sortData)
        {
            if (sortData == null || sortData.Count == 0)
                return Array<IColumnComparer>.Empty;

            Debug.Assert(sortData.Validate().Count == 0);

            var _ = sortData._;
            var result = new IColumnComparer[sortData.Count];
            for (int i = 0; i < sortData.Count; i++)
            {
                var columnHeader = _.SortBy[i];
                var direction = _.Order[i];
                Debug.Assert(direction == SortDirection.Ascending || direction == SortDirection.Descending);
                result[i] = DataRow.OrderBy(columnHeader.Column, direction);
            }

            return result;
        }
    }
}
