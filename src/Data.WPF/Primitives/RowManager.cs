using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections;

namespace DevZest.Data.Windows.Primitives
{
    internal abstract class RowManager : RowListMananger, IReadOnlyList<RowPresenter>
    {
        private abstract class _PlaceholderManager
        {
            public static readonly _PlaceholderManager None = new NonePlaceholderManager();
            public static readonly _PlaceholderManager Top = new TopPlaceholderManager();
            public static readonly _PlaceholderManager Bottom = new BottomPlaceholderManager();
            public static readonly _PlaceholderManager EmptyDataSet = new EmptyDataSetPlaceholderManager();

            public abstract void Initialize(RowManager rowManager);

            public abstract void Coerce(RowManager rowManager);

            private sealed class NonePlaceholderManager : _PlaceholderManager
            {
                public override void Initialize(RowManager rowManager)
                {
                }

                public override void Coerce(RowManager rowManager)
                {
                    rowManager.CoerceNoPlaceholder();
                }
            }

            private sealed class TopPlaceholderManager : _PlaceholderManager
            {
                public override void Initialize(RowManager rowManager)
                {
                    rowManager.InitializeTopPlaceholder();
                }

                public override void Coerce(RowManager rowManager)
                {
                }
            }

            private sealed class BottomPlaceholderManager : _PlaceholderManager
            {
                public override void Initialize(RowManager rowManager)
                {
                    rowManager.InitializeBottomPlaceholder();
                }

                public override void Coerce(RowManager rowManager)
                {
                    rowManager.CoerceBottomPlaceholder();
                }
            }

            private sealed class EmptyDataSetPlaceholderManager : _PlaceholderManager
            {
                public override void Initialize(RowManager rowManager)
                {
                    rowManager.CoerceEmptyDataSetPlaceholder();
                }

                public override void Coerce(RowManager rowManager)
                {
                    rowManager.CoerceEmptyDataSetPlaceholder();
                }
            }
        }

        private abstract class _InsertCommand
        {
            public static readonly _InsertCommand Top = new InsertBeforeCommand(null, null);
            public static readonly _InsertCommand Bottom = new InsertAfterCommand(null, null);
            public static _InsertCommand Before(RowPresenter parent, RowPresenter reference)
            {
                return new InsertBeforeCommand(parent, reference);
            }

            public static _InsertCommand After(RowPresenter parent, RowPresenter reference)
            {
                return new InsertAfterCommand(parent, reference);
            }

            protected _InsertCommand(RowPresenter parent, RowPresenter reference)
            {
                _parent = parent;
                Reference = reference;
            }

            private readonly RowPresenter _parent;
            protected RowPresenter Parent { get; }

            protected RowPresenter Reference { get; private set; }

            public void CoercePlaceholderIndex(RowManager rowManager)
            {
                rowManager.Placeholder.Index = GetPlaceholderIndex(rowManager);
            }

            protected abstract int GetPlaceholderIndex(RowManager rowManager);

            private sealed class InsertBeforeCommand : _InsertCommand
            {
                public InsertBeforeCommand(RowPresenter parent, RowPresenter reference)
                    : base(parent, reference)
                {
                }

                protected override int GetPlaceholderIndex(RowManager rowManager)
                {
                    if (Reference != null)
                        return Reference.Index;
                    else
                        return Parent != null ? Parent.Index + 1 : 0;
                }
            }

            private sealed class InsertAfterCommand : _InsertCommand
            {
                public InsertAfterCommand(RowPresenter parent, RowPresenter reference)
                    : base(parent, reference)
                {
                }

                protected override int GetPlaceholderIndex(RowManager rowManager)
                {
                    if (Reference != null)
                        return Reference.Index;
                    else
                        return Parent != null ? Parent.Index + Parent.Children.Count + 1 : rowManager.BaseRows.Count;
                }
            }
        }

        protected RowManager(Template template, DataSet dataSet, _Boolean where, ColumnSort[] orderBy)
            : base(template, dataSet, where, orderBy)
        {
            Template.RowManager = this;
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

        private IReadOnlyList<RowPresenter> BaseRows
        {
            get { return base.Rows; }
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

        private RowPlaceholderPosition PlaceholderPosition
        {
            get { return Template.RowPlaceholderPosition; }
        }

        private _PlaceholderManager PlaceholderManager
        {
            get
            {
                switch (PlaceholderPosition)
                {
                    case RowPlaceholderPosition.Bottom:
                        return _PlaceholderManager.Bottom;
                    case RowPlaceholderPosition.Top:
                        return _PlaceholderManager.Top;
                    case RowPlaceholderPosition.EmptyDataSet:
                        return _PlaceholderManager.EmptyDataSet;
                    case RowPlaceholderPosition.None:
                        return _PlaceholderManager.None;
                }
                return null;
            }
        }

        protected override void Initialize()
        {
            base.Initialize();
            PlaceholderManager.Initialize(this);
            SetCurrentRow(CoercedCurrentRow);
        }

        private void InitializeTopPlaceholder()
        {
            Placeholder = new RowPresenter(this, 0);
        }

        private void InitializeBottomPlaceholder()
        {
            Placeholder = new RowPresenter(this, base.Rows.Count);
        }

        private void CoerceNoPlaceholder()
        {
            Debug.Assert(PlaceholderPosition == RowPlaceholderPosition.None);

            ClearPlaceholder();
        }

        private void ClearPlaceholder()
        {
            if (Placeholder != null)
            {
                var placeholder = Placeholder;
                Placeholder = null;
                DisposeRow(placeholder);
            }
        }

        private void CoerceEmptyDataSetPlaceholder()
        {
            Debug.Assert(PlaceholderPosition == RowPlaceholderPosition.EmptyDataSet);

            var index = base.Rows.Count == 0 ? 0 : -1;
            if (index == 0)
            {
                if (Placeholder == null)
                    Placeholder = new RowPresenter(this, 0);
            }
            else
                ClearPlaceholder();
        }

        private void CoerceBottomPlaceholder()
        {
            Debug.Assert(PlaceholderPosition == RowPlaceholderPosition.Bottom && Placeholder != null);

            Placeholder.Index = base.Rows.Count;
        }

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
