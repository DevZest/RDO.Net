using DevZest.Data.Views;
using DevZest.Data.Presenters.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Presenters
{
    /// <summary>
    /// Contains presentation logic of <see cref="DataRow"/>.
    /// </summary>
    public sealed class RowPresenter : ElementPresenter
    {
        /// <summary>
        /// Occurs when data value changed.
        /// </summary>
        public event EventHandler<ValueChangedEventArgs> ValueChanged = delegate { };

        internal RowPresenter(RowMapper rowMapper, DataRow dataRow)
            : this(rowMapper, dataRow, -1)
        {
        }

        internal RowPresenter(RowMapper rowMapper, int rawIndex)
            : this(rowMapper, null, rawIndex)
        {
        }

        private RowPresenter(RowMapper rowMapper, DataRow dataRow, int rawIndex)
        {
            Debug.Assert(rowMapper != null);
            _rowMapper = rowMapper;
            _dataRow = dataRow;
            RawIndex = rawIndex;
            RefreshMatchValueHashCode();
        }

        bool _isDisposing;
        internal void Dispose()
        {
            // Two phase dispose to allow UI element cleanup correctly.
            if (!_isDisposing)
            {
                Debug.Assert(_rowMapper != null);
                _isDisposing = true;
            }
            else
            {
                MatchValueHashCode = null;
                _rowMapper = null;
                Parent = null;
                _isDisposing = false;
            }
        }

        internal bool IsDisposed
        {
            get { return _isDisposing || _rowMapper == null; }
        }

        private void VerifyNotDisposed()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(GetType().FullName);
        }

        private RowMapper _rowMapper;

        internal RowMapper RowMapper
        {
            get
            {
                VerifyNotDisposed();
                return _rowMapper;
            }
        }

        private RowNormalizer RowNormalizer
        {
            get { return RowMapper as RowNormalizer; }
        }

        internal RowManager RowManager
        {
            get { return RowMapper as RowManager; }
        }

        internal InputManager InputManager
        {
            get { return RowMapper as InputManager; }
        }

        internal ElementManager ElementManager
        {
            get { return RowMapper as ElementManager; }
        }

        internal LayoutManager LayoutManager
        {
            get { return RowMapper as LayoutManager; }
        }

        private bool IsRecursive
        {
            get { return !IsVirtual && Template.IsRecursive; }
        }

        /// <inheritdoc/>
        public sealed override Template Template
        {
            get { return _rowMapper?.Template; }
        }

        private DataRow _dataRow;
        /// <summary>
        /// Gets the <see cref="DataRow"/>.
        /// </summary>
        public DataRow DataRow
        {
            get { return _dataRow; }
            internal set
            {
                if (_dataRow == value)
                    return;

                _dataRow = value;
                RefreshMatchValueHashCode();
            }
        }

        /// <summary>
        /// Gets a value indicates whether this row is virtual for inserting.
        /// </summary>
        public bool IsVirtual
        {
            get { return RowManager == null ? false : (RowManager.VirtualRow == this || RowManager.InsertingRow == this); }
        }

        /// <summary>
        /// Gets a value indicates whether this row is in editing mode.
        /// </summary>
        public bool IsEditing
        {
            get { return RowManager == null ? false : (RowManager.CurrentRow == this && RowManager.IsEditing); }
        }

        /// <summary>
        /// Gets a value indicates whether this row is in inserting mode.
        /// </summary>
        public bool IsInserting
        {
            get { return IsVirtual && IsEditing; }
        }

        /// <summary>
        /// Gets the parent row presenter.
        /// </summary>
        public RowPresenter Parent { get; internal set; }

        internal bool IsDescendantOf(RowPresenter rowPresenter)
        {
            if (rowPresenter == null)
                return false;

            for (var parent = Parent; parent != null; parent = parent.Parent)
            {
                if (parent == rowPresenter)
                    return true;
            }
            return false;
        }

        private List<RowPresenter> _children;

        /// <summary>
        /// Gets the child row presenters.
        /// </summary>
        public IReadOnlyList<RowPresenter> Children
        {
            get
            {
                if (_children != null)
                    return _children;
                return Array.Empty<RowPresenter>();
            }
        }

        internal void InitializeChildren()
        {
            Debug.Assert(IsRecursive);

            _children = _rowMapper.GetOrCreateChildren(this);
            foreach (var child in Children)
                child.InitializeChildren();
        }

        internal void InsertChild(int index, RowPresenter child)
        {
            Debug.Assert(index >= 0 && index <= Children.Count);
            if (_children == null)
                _children = new List<RowPresenter>();
            _children.Insert(index, child);
        }

        internal void RemoveChild(int index)
        {
            Debug.Assert(index >= 0 && index < Children.Count);
            _children.RemoveAt(index);
            if (_children.Count == 0)
                _children = null;
        }

        internal int RawIndex { get; set; }

        /// <summary>
        /// Gets the index.
        /// </summary>
        public int Index
        {
            get
            {
                if (IsDisposed)
                    return -1;

                if (IsVirtual)
                    return RawIndex;

                var result = RawIndex;

                var virtualRow = RowManager.VirtualRow;
                if (virtualRow != null && result >= virtualRow.Index)
                    result++;
                var insertingRow = RowManager.InsertingRow;
                if (insertingRow != null && result >= insertingRow.Index)
                    result++;
                return result;
            }
        }

        /// <summary>
        /// Gets the depth of recursive row.
        /// </summary>
        public int Depth
        {
            get { return Parent == null ? 0 : RowMapper.GetDepth(Parent.DataRow) + 1; }
        }

        /// <summary>
        /// Gets a value indicates whether child row exists.
        /// </summary>
        public bool HasChildren
        {
            get
            {
                var dataPresenter = DataPresenter;
                return dataPresenter != null ? dataPresenter.HasChildren(this) : InternalHasChildren;
            }
        }

        internal bool InternalHasChildren
        {
            get { return Children.Count > 0; }
        }

        private bool _isExpanded = false;
        /// <summary>
        /// Gets a value indicates whether this row is expanded to display child rows.
        /// </summary>
        public bool IsExpanded
        {
            get { return _isExpanded; }
            private set
            {
                _isExpanded = value;
                ElementManager?.InvalidateView();
            }
        }

        /// <summary>
        /// Toggles the expand state to display/hide child rows.
        /// </summary>
        public void ToggleExpandState()
        {
            var dataPresenter = DataPresenter;
            if (dataPresenter != null)
                dataPresenter.ToggleExpandState(this);
            else
                InternalToggleExpandState();
        }

        internal void InternalToggleExpandState()
        {
            if (IsExpanded)
                Collapse();
            else
                Expand();
        }

        internal void Expand()
        {
            if (IsExpanded)
                return;

            if (IsRecursive)
                RowNormalizer.Expand(this);
            IsExpanded = true;
        }

        internal void Collapse()
        {
            if (!IsExpanded)
                return;

            if (IsRecursive)
                RowNormalizer.Collapse(this);
            IsExpanded = false;
        }

        /// <summary>
        /// Gets a value indicates whether this row is current row.
        /// </summary>
        public bool IsCurrent
        {
            get { return RowManager.CurrentRow == this; }
        }

        /// <summary>
        /// Gets or sets the value indicates whether this row is selected.
        /// </summary>
        public bool IsSelected
        {
            get { return RowManager.IsSelected(this); }
            set
            {
                var oldValue = IsSelected;
                if (oldValue == value)
                    return;

                if (oldValue)
                    RowManager.RemoveSelectedRow(this);
                if (value)
                    RowManager.AddSelectedRow(this);
            }
        }

        /// <summary>
        /// Gets the data value for specified column.
        /// </summary>
        /// <typeparam name="T">Data type of the column.</typeparam>
        /// <param name="column">The column.</param>
        /// <param name="beforeEdit">Indicates whether should get value before editing.</param>
        /// <returns>The data value.</returns>
        public T GetValue<T>(Column<T> column, bool beforeEdit = false)
        {
            if (Depth > 0)
            {
                var model = DataRow.Model;
                IReadOnlyList<Column> columns = (column.Kind & ColumnKind.Local) == ColumnKind.Local ? model.GetLocalColumns() : model.GetColumns();
                column = (Column<T>)columns[column.Ordinal];
            }
            return DataRow == null ? default(T) : column[DataRow, beforeEdit];
        }

        private Column VerifyColumn(Column column, string paramName)
        {
            if (column == null)
                throw new ArgumentNullException(paramName);

            if (column.OwnerModel != RowMapper.DataSet.Model)
                throw new ArgumentException(DiagnosticMessages.RowPresenter_VerifyColumn, paramName);

            if (Depth > 0)
            {
                var model = DataRow.Model;
                IReadOnlyList<Column> columns = (column.Kind & ColumnKind.Local) == ColumnKind.Local ? model.GetLocalColumns() : model.GetColumns();
                column = columns[column.Ordinal];
            }

            return column;
        }

        /// <summary>
        /// Gets or sets the data value for specified column.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <returns>The data value.</returns>
        public object this[Column column]
        {
            get { return this[column, false]; }
            set
            {
                column = VerifyColumn(column, nameof(column));

                var elementManager = ElementManager;
                elementManager?.SuspendInvalidateView();
                BeginEdit();
                column.SetValue(DataRow, value);
                DataPresenter?.OnEdit(column);
                elementManager?.InvalidateView();
                elementManager?.ResumeInvalidateView();
            }
        }

        /// <summary>
        /// Gets the data value for specified column.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <param name="beforeEdit">Indicates whether should get value before editing.</param>
        /// <returns>The data value.</returns>
        public object this[Column column, bool beforeEdit]
        {
            get
            {
                column = VerifyColumn(column, nameof(column));
                return DataRow == null ? null : column.GetValue(DataRow, beforeEdit);
            }
        }

        /// <summary>
        /// Determines whether data value of specified column is null.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <returns><see langword="true"/> if data value of specified column is null, otherwise <see langword="false"/>.</returns>
        public bool IsNull(Column column)
        {
            column = VerifyColumn(column, nameof(column));
            var dataRow = DataRow;
            return dataRow == null ? false : column.IsNull(dataRow);
        }

        private bool HasPendingEdit
        {
            get { return RowManager.IsEditing || DataSet.EditingRow != null; }
        }

        private void VerifyNoPendingEdit()
        {
            if (HasPendingEdit)
                throw new InvalidOperationException(DiagnosticMessages.RowPresenter_VerifyNoPendingEdit);
        }

        internal void VerifyIsCurrent()
        {
            if (!IsCurrent)
                throw new InvalidOperationException(DiagnosticMessages.RowPresenter_VerifyIsCurrent);
        }

        /// <summary>
        /// Begins the editing mode.
        /// </summary>
        public void BeginEdit()
        {
            if (IsEditing)
                return;

            VerifyIsCurrent();
            VerifyNoPendingEdit();
            if (IsVirtual && RowManager.Template.VirtualRowPlacement == VirtualRowPlacement.Exclusive)
                throw new InvalidOperationException(DiagnosticMessages.RowPresenter_BeginEditExclusiveVirtual);
            var elementManager = ElementManager;
            elementManager?.SuspendInvalidateView();
            RowManager.BeginEdit();
            elementManager?.ResumeInvalidateView();
        }

        /// <summary>
        /// Begins the editing mode and change the data value for specified column.
        /// </summary>
        /// <typeparam name="T">The data type of column.</typeparam>
        /// <param name="column">The column.</param>
        /// <param name="value">The data value.</param>
        public void EditValue<T>(Column<T> column, T value)
        {
            column = (Column<T>)VerifyColumn(column, nameof(column));

            var elementManager = ElementManager;
            elementManager?.SuspendInvalidateView();
            BeginEdit();
            column[DataRow] = value;
            DataPresenter?.OnEdit(column);
            elementManager?.InvalidateView();
            elementManager?.ResumeInvalidateView();
        }

        /// <summary>
        /// Cancels the editing mode.
        /// </summary>
        public void CancelEdit()
        {
            if (!IsEditing)
                throw new InvalidOperationException(DiagnosticMessages._VerifyIsEditing);

            var elementManager = ElementManager;
            elementManager?.SuspendInvalidateView();
            RowManager.CancelEdit();
            elementManager?.ResumeInvalidateView();
        }

        /// <summary>
        /// Ends the editing mode.
        /// </summary>
        /// <param name="staysOnInserting">Indicates whether should stay on inserting.</param>
        /// <returns>The row presenter.</returns>
        public RowPresenter EndEdit(bool staysOnInserting = true)
        {
            if (!IsEditing)
                throw new InvalidOperationException(DiagnosticMessages._VerifyIsEditing);

            var elementManager = ElementManager;
            elementManager?.SuspendInvalidateView();
            var result = RowManager.EndEdit(staysOnInserting);
            elementManager?.ResumeInvalidateView();
            return result;
        }

        private bool ConfirmEndEdit()
        {
            return DataPresenter == null ? true : DataPresenter.ConfirmEndEdit();
        }

        /// <summary>
        /// Begins inserting child row before specified child row.
        /// </summary>
        /// <param name="child">The specified child row, <see langword="null"/> if insert as first child row.</param>
        public void BeginInsertBefore(RowPresenter child = null)
        {
            VerifyInsert(child);
            var elementManager = ElementManager;
            elementManager?.SuspendInvalidateView();
            RowManager.BeginInsertBefore(this, child);
            elementManager?.ResumeInvalidateView();
        }

        /// <summary>
        /// Begins inserting child row after specified child row.
        /// </summary>
        /// <param name="child">The specified child row, <see langword="null"/> if insert as last child row.</param>
        public void BeginInsertAfter(RowPresenter child = null)
        {
            VerifyInsert(child);
            var elementManager = ElementManager;
            elementManager?.SuspendInvalidateView();
            RowManager.BeginInsertAfter(this, child);
            elementManager?.ResumeInvalidateView();
        }

        private void VerifyInsert(RowPresenter child)
        {
            if (child == null)
                VerifyNoPendingEdit();
            else
            {
                if (child.Parent != this)
                    throw new ArgumentException(DiagnosticMessages.RowPresenter_InvalidChildRow, nameof(child));
                child.VerifyNoPendingEdit();
            }
        }

        /// <summary>
        /// Gets the DataSet.
        /// </summary>
        public DataSet DataSet
        {
            get { return Parent == null ? RowManager.DataSet : Parent.DataRow[Template.RecursiveModelOrdinal]; }
        }

        /// <summary>
        /// Deletes this row.
        /// </summary>
        public void Delete()
        {
            VerifyNotDisposed();
            if (IsVirtual)
                return;

            DataRow.DataSet.Remove(DataRow);
        }

        /// <summary>
        /// Gets the row view.
        /// </summary>
        public RowView View { get; internal set; }

        internal RowBindingCollection RowBindings
        {
            get { return Template.InternalRowBindings; }
        }

        /// <summary>
        /// Sets column value in <see cref="ColumnValueBag"/>.
        /// </summary>
        /// <param name="valueBag">The <see cref="ColumnValueBag"/>.</param>
        /// <param name="column">The column.</param>
        public void SetValueBag(ColumnValueBag valueBag, Column column)
        {
            if (valueBag == null)
                throw new ArgumentNullException(nameof(valueBag));
            column = VerifyColumn(column, nameof(column));
            if (DataRow != null)
                valueBag.SetValue(column, DataRow);
            else
                valueBag[column] = column.GetDefaultValue();
        }

        /// <summary>
        /// Sets key and lookup values in <see cref="ColumnValueBag"/>.
        /// </summary>
        /// <param name="valueBag">The <see cref="ColumnValueBag"/>.</param>
        /// <param name="key">The key.</param>
        /// <param name="lookup">The lookup projection.</param>
        public void SetValueBag(ColumnValueBag valueBag, CandidateKey key, Projection lookup)
        {
            if (valueBag == null)
                throw new ArgumentNullException(nameof(valueBag));
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            for (int i = 0; i < key.Count; i++)
            {
                var column = key[i].Column;
                SetValueBag(valueBag, column);
            }

            if (lookup != null)
            {
                var columns = lookup.GetColumns();
                for (int i = 0; i < columns.Count; i++)
                    SetValueBag(valueBag, columns[i]);
            }
        }

        /// <summary>
        /// Creates column value bag with specified key and lookup.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="lookup">The lookup projection.</param>
        /// <returns>The created <see cref="ColumnValueBag"/>.</returns>
        public ColumnValueBag MakeValueBag(CandidateKey key, Projection lookup)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            var result = new ColumnValueBag();
            result.AutoSelect(key, DataRow);
            if (lookup != null)
                result.AutoSelect(lookup, DataRow);
            return result;
        }

        /// <summary>
        /// Performs validation operation.
        /// </summary>
        /// <param name="invalidateView">Indicates whether view should be invalidated.</param>
        public void Validate(bool invalidateView = true)
        {
            var rowValidation = InputManager.RowValidation;
            rowValidation.Validate(this, true);
            if (invalidateView)
                InputManager.InvalidateView();
        }

        /// <summary>
        /// Gets the validation info for specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>The result validation info.</returns>
        public ValidationInfo GetValidationInfo(Input<RowBinding, IColumns> input)
        {
            input.VerifyNotNull(nameof(input));
            return InputManager.GetValidationInfo(this, input);
        }

        /// <summary>
        /// Determines whether validation error exists for specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns><see langword="true"/> if validation error exists, otherwise <see langword="false"/>.</returns>
        public bool HasValidationError(Input<RowBinding, IColumns> input)
        {
            input.VerifyNotNull(nameof(input));
            return InputManager.HasValidationError(this, input);
        }

        /// <summary>
        /// Determines whether specified input is validating.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns><see langword="true"/> if input is validating, otherwise <see langword="false"/>.</returns>
        public bool IsValidating(Input<RowBinding, IColumns> input)
        {
            input.VerifyNotNull(nameof(input));
            return InputManager.IsValidating(this, input);
        }

        /// <summary>
        /// Gets the visible validation errors.
        /// </summary>
        public IValidationErrors VisibleValidationErrors
        {
            get { return InputManager.GetVisibleValidationErrors(this); }
        }

        /// <summary>
        /// Gets the value indicates whether visible validation error exists.
        /// </summary>
        public bool HasVisibleValidationError
        {
            get { return InputManager.HasVisibleValidationError(this); }
        }

        internal void OnValueChanged(ValueChangedEventArgs e)
        {
            if (AffectsRowMatch(e.Columns))
                RefreshMatchValueHashCode();
            ValueChanged(this, e);
        }

        private bool AffectsRowMatch(IColumns columns)
        {
            if (columns == null || columns.Count == 0 || _rowMapper == null || !_rowMapper.CanMatchRow)
                return false;

            var rowMatchColumns = _rowMapper.RowMatchColumns;
            for (int i = 0; i < rowMatchColumns.Count; i++)
            {
                if (columns.Contains(rowMatchColumns[i]))
                    return true;
            }
            return false;
        }

        private int? _matchValueHashCode;
        internal int? MatchValueHashCode
        {
            get { return _matchValueHashCode; }
            set
            {
                if (_matchValueHashCode == value)
                    return;

                var oldValue = _matchValueHashCode;
                _matchValueHashCode = value;
                Debug.Assert(oldValue.HasValue || value.HasValue);
                Debug.Assert(_rowMapper.CanMatchRow);
                _rowMapper.UpdateRowMatch(this, oldValue, value);
            }
        }

        internal IReadOnlyList<Column> MatchColumns
        {
            get { return _rowMapper?.RowMatchColumns; }
        }

        private void RefreshMatchValueHashCode()
        {
            MatchValueHashCode = GetMatchValueHashCode();
        }

        private int? GetMatchValueHashCode()
        {
            return _rowMapper == null || !_rowMapper.CanMatchRow || DataRow == null || DataRow.IsAdding ? null : RowMatch.GetHashCode(MatchColumns, DataRow);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Index.ToString();
        }

        private ScrollableManager ScrollableManager
        {
            get { return RowMapper as ScrollableManager; }
        }

        /// <summary>
        /// Resizes the layout grid track.
        /// </summary>
        /// <param name="gridTrack">The layout grid track.</param>
        /// <param name="length">The resized length.</param>
        public void Resize(GridTrack gridTrack, GridLength length)
        {
            gridTrack.VerifyNotNull(nameof(gridTrack));
            if (!gridTrack.IsContainer)
                throw new ArgumentException(DiagnosticMessages.RowPresenter_Resize_InvalidGridTrack, nameof(gridTrack));
            if (length.IsStar)
                throw new ArgumentException(DiagnosticMessages.RowPresenter_Resize_InvalidStarLength, nameof(length));
            if (IsDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            var scrollableManager = ScrollableManager;
            if (scrollableManager == null || Template.FlowRepeatCount != 1)
                throw new InvalidOperationException(DiagnosticMessages.RowPresenter_Resize_NotAllowed);

            scrollableManager.Resize(this, gridTrack, length);
        }

        /// <summary>
        /// Gets the length for layout grid track.
        /// </summary>
        /// <param name="gridTrack"></param>
        /// <returns></returns>
        public GridLength GetLength(GridTrack gridTrack)
        {
            gridTrack.VerifyNotNull(nameof(gridTrack));
            if (!gridTrack.IsContainer)
                throw new ArgumentException(DiagnosticMessages.RowPresenter_Resize_InvalidGridTrack, nameof(gridTrack));
            if (IsDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            var scrollableManager = ScrollableManager;
            if (scrollableManager == null || Template.FlowRepeatCount != 1)
                throw new InvalidOperationException(DiagnosticMessages.RowPresenter_Resize_NotAllowed);

            return scrollableManager.GetLength(gridTrack, this);
        }

        /// <summary>
        /// Gets the measured length of layout grid track.
        /// </summary>
        /// <param name="gridTrack">The layout grid track.</param>
        /// <returns>the measured length.</returns>
        public double? GetMeasuredLength(GridTrack gridTrack)
        {
            gridTrack.VerifyNotNull(nameof(gridTrack));
            if (!gridTrack.IsContainer)
                throw new ArgumentException(DiagnosticMessages.RowPresenter_Resize_InvalidGridTrack, nameof(gridTrack));
            if (IsDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            var scrollableManager = ScrollableManager;
            if (scrollableManager == null || Template.FlowRepeatCount != 1)
                throw new InvalidOperationException(DiagnosticMessages.RowPresenter_Resize_NotAllowed);

            var rowView = View;
            if (rowView == null)
                return null;
            var containerView = scrollableManager[rowView.ContainerOrdinal];
            return scrollableManager.GetMeasuredLength(containerView, gridTrack);
        }

        /// <summary>Update sort order of this <see cref="RowPresenter"/>.</summary>
        /// <remarks>Sort order of <see cref="RowPresenter"/> will be maintained automatically when underlying <see cref="DataRow"/> value changed, there is
        /// no need to call this method to reflect sort order change when underlying value changed; This method is provided for scenario of custom sort
        /// when order is decided other than value change, for example, sort by <see cref="RowPresenter.IsSelected"/>.</remarks>
        public void UpdateSortOrder()
        {
            RowManager.Update(this, null);
        }
    }
}
