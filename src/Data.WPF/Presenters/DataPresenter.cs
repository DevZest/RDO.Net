using System.Collections.Generic;
using DevZest.Data.Presenters.Primitives;
using System;
using System.Windows.Controls;
using DevZest.Data.Views;
using DevZest.Data.Views.Primitives;
using System.Windows.Input;

namespace DevZest.Data.Presenters
{
    /// <summary>
    /// Base class to contain presentation logic for scalar data and DataSet.
    /// </summary>
    public abstract partial class DataPresenter : BasePresenter, IDataPresenter
    {
        /// <summary>
        /// Initializes a new instance of <see cref="DataPresenter"/> class.
        /// </summary>
        protected DataPresenter()
            : base()
        {
        }

        /// <summary>
        /// Gets the <see cref="DataView"/> that is attached to this <see cref="DataPresenter"/>.
        /// </summary>
        public new DataView View { get; private set; }

        internal sealed override IBaseView GetView()
        {
            return View;
        }

        internal sealed override void SetView(IBaseView value)
        {
            View = (DataView)value;
        }

        /// <summary>
        /// Gets the layout orientation that rows will be presented repeatedly.
        /// </summary>
        public Orientation? LayoutOrientation
        {
            get { return Template?.Orientation; }
        }

        /// <summary>
        /// Gets the source DataSet.
        /// </summary>
        public DataSet DataSet
        {
            get { return LayoutManager?.DataSet; }
        }

        /// <summary>
        /// Gets a value indicates the underlying rows are recursive tree structure.
        /// </summary>
        public bool IsRecursive
        {
            get { return LayoutManager == null ? false : LayoutManager.IsRecursive; }
        }

        /// <summary>
        /// Gets or sets the condition to filter the rows.
        /// </summary>
        public Predicate<DataRow> Where
        {
            get { return LayoutManager?.Where; }
            set { Apply(value, OrderBy); }
        }
        
        /// <summary>
        /// Gets or sets the comparer to sort the rows.
        /// </summary>
        public IComparer<DataRow> OrderBy
        {
            get { return LayoutManager?.OrderBy; }
            set { Apply(Where, value); }
        }

        /// <summary>
        /// Applies filtering condition and sorting comparer.
        /// </summary>
        /// <param name="where">The filtering condition.</param>
        /// <param name="orderBy">The sorting comparer.</param>
        public virtual void Apply(Predicate<DataRow> where, IComparer<DataRow> orderBy)
        {
            RequireLayoutManager();
            if (where != Where || OrderBy != orderBy)
                PerformApply(where, orderBy);
        }

        internal abstract void PerformApply(Predicate<DataRow> where, IComparer<DataRow> orderBy);

        /// <summary>
        /// Gets the collection <see cref="RowPresenter"/> objects.
        /// </summary>
        public IReadOnlyList<RowPresenter> Rows
        {
            get { return LayoutManager?.Rows; }
        }

        /// <summary>
        /// Gets or sets the current <see cref="RowPresenter"/>.
        /// </summary>
        public RowPresenter CurrentRow
        {
            get { return LayoutManager?.CurrentRow; }
            set
            {
                if (value == CurrentRow)
                    return;

                VerifyRowPresenter(value, nameof(value));
                var layoutManager = RequireLayoutManager();
                View.UpdateLayout();
                layoutManager.CurrentRow = value;
            }
        }

        /// <summary>
        /// Gets the current <see cref="ContainerView"/>, which is either <see cref="RowView"/> or <see cref="BlockView"/>.
        /// </summary>
        public ContainerView CurrentContainerView
        {
            get { return RequireLayoutManager().CurrentContainerView; }
        }

        /// <summary>
        /// Gets the value that specifies how many rows will flow in BlockView first, then expand afterwards.
        /// </summary>
        public int FlowRepeatCount
        {
            get { return LayoutManager == null ? 1 : LayoutManager.FlowRepeatCount; }
        }

        private void VerifyRowPresenter(RowPresenter value, string paramName, int index = -1)
        {
            if (value == null)
                throw new ArgumentNullException(GetParamName(paramName, index));

            if (value.DataPresenter != this || value.Index < 0)
                throw new ArgumentException(DiagnosticMessages.DataPresenter_InvalidRowPresenter, GetParamName(paramName, index));
        }

        private static string GetParamName(string paramName, int index)
        {
            return index == -1 ? paramName : string.Format("{0}[{1}]", paramName, index);
        }

        /// <summary>
        /// Selects specified <see cref="RowPresenter"/>.
        /// </summary>
        /// <param name="rowPresenter">The specified <see cref="RowPresenter"/>.</param>
        /// <param name="selectionMode">The selection mode.</param>
        /// <param name="ensureVisible">Indicates whether selected row must be visible.</param>
        /// <param name="beforeSelecting">A delegate will be invoked before selectinng.</param>
        public void Select(RowPresenter rowPresenter, SelectionMode selectionMode, bool ensureVisible = true, Action beforeSelecting = null)
        {
            VerifyRowPresenter(rowPresenter, nameof(rowPresenter));

            var oldCurrentRow = CurrentRow;
            beforeSelecting?.Invoke();
            SuspendInvalidateView();
            CurrentRow = rowPresenter;
            RequireLayoutManager().Select(rowPresenter, selectionMode, oldCurrentRow);
            ResumeInvalidateView();
            if (ensureVisible && Scrollable != null)
                Scrollable.EnsureCurrentRowVisible();
        }

        /// <summary>
        /// Selects multiple rows.
        /// </summary>
        /// <param name="rows">The multiple rows.</param>
        public void Select(params RowPresenter[] rows)
        {
            if (rows != null)
            {
                for (int i = 0; i < rows.Length; i++)
                    VerifyRowPresenter(rows[i], nameof(rows), i);
            }
            SuspendInvalidateView();
            RequireLayoutManager().Select(rows);
            InvalidateView();
            ResumeInvalidateView();
        }

        /// <summary>
        /// Gets the collection of selected rows.
        /// </summary>
        public IReadOnlyCollection<RowPresenter> SelectedRows
        {
            get { return LayoutManager?.SelectedRows; }
        }

        /// <summary>
        /// Gets the collection of selected DataRows.
        /// </summary>
        public IEnumerable<DataRow> SelectedDataRows
        {
            get
            {
                foreach (var row in SelectedRows)
                {
                    if (row.IsVirtual)
                        continue;
                    yield return row.DataRow;
                }
            }
        }

        /// <summary>
        /// Gets the virtual row for inserting indication.
        /// </summary>
        public RowPresenter VirtualRow
        {
            get { return LayoutManager?.VirtualRow; }
        }

        /// <summary>
        /// Gets a value indicates whether current row is in edit mode.
        /// </summary>
        public bool IsEditing
        {
            get { return LayoutManager == null ? false : LayoutManager.IsEditing; }
        }

        /// <summary>
        /// Gets the current <see cref="RowPresenter"/> which is in edit mode.
        /// </summary>
        public RowPresenter EditingRow
        {
            get { return CurrentRow != null && IsEditing ? CurrentRow : null; }
        }

        /// <summary>
        /// Gets a value indicates whether current row is in inserting mode.
        /// </summary>
        public bool IsInserting
        {
            get { return IsEditing && LayoutManager.CurrentRow == LayoutManager.VirtualRow; }
        }

        /// <summary>
        /// Gets the current <see cref="RowPresenter"/> which is in inserting mode.
        /// </summary>
        public RowPresenter InsertingRow
        {
            get { return IsInserting ? CurrentRow : null; }
        }

        /// <summary>
        /// Gets a value inidicates whether new row can be inserted.
        /// </summary>
        public bool CanInsert
        {
            get { return !IsEditing && RequireLayoutManager().DataSet.EditingRow == null; }
        }

        /// <summary>
        /// Begins inserting new row before specified row.
        /// </summary>
        /// <param name="row">The specified row. If <see langword="null"/>, insert at the beginning.</param>
        public void BeginInsertBefore(RowPresenter row = null)
        {
            VerifyInsert(row);
            SuspendInvalidateView();
            RequireLayoutManager().BeginInsertBefore(null, row);
            ResumeInvalidateView();
        }

        /// <summary>
        /// Begins inserting new row after specified row.
        /// </summary>
        /// <param name="row">The specified row. If <see langword="null"/>, insert at the end.</param>
        public void BeginInsertAfter(RowPresenter row = null)
        {
            VerifyInsert(row);
            SuspendInvalidateView();
            RequireLayoutManager().BeginInsertAfter(null, row);
            ResumeInvalidateView();
        }

        private void VerifyInsert(RowPresenter row)
        {
            if (!CanInsert)
                throw new InvalidOperationException(DiagnosticMessages.DataPresenter_VerifyCanInsert);
            if (row != null)
            {
                if (row.DataPresenter != this || row.Parent != null)
                    throw new ArgumentException(DiagnosticMessages.DataPresenter_InvalidRowPresenter, nameof(row));
            }
        }

        /// <summary>
        /// Gets the object that contains row validation logic.
        /// </summary>
        public RowValidation RowValidation
        {
            get { return LayoutManager?.RowValidation; }
        }

        /// <summary>
        /// Gets the object that contains layout scrolling logic.
        /// </summary>
        public IScrollable Scrollable
        {
            get { return LayoutManager as ScrollableManager; }
        }

        /// <summary>
        /// Toggles the expand state for specified <see cref="RowPresenter"/>.
        /// </summary>
        /// <param name="rowPresenter">The specified <see cref="RowPresenter"/>.</param>
        internal protected virtual void ToggleExpandState(RowPresenter rowPresenter)
        {
            rowPresenter.InternalToggleExpandState();
        }

        /// <summary>
        /// Gets a value indicates whether specified <see cref="RowPresenter"/> has child rows.
        /// </summary>
        /// <param name="rowPresenter">The specified <see cref="RowPresenter"/>.</param>
        /// <returns><see langword="true"/> if specified <see cref="RowPresenter"/> has child rows, otherwise <see langword="false"/>.</returns>
        internal protected virtual bool HasChildren(RowPresenter rowPresenter)
        {
            return rowPresenter.InternalHasChildren;
        }

        internal abstract void Reload();

        internal abstract bool CanReload { get; }

        internal abstract void CancelLoading();

        internal abstract bool CanCancelLoading { get; }

        /// <inheritdoc/>
        public override bool SubmitInput(bool focusToErrorInput = true)
        {
            RequireLayoutManager();

            if (IsEditing)
                CurrentRow.EndEdit();

            if (IsEditing)
                return false;

            RowValidation.Validate();
            ScalarValidation.Validate();
            if (!CanSubmitInput)
            {
                if (focusToErrorInput)
                    LayoutManager.FocusToInputError();
                return false;
            }

            return true;
        }

        /// <summary>
        /// Called when current row changes.
        /// </summary>
        /// <param name="oldValue">The old value of current row.</param>
        protected internal virtual void OnCurrentRowChanged(RowPresenter oldValue)
        {
        }

        /// <summary>
        /// Determines whether editing mode can be ended.
        /// </summary>
        /// <returns><see langword="true"/> if editing mode can be ended, otherwise <see langword="false"/>.</returns>
        /// <remarks>The default implementation validates data and returns <see langword="true"/> if there is no
        /// validation error.</remarks>
        protected internal virtual bool QueryEndEdit()
        {
            return RequireLayoutManager().QueryEndEdit();
        }

        /// <summary>
        /// Determines whether editing mode can be cancelled.
        /// </summary>
        /// <returns><see langword="true"/> if editing mode can be cancelled, otherwise <see langword="false"/>.</returns>
        /// <remarks>The default implementation always returns <see langword="true"/>.</remarks>
        protected internal virtual bool QueryCancelEdit()
        {
            return true;
        }

        /// <summary>
        /// Confirms end of editing mode.
        /// </summary>
        /// <returns><see langword="true"/> if end of editing mode confirmed, otherwise <see langword="false"/>.</returns>
        /// <remarks>The default implementation always returns <see langword="true"/>. Derived class
        /// can override this method to provide custom implementation such as displaying a confirmation dialog.</remarks>
        protected internal virtual bool ConfirmEndEdit()
        {
            return true;
        }

        #region IService

        DataPresenter IService.DataPresenter
        {
            get { return this; }
        }

        void IService.Initialize(DataPresenter dataPresenter)
        {
            throw new NotSupportedException();
        }
        #endregion

        /// <summary>
        /// Selects specified <see cref="RowPresenter"/> by mouse button click.
        /// </summary>
        /// <param name="row">The specified <see cref="RowPresenter"/>.</param>
        /// <param name="mouseButton">The mouse button click.</param>
        /// <param name="beforeSelecting">The action will be invoked before selecting.</param>
        public void Select(RowPresenter row, MouseButton mouseButton, Action beforeSelecting)
        {
            VerifyRowPresenter(row, nameof(row));

            if (EditingRow != null)
                return;

            var selectionMode = PredictSelectionMode(mouseButton, row);
            if (selectionMode.HasValue)
            {
                SuspendInvalidateView();
                Select(row, selectionMode.GetValueOrDefault(), true, beforeSelecting);
                ResumeInvalidateView();
            }
        }

        /// <summary>
        /// Predicates the selection mode by mouse button click.
        /// </summary>
        /// <param name="mouseButton">The mouse button click.</param>
        /// <param name="row">The <see cref="RowPresenter"/>.</param>
        /// <returns>The selection mode.</returns>
        protected virtual SelectionMode? PredictSelectionMode(MouseButton mouseButton, RowPresenter row)
        {
            switch (Template.SelectionMode)
            {
                case SelectionMode.Single:
                    return SelectionMode.Single;
                case SelectionMode.Multiple:
                    return SelectionMode.Multiple;
                case SelectionMode.Extended:
                    if (mouseButton != MouseButton.Left)
                    {
                        if (mouseButton == MouseButton.Right && (Keyboard.Modifiers & (ModifierKeys.Control | ModifierKeys.Shift)) == ModifierKeys.None)
                        {
                            if (row.IsSelected)
                                return null;
                            return SelectionMode.Single;
                        }
                        return null;
                    }

                    if (IsControlDown && IsShiftDown)
                        return null;

                    return IsShiftDown ? SelectionMode.Extended : (IsControlDown ? SelectionMode.Multiple : SelectionMode.Single);
            }
            return null;
        }

        private static bool IsControlDown
        {
            get { return (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control; }
        }

        private static bool IsShiftDown
        {
            get { return (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift; }
        }

        /// <summary>
        /// Confirms row deletion.
        /// </summary>
        /// <returns><see langword="true"/> if row deletion confirmed, otherwise <see langword="false"/>.</returns>
        /// <remarks>The default implementation always returns <see langword="true"/>. Derived class
        /// can override this method to provide custom implementation such as displaying a confirmation dialog.</remarks>
        protected internal virtual bool ConfirmDelete()
        {
            return true;
        }

        /// <summary>
        /// Gets the serializer for specified <see cref="Column"/>.
        /// </summary>
        /// <param name="column">The specified <see cref="Column"/>.</param>
        /// <returns>The <see cref="ColumnSerializer"/>.</returns>
        public virtual ColumnSerializer GetSerializer(Column column)
        {
            return ColumnSerializer.Create(column);
        }

        /// <summary>
        /// Gets the <see cref="RowPresenter"/> for specified <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dataRow">The specified <see cref="DataRow"/>.</param>
        /// <returns>The result <see cref="RowPresenter"/>.</returns>
        public RowPresenter this[DataRow dataRow]
        {
            get { return LayoutManager?[dataRow]; }
        }

        /// <summary>
        /// Invoked when column is being edit.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <remarks>There is no default implementation. Derived class can override this method to provide custom implemenation.</remarks>
        protected internal virtual void OnEdit(Column column)
        {
        }

        /// <summary>
        /// Gets a value indicates whether underlying rows can be matched by primary key value(s).
        /// </summary>
        public bool CanMatchRow
        {
            get { return LayoutManager == null ? false : LayoutManager.CanMatchRow; }
        }

        /// <summary>
        /// Matches underlying rows with provided primary key value(s) from other DataSet.
        /// </summary>
        /// <param name="columns">The columns of other DataSet's primary key.</param>
        /// <param name="dataRow">The DataRow of other DataSet.</param>
        /// <returns>The matched <see cref="RowPresenter"/>, <see langword="null"/> if no matched row.</returns>
        public RowPresenter Match(IReadOnlyList<Column> columns, DataRow dataRow = null)
        {
            columns.VerifyNotNull(nameof(columns));
            if (!CanMatchRow)
                return null;

            var valueHashCode = RowMatch.GetHashCode(columns, dataRow);
            if (!valueHashCode.HasValue)
                return null;
            return LayoutManager[new RowMatch(columns, dataRow, valueHashCode.Value)];
        }

        /// <summary>
        /// Matches underlying rows with provided <see cref="RowPresenter"/> from other data presenter.
        /// </summary>
        /// <param name="rowPresenter">The provided <see cref="RowPresenter"/> from other data presenter.</param>
        /// <param name="matchVirtual">Indicates whether virtual row should be included.</param>
        /// <returns>The matched <see cref="RowPresenter"/>, <see langword="null"/> if no matched row.</returns>
        public RowPresenter Match(RowPresenter rowPresenter, bool matchVirtual = true)
        {
            return LayoutManager?.Match(rowPresenter, matchVirtual);
        }

        /// <summary>
        /// Invoked when selection status of <see cref="RowPresenter"/> changed.
        /// </summary>
        /// <param name="row">The <see cref="RowPresenter"/>.</param>
        internal protected virtual void OnIsSelectedChanged(RowPresenter row)
        {
        }

        /// <summary>
        /// Gets the service.
        /// </summary>
        /// <typeparam name="T">The type of the service.</typeparam>
        /// <param name="autoCreate">Indicates whether the service should be created automatically.</param>
        /// <returns>The result service.</returns>
        public virtual T GetService<T>(bool autoCreate = true)
            where T : class, IService
        {
            return (this is T) ? (T)((object)this) : ServiceManager.GetService<T>(this, autoCreate);
        }

        /// <summary>
        /// Determines whether service exists.
        /// </summary>
        /// <typeparam name="T">The type of the service.</typeparam>
        /// <returns><see langword="true"/> if service exists, otherwise <see langword="false"/>.</returns>
        public bool ExistsService<T>()
            where T : class, IService
        {
            return GetService<T>(autoCreate: false) != null;
        }
    }
}
