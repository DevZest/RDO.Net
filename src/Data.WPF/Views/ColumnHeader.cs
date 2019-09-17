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
using System.ComponentModel;

namespace DevZest.Data.Views
{
    /// <summary>
    /// Represents a clickable header to identify, resize and sort column of data.
    /// </summary>
    [TemplateVisualState(GroupName = VisualStates.GroupCommon, Name = VisualStates.StateNormal)]
    [TemplateVisualState(GroupName = VisualStates.GroupCommon, Name = VisualStates.StateMouseOver)]
    [TemplateVisualState(GroupName = VisualStates.GroupCommon, Name = VisualStates.StatePressed)]
    [TemplateVisualState(GroupName = VisualStates.GroupSort, Name = VisualStates.StateUnsorted)]
    [TemplateVisualState(GroupName = VisualStates.GroupSort, Name = VisualStates.StateSortAscending)]
    [TemplateVisualState(GroupName = VisualStates.GroupSort, Name = VisualStates.StateSortDescending)]
    public class ColumnHeader : ButtonBase, IScalarElement
    {
        /// <summary>
        /// Customizable service to perform the sort operation.
        /// </summary>
        public interface ISortService : IService
        {
            /// <summary>
            /// Gets or sets the list of column comparer to perform the sort operation.
            /// </summary>
            IReadOnlyList<IColumnComparer> OrderBy { get; set; }
        }

        private sealed class SortService : ISortService, IReloadableService
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

        /// <summary>
        /// Contains commands implemented by <see cref="ColumnHeader"/> class.
        /// </summary>
        public abstract class Commands
        {
            /// <summary>
            /// Toggles sort direction for current column.
            /// </summary>
            public static readonly RoutedUICommand ToggleSortDirection = new RoutedUICommand(nameof(ToggleSortDirection), nameof(ToggleSortDirection), typeof(Commands));

            /// <summary>
            /// Displays a dialog via context menu to sort by specifying column(s).
            /// </summary>
            public static readonly RoutedUICommand Sort = new RoutedUICommand(UserMessages.ColumnHeaderCommands_SortCommandText, nameof(Sort), typeof(Commands));
        }

        /// <summary>
        /// Customizable service to provide command implementations.
        /// </summary>
        public interface ICommandService : IService
        {
            /// <summary>
            /// Retrieves command implementations for specified <see cref="DataView"/>.
            /// </summary>
            /// <param name="dataView">The specified <see cref="DataView"/>.</param>
            /// <returns>The retrieved command implementations.</returns>
            IEnumerable<CommandEntry> GetCommandEntries(DataView dataView);

            /// <summary>
            /// Retrieves command implementations for specified <see cref="ColumnHeader"/>.
            /// </summary>
            /// <param name="columnHeader">The specified <see cref="ColumnHeader"/>.</param>
            /// <returns>The retrieved command implementations.</returns>
            IEnumerable<CommandEntry> GetCommandEntries(ColumnHeader columnHeader);
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
                yield return Commands.Sort.Bind(ExecSort, CanExecSort);
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
                yield return Commands.ToggleSortDirection.Bind(ExecToggleSortDirection, CanExecToggleSortDirection, new MouseGesture(MouseAction.LeftClick));
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
                if (direction == null)
                    sortService.OrderBy = null;
                else
                    sortService.OrderBy = new IColumnComparer[] { DataRow.OrderBy(columnHeader.Column, direction.GetValueOrDefault() == ListSortDirection.Ascending ? Data.SortDirection.Ascending : Data.SortDirection.Descending) };
            }

            private static ListSortDirection? Toggle(ListSortDirection? direction)
            {
                if (direction == null)
                    return ListSortDirection.Ascending;
                else if (direction == ListSortDirection.Ascending)
                    return ListSortDirection.Descending;
                else
                    return null;
            }
        }

        private sealed class DragHandler : DragHandlerBase
        {
            private GridTrack _gridTrack;
            private double _resized;
            private GridLength[] _oldValues;

            public bool BeginDrag(ColumnHeader columnHeader, UIElement resizeGripper, MouseEventArgs e)
            {
                Debug.Assert(columnHeader != null);
                Debug.Assert(resizeGripper != null);
                _gridTrack = columnHeader.GridTrackToResize;
                if (_gridTrack == null)
                    return false;
                _oldValues = new GridLength[_gridTrack.Ordinal + 1];
                var gridTrackOwner = _gridTrack.Owner;
                for (int i = 0; i < _oldValues.Length - 1; i++)
                {
                    var current = gridTrackOwner[i];
                    _oldValues[i] = current.Length;
                    if (current.Length.IsStar)
                        current.Resize(new GridLength(current.MeasuredLength, GridUnitType.Pixel));
                }
                _oldValues[_oldValues.Length - 1] = _gridTrack.Length;
                DragDetect(resizeGripper, e);
                return true;
            }

            protected override void OnBeginDrag()
            {
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
                _gridTrack.Resize(new GridLength(newValue, GridUnitType.Pixel));
            }

            protected override void OnEndDrag(UIElement dragElement, bool abort)
            {
                if (abort)
                    _gridTrack.Resize(_oldValues[_oldValues.Length - 1]);

                var gridTrackOwner = _gridTrack.Owner;
                for (int i = 0; i < _oldValues.Length - 1; i++)
                {
                    var oldValue = _oldValues[i];
                    if (oldValue.IsStar)
                        gridTrackOwner[i].Resize(oldValue);
                }
            }
        }

        /// <summary>
        /// Identifies <see cref="CanSort"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CanSortProperty = DependencyProperty.Register(nameof(CanSort), typeof(bool),
            typeof(ColumnHeader), new FrameworkPropertyMetadata(BooleanBoxes.True));

        private static readonly DependencyPropertyKey SortDirectionPropertyKey = DependencyProperty.RegisterReadOnly(nameof(SortDirection), typeof(ListSortDirection?),
            typeof(ColumnHeader), new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Identifies <see cref="SortDirection"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SortDirectionProperty = SortDirectionPropertyKey.DependencyProperty;

        /// <summary>
        /// Identifies <see cref="SeparatorBrush"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SeparatorBrushProperty = DependencyProperty.Register(nameof(SeparatorBrush), typeof(Brush),
            typeof(ColumnHeader), new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Identifies <see cref="SeparatorVisibility"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SeparatorVisibilityProperty = DependencyProperty.Register(nameof(SeparatorVisibility), typeof(Visibility),
            typeof(ColumnHeader), new FrameworkPropertyMetadata(Visibility.Visible));

        /// <summary>
        /// Identifies <see cref="ResizeGripperVisibility"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ResizeGripperVisibilityProperty = DependencyProperty.Register(nameof(ResizeGripperVisibility), typeof(Visibility),
            typeof(ColumnHeader), new FrameworkPropertyMetadata(Visibility.Visible));

        /// <summary>
        /// Identifies IsReiszeGripper attached property (<see cref="GetIsResizeGripper(DependencyObject)"/>/<see cref="SetIsResizeGripper(DependencyObject, bool)"/>).
        /// </summary>
        /// <remarks>Resizing is implemented via IsResizeGripper attached property. In the control template of ColumnHeader,
        /// an <see cref="UIElement"/> with this attached property value set to <see langword="true"/> will detect the mouse drag-and-drop
        /// and perform the column resizing operation.</remarks>
        public static readonly DependencyProperty IsResizeGripperProperty = DependencyProperty.RegisterAttached("IsResizeGripper", typeof(bool),
            typeof(ColumnHeader), new FrameworkPropertyMetadata(BooleanBoxes.False));

        static ColumnHeader()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ColumnHeader), new FrameworkPropertyMetadata(typeof(ColumnHeader)));
            ServiceManager.Register<ISortService, SortService>();
            ServiceManager.Register<ICommandService, CommandService>();
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ColumnHeader"/>.
        /// </summary>
        public ColumnHeader()
        {
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdateVisualState(DataPresenter);
        }

        /// <summary>
        /// Gets or sets the associated column.
        /// </summary>
        public Column Column { get; set; }

        /// <summary>
        /// Gets or sets a value indicates whether column is sortable. This is a dependency property.
        /// </summary>
        public bool CanSort
        {
            get { return (bool)GetValue(CanSortProperty); }
            set { SetValue(CanSortProperty, BooleanBoxes.Box(value)); }
        }

        /// <summary>
        /// Gets the sort direction.
        /// </summary>
        public ListSortDirection? SortDirection
        {
            get { return (ListSortDirection?)GetValue(SortDirectionProperty); }
            private set { SetValue(SortDirectionPropertyKey, value); }
        }

        /// <summary>
        /// Gets or sets the brush to paint the separator. This is a dependency property.
        /// </summary>
        public Brush SeparatorBrush
        {
            get { return (Brush)GetValue(SeparatorBrushProperty); }
            set { SetValue(SeparatorBrushProperty, value); }
        }

        /// <summary>
        /// Gets or sets a value indicates the visibility of separator. This is a dependency property.
        /// </summary>
        public Visibility SeparatorVisibility
        {
            get { return (Visibility)GetValue(SeparatorVisibilityProperty); }
            set { SetValue(SeparatorVisibilityProperty, value); }
        }

        /// <summary>
        /// Gets or sets a value indicates the visibility of resize gripper. This is a dependency property.
        /// </summary>
        public Visibility ResizeGripperVisibility
        {
            get { return (Visibility)GetValue(ResizeGripperVisibilityProperty); }
            set { SetValue(ResizeGripperVisibilityProperty, value); }
        }

        /// <summary>
        /// Gets a value indicates whether specified object is resize gripper. This is the getter of IsResizeGripper attached property.
        /// </summary>
        /// <param name="obj">The specified object.</param>
        /// <returns><see langword="true"/> if specified object is resize gripper, otherwise <see langword="false"/>.</returns>
        /// <remarks>Resizing is implemented via IsResizeGripper attached property. In the control template of ColumnHeader,
        /// an <see cref="UIElement"/> with this attached property value set to <see langword="true"/> will detect the mouse drag-and-drop
        /// and perform the column resizing operation.</remarks>
        public static bool GetIsResizeGripper(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsResizeGripperProperty);
        }

        /// <summary>
        /// Sets a value indicates whether specified object is resize gripper. This is the setter of IsResizeGripper attached property.
        /// </summary>
        /// <param name="obj">The specified object..</param>
        /// <param name="value"><see langword="true"/> if specified object is resize gripper, otherwise <see langword="false"/>.</param>
        /// <remarks>Resizing is implemented via IsResizeGripper attached property. In the control template of ColumnHeader,
        /// an <see cref="UIElement"/> with this attached property value set to <see langword="true"/> will detect the mouse drag-and-drop
        /// and perform the column resizing operation.</remarks>
        public static void SetIsResizeGripper(DependencyObject obj, bool value)
        {
            obj.SetValue(IsResizeGripperProperty, BooleanBoxes.Box(value));
        }

        private ICommandService GetCommandService(DataPresenter dataPresenter)
        {
            return dataPresenter.GetService<ICommandService>();
        }

        void IScalarElement.Setup(ScalarPresenter p)
        {
            var dataPresenter = p.DataPresenter;
            var commandService = GetCommandService(dataPresenter);
            dataPresenter.View.SetupCommandEntries(commandService, GetCommandEntries);
            this.SetupCommandEntries(commandService, GetCommandEntries);
        }

        private static IEnumerable<CommandEntry> GetCommandEntries(ICommandService commandService, DataView dataView)
        {
            return commandService.GetCommandEntries(dataView);
        }

        private static IEnumerable<CommandEntry> GetCommandEntries(ICommandService commandService, ColumnHeader columnHeader)
        {
            return commandService.GetCommandEntries(columnHeader);
        }

        void IScalarElement.Cleanup(ScalarPresenter p)
        {
            this.CleanupCommandEntries();
        }

        void IScalarElement.Refresh(ScalarPresenter p)
        {
            UpdateVisualState(p.DataPresenter);
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
            if (SortDirection == ListSortDirection.Ascending)
                VisualStates.GoToState(this, useTransitions, VisualStates.StateSortAscending);
            else if (SortDirection == ListSortDirection.Descending)
                VisualStates.GoToState(this, useTransitions, VisualStates.StateSortDescending);
            else
                VisualStates.GoToState(this, useTransitions, VisualStates.StateUnsorted);
        }

        private DataPresenter DataPresenter
        {
            get { return DataView.GetCurrent(this)?.DataPresenter; }
        }

        private ListSortDirection? GetSortDirection(DataPresenter dataPresenter)
        {
            if (!CanSort)
                return null;

            var orderBy = dataPresenter.GetService<ISortService>()?.OrderBy;
            if (orderBy == null || orderBy.Count == 0)
                return null;

            for (int i = 0; i < orderBy.Count; i++)
            {
                var columnComparer = orderBy[i];
                if (columnComparer.GetColumn(dataPresenter.DataSet.Model) == Column)
                    return ToListSortDirection(columnComparer.Direction);
            }
            return null;
        }

        private static ListSortDirection? ToListSortDirection(SortDirection sortDirection)
        {
            if (sortDirection == Data.SortDirection.Unspecified)
                return null;
            else if (sortDirection == Data.SortDirection.Ascending)
                return ListSortDirection.Ascending;
            else
                return ListSortDirection.Descending;
        }

        private GridTrack GridTrackToResize
        {
            get { return this.GetBinding()?.GridRange.ColumnSpan.EndTrack; }
        }

        /// <inherited/>
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
            gridTrackToResize.Resize(GridLength.Auto);
            return true;
        }
    }
}
