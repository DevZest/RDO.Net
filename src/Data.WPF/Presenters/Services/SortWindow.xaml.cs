using DevZest.Data.Views;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DevZest.Data.Presenters.Services
{
    /// <summary>
    /// Interaction logic for SortWindow.xaml
    /// </summary>
    internal partial class SortWindow : Window
    {
        private sealed class Model : Data.Model
        {
            public Column<ColumnHeader> ColumnHeader { get; private set; }
            public Column<SortDirection> Direction { get; private set; }

            protected override void OnInitializing()
            {
                ColumnHeader = CreateLocalColumn<ColumnHeader>(builder => builder.DisplayName = UIText.SortModel_Column);
                Direction = CreateLocalColumn<SortDirection>(builder => builder.DisplayName = UIText.SortModel_Direction);
                base.OnInitializing();
            }

            protected override IValidationMessageGroup Validate(DataRow dataRow, ValidationSeverity? severity)
            {
                var result = base.Validate(dataRow, severity);
                if (severity == ValidationSeverity.Warning)
                    return result;

                if (ColumnHeader[dataRow] == null)
                    result = result.Add(new ValidationMessage(null, ValidationSeverity.Error, UIText.SortModel_InputRequired(ColumnHeader.DisplayName), ColumnHeader));
                if (Direction[dataRow] == SortDirection.Unspecified)
                    result = result.Add(new ValidationMessage(null, ValidationSeverity.Error, UIText.SortModel_InputRequired(Direction.DisplayName), Direction));

                return result;
            }

            public static void Apply(ISortService sortService, DataSet<Model> sortData)
            {
                sortService.OrderBy = GetOrderBy(sortData);
            }

            private static IReadOnlyList<IColumnComparer> GetOrderBy(DataSet<Model> sortData)
            {
                if (sortData == null || sortData.Count == 0)
                    return Array<IColumnComparer>.Empty;

                Debug.Assert(sortData.Validate(ValidationSeverity.Error).Count == 0);

                var _ = sortData._;
                var result = new IColumnComparer[sortData.Count];
                for (int i = 0; i < sortData.Count; i++)
                {
                    var columnHeader = _.ColumnHeader[i];
                    var direction = _.Direction[i];
                    Debug.Assert(direction == SortDirection.Ascending || direction == SortDirection.Descending);
                    result[i] = DataRow.OrderBy(columnHeader.Column, direction);
                }

                return result;
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
                        new { Value = SortDirection.Ascending, Display = "Ascending" },
                        new { Value = SortDirection.Descending, Display = "Descending" }
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
                    .AddBinding(1, 0, "Sort By".AsScalarTextBlock())
                    .AddBinding(2, 0, "Order".AsScalarTextBlock())
                    .AddBinding(0, 1, _.AsRowHeader())
                    .AddBinding(1, 1, _.ColumnHeader.AsComboBox(ColumnHeaderSelection, "Value", "Display"))
                    .AddBinding(2, 1, _.Direction.AsComboBox(DirectionSelection, "Value", "Display"));
            }
        }

        public static RoutedUICommand MoveUp { get { return ComponentCommands.MoveUp; } }
        public static RoutedUICommand MoveDown { get { return ComponentCommands.MoveDown; } }
        public static RoutedUICommand Delete { get { return ApplicationCommands.Delete; } }
        public static RoutedUICommand Apply { get; private set; } = new RoutedUICommand();

        private Presenter _presenter;
        private DataSet<Model> _data;

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

        private static DataSet<Model> GetData(ISortService sortService)
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
                    _.ColumnHeader[dataRow] = columnHeaders.First(x => x.Column == column);
                    _.Direction[dataRow] = orderByItem.Direction;
                });
            }

            return result;
        }

        public void Show(DataPresenter target)
        {
            _data = GetData(target.GetService<ISortService>());
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
            get { return true; }
        }

        private void ExecApply(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }
    }
}
