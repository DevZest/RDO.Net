using DevZest.Data.Primitives;
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections;
using System;
using System.Linq;
using DevZest.Data.Windows.Factories;

namespace DevZest.Data.Windows
{
    public sealed partial class DataView : IReadOnlyList<RowView>
    {
        internal static DataView Create<T>(RowView owner, T childModel, Action<DataViewBuilder, T> action)
            where T : Model, new()
        {
            Debug.Assert(owner != null);
            Debug.Assert(childModel != null);
            Debug.Assert(action != null);

            var result = new DataView(owner, owner.DataRow[childModel]);
            using (var viewBuilder = new DataViewBuilder(result))
            {
                action(viewBuilder, childModel);
            }

            result.Initialize();
            return result;
        }

        public static DataView Create<T>(DataSet<T> dataSet, Action<DataViewBuilder, T> builderAction = null)
            where T : Model, new()
        {
            if (dataSet == null)
                throw new ArgumentNullException(nameof(dataSet));

            return Create<T>(null, dataSet, builderAction);
        }

        private static DataView Create<T>(RowView owner, DataSet<T> dataSet, Action<DataViewBuilder, T> builderAction)
            where T : Model, new()
        {
            var model = dataSet._;
            var result = new DataView(owner, dataSet);
            using (var builder = new DataViewBuilder(result))
            {
                if (builderAction != null)
                    builderAction(builder, model);
                else
                    DefaultBuilder(builder, model);
            }

            result.Initialize();
            return result;
        }

        private static void DefaultBuilder(DataViewBuilder builder, Model model)
        {
            var columns = model.GetColumns();
            if (columns.Count == 0)
                return;

            builder.AddGridColumns(columns.Select(x => "Auto").ToArray())
                .AddGridRows("Auto", "Auto")
                .Range(0, 1, columns.Count - 1, 1).AsListRange();

            for (int i = 0; i < columns.Count; i++)
            {
                var column = columns[i];
                builder.Range(i, 0).ColumnHeader(column)
                    .Range(i, 1).TextBlock(column);
            }
        }

        private DataView(RowView owner, DataSet dataSet)
        {
            Debug.Assert(dataSet != null);
            Debug.Assert(dataSet.ParentRow == null || dataSet.ParentRow == owner.DataRow);

            _owner = owner;
            _dataSet = dataSet;
            _template = new GridTemplate(this);
            VirtualizingThreshold = 50;
        }

        private void Initialize()
        {
            _rows = new RowCollection(this);
            if (Count > 0)
                CurrentRow = this[0];
            LayoutManager = LayoutManager.Create(this);

            _dataSet.RowUpdated += (sender, e) => OnRowUpdated(e.DataRow.Index);
        }

        public int VirtualizingThreshold { get; private set; }

        internal void InitVirtualizingThreshold(int value)
        {
            VirtualizingThreshold = value;
        }

        public bool IsEofVisible { get; private set; }

        internal void InitIsEofVisible(bool value)
        {
            IsEofVisible = value;
        }

        private bool _isUpdatingTarget;

        internal void EnterUpdatingTarget()
        {
            Debug.Assert(!_isUpdatingTarget);
            _isUpdatingTarget = true;
        }

        internal void ExitUpdatingTarget()
        {
            Debug.Assert(_isUpdatingTarget);
            _isUpdatingTarget = false;
        }

        int _rowViewBindingSourceFlags;

        private static int GetMask(RowViewBindingSource bindingSource)
        {
            return 1 << (int)bindingSource;
        }

        internal bool IsConsumed(RowViewBindingSource bindingSource)
        {
            int mask = GetMask(bindingSource);
            return (_rowViewBindingSourceFlags & mask) != 0;
        }

        internal void OnGetValue(RowViewBindingSource bindingSource)
        {
            if (_isUpdatingTarget)
            {
                int mask = GetMask(bindingSource);
                _rowViewBindingSourceFlags |= mask;
            }
        }

        int _dataViewBindingSourceFlags;

        private static int GetMask(DataViewBindingSource bindingSource)
        {
            return 1 << (int)bindingSource;
        }

        internal bool IsConsumed(DataViewBindingSource bindingSource)
        {
            int mask = GetMask(bindingSource);
            return (_dataViewBindingSourceFlags & mask) != 0;
        }

        private void OnGetValue(DataViewBindingSource bindingSource)
        {
            if (_isUpdatingTarget)
            {
                int mask = GetMask(bindingSource);
                _dataViewBindingSourceFlags |= mask;
            }
        }

        public event EventHandler BindingsReset;

        private void OnUpdated(DataViewBindingSource bindingSource)
        {
            if (IsConsumed(bindingSource))
                OnBindingsReset();
        }

        private void OnBindingsReset()
        {
            var bindingsReset = BindingsReset;
            if (bindingsReset != null)
                bindingsReset(this, EventArgs.Empty);
        }

        public bool IsEmptySetVisible { get; private set; }

        internal void InitIsEmptySetVisible(bool value)
        {
            IsEmptySetVisible = value;
        }

        private void OnRowAdded(int index)
        {
            if (CurrentRow == null)
                CurrentRow = this[0];

            if (IsConsumed(RowViewBindingSource.Index))
            {
                for (int i = index + 1; i < Count; i++)
                    this[i].OnBindingsReset();
            }

            LayoutManager.OnRowAdded(index);
            OnUpdated(DataViewBindingSource.Rows);
        }

        private void OnRowRemoved(int index, RowView row)
        {
            if (CurrentRow == row)
                CurrentRow = Count == 0 ? null : this[Math.Min(Count - 1, index)];

            if (IsConsumed(RowViewBindingSource.Index))
            {
                for (int i = index; i < Count; i++)
                    this[i].OnBindingsReset();
            }

            LayoutManager.OnRowRemoved(index, row);
            OnUpdated(DataViewBindingSource.Rows);
        }

        private void OnRowUpdated(int index)
        {
            this[index].OnBindingsReset();
        }

        private readonly RowView _owner;
        public RowView Owner
        {
            get { return _owner; }
        }

        private readonly DataSet _dataSet;

        private readonly GridTemplate _template;
        public GridTemplate Template
        {
            get { return _template; }
        }

        public Model Model
        {
            get { return _dataSet.Model; }
        }

        #region IReadOnlyList<RowView>

        RowCollection _rows;
        private RowCollection Rows
        {
            get
            {
                OnGetValue(DataViewBindingSource.Rows);
                return _rows;
            }
        }

        public IEnumerator<RowView> GetEnumerator()
        {
            return Rows.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Rows.GetEnumerator();
        }

        public int Count
        {
            get { return Rows.Count; }
        }

        public RowView this[int index]
        {
            get { return Rows[index]; }
        }
        #endregion

        internal void EofToDataRow()
        {
            _rows.EofToDataRow();
        }

        internal void DataRowToEof()
        {
            _rows.DataRowToEof();
        }

        private RowView _currentRow;
        public RowView CurrentRow
        {
            get
            {
                OnGetValue(DataViewBindingSource.CurrentRow);
                return _currentRow;
            }
            set
            {
                if (_currentRow == value)
                    return;

                if (value != null && value.Owner != this)
                    throw new ArgumentException(Strings.DataView_InvalidCurrentRow, nameof(value));

                var oldValue = _currentRow;
                if (_currentRow != null)
                    _currentRow.IsCurrent = false;

                _currentRow = value;

                if (_currentRow != null)
                    _currentRow.IsCurrent = true;

                if (LayoutManager != null)
                    LayoutManager.OnCurrentRowChanged(oldValue);
                OnUpdated(DataViewBindingSource.CurrentRow);
            }
        }

        private HashSet<RowView> _selectedRows = new HashSet<RowView>();
        public IReadOnlyCollection<RowView> SelectedRows
        {
            get
            {
                OnGetValue(DataViewBindingSource.SelectedRows);
                return _selectedRows;
            }
        }

        internal void AddSelectedRow(RowView row)
        {
            _selectedRows.Add(row);
            OnUpdated(DataViewBindingSource.SelectedRows);
        }

        internal void RemoveSelectedRow(RowView row)
        {
            _selectedRows.Remove(row);
            OnUpdated(DataViewBindingSource.SelectedRows);
        }

        internal LayoutManager LayoutManager { get; private set; }
    }
}
