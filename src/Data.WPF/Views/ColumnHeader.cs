using DevZest.Data.Presenters;
using DevZest.Data.Presenters.Primitives;
using DevZest.Windows;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System;
using System.Collections.Generic;

namespace DevZest.Data.Views
{
    [TemplateVisualState(GroupName = VisualStates.GroupCommon, Name = VisualStates.StateNormal)]
    [TemplateVisualState(GroupName = VisualStates.GroupCommon, Name = VisualStates.StateMouseOver)]
    [TemplateVisualState(GroupName = VisualStates.GroupCommon, Name = VisualStates.StatePressed)]
    [TemplateVisualState(GroupName = VisualStates.GroupSort, Name = VisualStates.StateUnsorted)]
    [TemplateVisualState(GroupName = VisualStates.GroupSort, Name = VisualStates.StateSortAscending)]
    [TemplateVisualState(GroupName = VisualStates.GroupSort, Name = VisualStates.StateSortDescending)]
    public class ColumnHeader : ButtonBase, IScalarElement
    {
        public interface ISortService : IService
        {
            IReadOnlyList<IColumnComparer> OrderBy { get; set; }
        }

        private sealed class SortService : ISortService
        {
            public DataPresenter DataPresenter { get; private set; }

            public void Initialize(DataPresenter dataPresenter)
            {
                DataPresenter = dataPresenter;
            }

            private IReadOnlyList<IColumnComparer> _orderBy;
            public IReadOnlyList<IColumnComparer> OrderBy
            {
                get { return _orderBy; }
                set
                {
                    _orderBy = value;
                    DataPresenter.OrderBy = GetOrderBy(_orderBy);
                }
            }

            private static IComparer<DataRow> GetOrderBy(IReadOnlyList<IColumnComparer> orderBy)
            {
                if (orderBy == null || orderBy.Count == 0)
                    return null;

                IDataRowComparer result = orderBy[0];
                for (int i = 1; i < orderBy.Count; i++)
                    result = result.ThenBy(orderBy[i]);

                return result;
            }
        }

        public static readonly RoutedUICommand ToggleSortDirectionCommand = new RoutedUICommand(nameof(ToggleSortDirectionCommand), nameof(ToggleSortDirectionCommand), typeof(CommandService));
        public static readonly RoutedUICommand SortCommand = new RoutedUICommand(UIText.ColumnHeaderCommands_SortCommandText, nameof(SortCommand), typeof(CommandService));

        private interface ICommandManager : IService
        {
            void EnsureCommandEntriesSetup(DataView dataView);
        }

        public interface ICommandService : IService
        {
            IEnumerable<CommandEntry> GetCommandEntries(DataView dataView);
            IEnumerable<CommandEntry> GetCommandEntries(ColumnHeader columnHeader);
        }

        private sealed class CommandManager : ICommandManager
        {
            private DataPresenter _dataPresenter;
            public DataPresenter DataPresenter
            {
                get { return _dataPresenter; }
            }

            public void Initialize(DataPresenter dataPresenter)
            {
                _dataPresenter = dataPresenter;
                dataPresenter.ViewChanged += OnViewChanged;
            }

            private void OnViewChanged(object sender, EventArgs e)
            {
                _dataViewCommandEntriesSetup = false;
            }

            private bool _dataViewCommandEntriesSetup = false;
            public void EnsureCommandEntriesSetup(DataView dataView)
            {
                if (_dataViewCommandEntriesSetup)
                    return;

                var commandService = _dataPresenter.GetService<ICommandService>();
                dataView.SetupCommandEntries(commandService.GetCommandEntries(dataView));
                _dataViewCommandEntriesSetup = true;
            }
        }

        private sealed class CommandService : ICommandService
        {
            public DataPresenter DataPresenter { get; private set; }

            public void Initialize(DataPresenter dataPresenter)
            {
                DataPresenter = dataPresenter;
            }

            public IEnumerable<CommandEntry> GetCommandEntries(DataView dataView)
            {
                yield return SortCommand.CommandBinding(ExecSort, CanExecSort);
            }

            private void CanExecSort(object sender, CanExecuteRoutedEventArgs e)
            {
                e.CanExecute = CanShowSortWindow;
            }

            private bool CanShowSortWindow
            {
                get
                {
                    var bindings = DataPresenter.Template.ScalarBindings;
                    for (int i = 0; i < bindings.Count; i++)
                    {
                        var columnHeader = bindings[i][0] as ColumnHeader;
                        if (columnHeader != null && columnHeader.Column != null && columnHeader.CanSort)
                            return true;
                    }
                    return false;
                }
            }

            private void ExecSort(object sender, ExecutedRoutedEventArgs e)
            {
                var dataView = (DataView)sender;
                var sortWindow = new SortWindow();
                var positionFromScreen = dataView.PointToScreen(new Point(0, 0));
                PresentationSource source = PresentationSource.FromVisual(dataView);
                Point targetPoints = source.CompositionTarget.TransformFromDevice.Transform(positionFromScreen);

                sortWindow.Top = targetPoints.Y + Math.Max(0, (dataView.ActualHeight - sortWindow.Height) / 3);
                sortWindow.Left = targetPoints.X + Math.Max(0, (dataView.ActualWidth - sortWindow.Width) / 3);
                sortWindow.Show(dataView.DataPresenter);
            }

            public IEnumerable<CommandEntry> GetCommandEntries(ColumnHeader columnHeader)
            {
                yield return ToggleSortDirectionCommand.InputBinding(ExecToggleSortDirection, CanExecToggleSortDirection, new MouseGesture(MouseAction.LeftClick));
            }

            private void CanExecToggleSortDirection(object sender, CanExecuteRoutedEventArgs e)
            {
                var columnHeader = (ColumnHeader)sender;
                e.CanExecute = columnHeader.Column != null && columnHeader.CanSort;
            }

            private void ExecToggleSortDirection(object sender, ExecutedRoutedEventArgs e)
            {
                var columnHeader = (ColumnHeader)sender;
                var direction = Toggle(columnHeader.SortDirection);
                var sortService = DataPresenter.GetService<ISortService>();
                if (direction == SortDirection.Unspecified)
                    sortService.OrderBy = null;
                else
                    sortService.OrderBy = new IColumnComparer[] { DataRow.OrderBy(columnHeader.Column, direction) };
            }

            private static SortDirection Toggle(SortDirection direction)
            {
                if (direction == SortDirection.Unspecified)
                    return SortDirection.Ascending;
                else if (direction == SortDirection.Ascending)
                    return SortDirection.Descending;
                else
                    return SortDirection.Unspecified;
            }
        }

        private sealed class DragHandler : DragHandlerBase
        {
            private GridTrack _gridTrack;
            private double _resized;
            private GridLength _oldValue;

            public bool BeginDrag(ColumnHeader columnHeader, UIElement resizeGripper, MouseEventArgs e)
            {
                Debug.Assert(columnHeader != null);
                Debug.Assert(resizeGripper != null);
                _gridTrack = columnHeader.GridTrackToResize;
                if (_gridTrack == null)
                    return false;
                DragDetect(resizeGripper, e);
                return true;
            }

            protected override void OnBeginDrag()
            {
                _oldValue = _gridTrack.Length;
                _resized = _gridTrack.MeasuredLength;
            }

            private double MinLength
            {
                get { return _gridTrack.MinLength; }
            }

            private double MaxLength
            {
                get { return _gridTrack.MaxLength; }
            }

            protected override void OnDragDelta()
            {
                var newValue = _resized = _resized + MouseDeltaX;
                newValue = Math.Max(newValue, MinLength);
                newValue = Math.Min(newValue, MaxLength);
                _resized += newValue - _resized;
                _gridTrack.Length = new GridLength(newValue, GridUnitType.Pixel);
            }

            protected override void OnEndDrag(UIElement dragElement, bool abort)
            {
                if (abort)
                    _gridTrack.Length = _oldValue;
            }
        }

        public static readonly DependencyProperty CanSortProperty = DependencyProperty.Register(nameof(CanSort), typeof(bool),
            typeof(ColumnHeader), new FrameworkPropertyMetadata(BooleanBoxes.True));

        private static readonly DependencyPropertyKey SortDirectionPropertyKey = DependencyProperty.RegisterReadOnly(nameof(SortDirection), typeof(SortDirection),
            typeof(ColumnHeader), new FrameworkPropertyMetadata(SortDirection.Unspecified));

        public static readonly DependencyProperty SortDirectionProperty = SortDirectionPropertyKey.DependencyProperty;

        public static readonly DependencyProperty SeparatorBrushProperty = DependencyProperty.Register(nameof(SeparatorBrush), typeof(Brush),
            typeof(ColumnHeader), new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty SeparatorVisibilityProperty = DependencyProperty.Register(nameof(SeparatorVisibility), typeof(Visibility),
            typeof(ColumnHeader), new FrameworkPropertyMetadata(Visibility.Visible));

        public static readonly DependencyProperty ResizeGripperVisibilityProperty = DependencyProperty.Register(nameof(ResizeGripperVisibility), typeof(Visibility),
            typeof(ColumnHeader), new FrameworkPropertyMetadata(Visibility.Visible));

        public static readonly DependencyProperty IsResizeGripperProperty = DependencyProperty.RegisterAttached("IsResizeGripper", typeof(bool),
            typeof(ColumnHeader), new FrameworkPropertyMetadata(BooleanBoxes.False));

        static ColumnHeader()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ColumnHeader), new FrameworkPropertyMetadata(typeof(ColumnHeader)));
            ServiceManager.Register<ISortService, SortService>();
            ServiceManager.Register<ICommandManager, CommandManager>();
            ServiceManager.Register<ICommandService, CommandService>();
        }

        public ColumnHeader()
        {
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdateVisualState(DataPresenter);
        }

        public Column Column { get; set; }

        public bool CanSort
        {
            get { return (bool)GetValue(CanSortProperty); }
            set { SetValue(CanSortProperty, BooleanBoxes.Box(value)); }
        }

        public SortDirection SortDirection
        {
            get { return (SortDirection)GetValue(SortDirectionProperty); }
            private set { SetValue(SortDirectionPropertyKey, value); }
        }

        public Brush SeparatorBrush
        {
            get { return (Brush)GetValue(SeparatorBrushProperty); }
            set { SetValue(SeparatorBrushProperty, value); }
        }

        public Visibility SeparatorVisibility
        {
            get { return (Visibility)GetValue(SeparatorVisibilityProperty); }
            set { SetValue(SeparatorVisibilityProperty, value); }
        }

        public Visibility ResizeGripperVisibility
        {
            get { return (Visibility)GetValue(ResizeGripperVisibilityProperty); }
            set { SetValue(ResizeGripperVisibilityProperty, value); }
        }

        public static bool GetIsResizeGripper(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsResizeGripperProperty);
        }

        public static void SetIsResizeGripper(DependencyObject obj, bool value)
        {
            obj.SetValue(IsResizeGripperProperty, BooleanBoxes.Box(value));
        }

        void IScalarElement.Cleanup(ScalarPresenter scalarPresenter)
        {
        }

        void IScalarElement.Refresh(ScalarPresenter scalarPresenter)
        {
            UpdateVisualState(scalarPresenter.DataPresenter);
        }

        private void UpdateVisualState(DataPresenter dataPresenter)
        {
            UpdateVisualState(dataPresenter, true);
        }

        private void UpdateVisualState(DataPresenter dataPresenter, bool useTransitions)
        {
            SortDirection = GetSortDirection(dataPresenter);
            if (!IsLoaded)  // First call of VisualStateManager.GotoState must after control loaded.
                return;
            if (SortDirection == SortDirection.Ascending)
                VisualStates.GoToState(this, useTransitions, VisualStates.StateSortAscending);
            else if (SortDirection == SortDirection.Descending)
                VisualStates.GoToState(this, useTransitions, VisualStates.StateSortDescending);
            else
                VisualStates.GoToState(this, useTransitions, VisualStates.StateUnsorted);
        }

        private DataPresenter DataPresenter
        {
            get { return DataView.GetCurrent(this)?.DataPresenter; }
        }

        private SortDirection GetSortDirection(DataPresenter dataPresenter)
        {
            if (!CanSort)
                return SortDirection.Unspecified;

            var orderBy = dataPresenter.GetService<ISortService>()?.OrderBy;
            if (orderBy == null || orderBy.Count == 0)
                return SortDirection.Unspecified;

            for (int i = 0; i < orderBy.Count; i++)
            {
                var columnComparer = orderBy[i];
                if (columnComparer.GetColumn(dataPresenter.DataSet.Model) == Column)
                    return columnComparer.Direction;
            }
            return SortDirection.Unspecified;
        }

        void IScalarElement.Setup(ScalarPresenter scalarPresenter)
        {
            var dataPresenter = scalarPresenter.DataPresenter;
            var commandService = dataPresenter.GetService<ICommandService>();
            EnsureCommandEntriesSetup(commandService);
            var commandManager = dataPresenter.GetService<ICommandManager>();
            commandManager.EnsureCommandEntriesSetup(dataPresenter.View);
        }

        private bool _commandEntriesSetup;
        private void EnsureCommandEntriesSetup(ICommandService commandService)
        {
            if (_commandEntriesSetup)
                return;

            this.SetupCommandEntries(commandService.GetCommandEntries(this));
            _commandEntriesSetup = true;
        }

        private GridTrack GridTrackToResize
        {
            get { return this.GetBinding()?.GridRange.ColumnSpan.EndTrack; }
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);
            if (e.Handled)
                return;

            var resizeGripper = e.OriginalSource as UIElement;
            if (resizeGripper == null || !GetIsResizeGripper(resizeGripper))
                return;

            if (e.ClickCount == 1)
                e.Handled = new DragHandler().BeginDrag(this, resizeGripper, e);
            else if (e.ClickCount == 2)
                e.Handled = AutoResize();
        }

        private bool AutoResize()
        {
            var gridTrackToResize = GridTrackToResize;
            if (gridTrackToResize == null)
                return false;
            gridTrackToResize.Length = GridLength.Auto;
            return true;
        }
    }
}
