using DevZest.Data.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Collections;

namespace DevZest.Data.Windows.Primitives
{
    internal abstract class RowManager : RowListMananger, IReadOnlyList<RowPresenter>
    {
        protected RowManager(Template template, DataSet dataSet, _Boolean where, ColumnSort[] orderBy)
            : base(template, dataSet, where, orderBy)
        {
            Template.RowManager = this;
        }

        protected override void Initialize()
        {
            base.Initialize();
            var rowPlaceholderStrategy = Template.RowPlaceholderStrategy;
            if (rowPlaceholderStrategy == RowPlaceholderStrategy.Top)
                Placeholder = new RowPresenter(this, 0);
            else if (rowPlaceholderStrategy == RowPlaceholderStrategy.Bottom)
                Placeholder = new RowPresenter(this, base.Rows.Count);
            else
                Rows_CoerceNoDataPlaceholder();
        }

        //private void Initialize()
        //{
        //    Rows_Initialize();
        //    SetCurrentRow(CoercedCurrentRow);
        //}

        protected virtual void OnCurrentRowChanged()
        {
            Invalidate(null);
        }

        private void OnEditingRowChanged()
        {
            Invalidate(null);
        }

        private void OnSelectedRowsChanged()
        {
            Invalidate(null);
        }

        protected virtual void DisposeRow(RowPresenter row)
        {
            row.Dispose();
        }

        private int _savedCurrentRowIndex = -1;
        private void SaveCurrentRowIndex(int startRemovalIndex, int count)
        {
            if (_currentRow != null)
            {
                var currentRowIndex = _currentRow.Index;
                if (currentRowIndex >= startRemovalIndex && currentRowIndex < startRemovalIndex + count)
                    _savedCurrentRowIndex = currentRowIndex;
            }
        }

        public RowPresenter Placeholder { get; private set; }

        private int PlaceholderIndex
        {
            get { return Placeholder == null ? -1 : Placeholder.Index; }
        }

        public sealed override IReadOnlyList<RowPresenter> Rows
        {
            get { return this; }
        }

        #region IReadOnlyList<RowPresenter>

        private int RowCount
        {
            get
            {
                var result = base.Rows.Count;
                if (Placeholder != null)
                    result++;
                return result;
            }
        }

        private RowPresenter GetRow(int index)
        {
            Debug.Assert(index >= 0 && index < RowCount);

            if (index == PlaceholderIndex)
                return Placeholder;

            if (PlaceholderIndex >= 0 && index > PlaceholderIndex)
                index--;
            return base.Rows[index];
        }

        int IReadOnlyCollection<RowPresenter>.Count
        {
            get { return RowCount; }
        }

        RowPresenter IReadOnlyList<RowPresenter>.this[int index]
        {
            get
            {
                if (index < 0 || index >= RowCount)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return GetRow(index);
            }
        }

        IEnumerator<RowPresenter> IEnumerable<RowPresenter>.GetEnumerator()
        {
            for (int i = 0; i < RowCount; i++)
                yield return GetRow(i);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            for (int i = 0; i < RowCount; i++)
                yield return GetRow(i);
        }

        #endregion

        private void Rows_CoerceNoDataPlaceholder()
        {
            if (Template.RowPlaceholderStrategy != RowPlaceholderStrategy.NoData)
                return;

            var index = base.Rows.Count == 0 ? 0 : -1;
            if (index == 0)
            {
                if (Placeholder == null)
                    Placeholder = new RowPresenter(this, 0);
            }
            else
            {
                if (Placeholder != null)
                {
                    var placeholder = Placeholder;
                    Placeholder = null;
                    DisposeRow(placeholder);
                }
            }
        }

        //private void RowMappings_Remove(int depth, int ordinal)
        //{
        //    DisposeRow(_rowMappings[depth][ordinal]);
        //    _rowMappings[depth].RemoveAt(ordinal);
        //    if (!IsQuery)
        //        OnRowsChanged();
        //}

        //private int GetIndex(RowPresenter row)
        //{
        //    Debug.Assert(!row.IsPlaceholder);

        //    if (!IsRecursive)
        //        return -1;

        //    var parentRow = row.Parent;
        //    var prevSiblingRow = PrevSiblingOf(row);
        //    if (parentRow == null)
        //        return prevSiblingRow == null ? 0 : FlatRows_NextIndexOf(prevSiblingRow);
        //    else if (parentRow.Index >= 0 && parentRow.IsExpanded)
        //        return prevSiblingRow == null ? parentRow.Index + 1 : FlatRows_NextIndexOf(prevSiblingRow);
        //    else
        //        return -1;
        //}

        //private RowPresenter PrevSiblingOf(RowPresenter row)
        //{
        //    return row.DataRow.Index == 0 ? null : RowMappings_GetRow(row.Depth, row.DataRow.Ordinal - 1);
        //}

        private RowPresenter CoercedCurrentRow
        {
            get
            {
                if (_currentRow == null)
                {
                    if (Rows.Count > 0)
                        return Rows[0];
                }
                else
                {
                    if (_savedCurrentRowIndex != -1)
                    {
                        var currentRowIndex = Math.Min(Rows.Count - 1, _savedCurrentRowIndex);
                        _savedCurrentRowIndex = -1;
                        return currentRowIndex < 0 ? null : Rows[currentRowIndex];
                    }
                    else if (Rows.Count == 0)
                        return null;
                }
                return _currentRow;
            }
        }

        private DataRow _viewUpdateSuppressed;

        internal void SuppressViewUpdate(DataRow dataRow)
        {
            Debug.Assert(_viewUpdateSuppressed == null);
            _viewUpdateSuppressed = dataRow;
        }

        internal void ResumeViewUpdate()
        {
            Debug.Assert(_viewUpdateSuppressed != null);
            _viewUpdateSuppressed = null;
        }

        private RowPresenter _currentRow;
        public virtual RowPresenter CurrentRow
        {
            get { return _currentRow; }
            set
            {
                SetCurrentRow(value);
                OnCurrentRowChanged();
            }
        }

        private void SetCurrentRow(RowPresenter value)
        {
            if (_currentRow == value)
                return;

            if (value != null)
            {
                if (value.RowManager != this || value.Index < 0)
                    throw new ArgumentException(Strings.RowManager_InvalidCurrentRow, nameof(value));
            }

            var oldValue = _currentRow;
            if (_currentRow != null)
                _currentRow.IsCurrent = false;

            _currentRow = value;

            if (_currentRow != null)
                _currentRow.IsCurrent = true;
        }

        private HashSet<RowPresenter> _selectedRows = new HashSet<RowPresenter>();
        public IReadOnlyCollection<RowPresenter> SelectedRows
        {
            get { return _selectedRows; }
        }

        internal void AddSelectedRow(RowPresenter row)
        {
            _selectedRows.Add(row);
            OnSelectedRowsChanged();
        }

        internal void RemoveSelectedRow(RowPresenter row)
        {
            _selectedRows.Remove(row);
            OnSelectedRowsChanged();
        }

        private RowPresenter _editingRow;
        public virtual RowPresenter EditingRow
        {
            get { return _editingRow; }
            internal set
            {
                _editingRow = value;
                OnEditingRowChanged();
            }
        }
    }
}
