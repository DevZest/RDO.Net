using DevZest.Data.Primitives;
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections;
using System;
using System.Linq;
using DevZest.Data.Windows.Factories;
using DevZest.Data.Windows.Primitives;

namespace DevZest.Data.Windows
{
    public sealed partial class DataPresenter : RowManager, IReadOnlyList<RowPresenter>
    {
        internal static DataPresenter Create<T>(RowPresenter owner, T childModel, Action<TemplateBuilder, T> buildTemplateAction)
            where T : Model, new()
        {
            Debug.Assert(owner != null);
            Debug.Assert(childModel != null);
            Debug.Assert(buildTemplateAction != null);

            var result = new DataPresenter(owner, owner.DataRow[childModel]);
            using (var templateBuilder = new TemplateBuilder(result.Template))
            {
                buildTemplateAction(templateBuilder, childModel);
            }

            result.Initialize();
            return result;
        }

        public static DataPresenter Create<T>(DataSet<T> dataSet, Action<TemplateBuilder, T> buildTemplateAction = null)
            where T : Model, new()
        {
            if (dataSet == null)
                throw new ArgumentNullException(nameof(dataSet));

            return Create<T>(null, dataSet, buildTemplateAction);
        }

        private static DataPresenter Create<T>(RowPresenter owner, DataSet<T> dataSet, Action<TemplateBuilder, T> buildTemplateAction)
            where T : Model, new()
        {
            var model = dataSet._;
            var result = new DataPresenter(owner, dataSet);
            using (var templateBuilder = new TemplateBuilder(result.Template))
            {
                if (buildTemplateAction != null)
                    buildTemplateAction(templateBuilder, model);
                else
                    BuildDefaultTemplate(templateBuilder, model);
            }

            result.Initialize();
            return result;
        }

        private static void BuildDefaultTemplate(TemplateBuilder templateBuilder, Model model)
        {
            var columns = model.GetColumns();
            if (columns.Count == 0)
                return;

            templateBuilder.AddGridColumns(columns.Select(x => "Auto").ToArray())
                .AddGridRows("Auto", "Auto")
                .Range(0, 1, columns.Count - 1, 1).Repeat();

            for (int i = 0; i < columns.Count; i++)
            {
                var column = columns[i];
                templateBuilder.Range(i, 0).ColumnHeader(column).Range(i, 1).TextBlock(column);
            }
        }

        private DataPresenter(RowPresenter parent, DataSet dataSet)
            : base(dataSet)
        {
            Debug.Assert(dataSet != null);
            Debug.Assert(dataSet.ParentRow == null || dataSet.ParentRow == parent.DataRow);

            _parent = parent;
            FlowCount = 1;
        }

        private new void Initialize()
        {
            _rows = new RowPresenterCollection(this);
            if (Count > 0)
                CurrentRow = this[0];
            LayoutManager = LayoutManager.Create(this);

            DataSet.RowUpdated += (sender, e) => OnRowUpdated(e.DataRow.Index);
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

        int _rowPresenterStateFlags;

        private static int GetMask(RowPresenterState rowPresenterState)
        {
            return 1 << (int)rowPresenterState;
        }

        internal bool IsConsumed(RowPresenterState rowPresenterState)
        {
            int mask = GetMask(rowPresenterState);
            return (_rowPresenterStateFlags & mask) != 0;
        }

        internal void OnGetValue(RowPresenterState rowPresenterState)
        {
            if (_isUpdatingTarget)
            {
                int mask = GetMask(rowPresenterState);
                _rowPresenterStateFlags |= mask;
            }
        }

        int _dataPresenterStateFlags;

        private static int GetMask(DataPresenterState dataPresenterState)
        {
            return 1 << (int)dataPresenterState;
        }

        internal bool IsConsumed(DataPresenterState dataPresenterState)
        {
            int mask = GetMask(dataPresenterState);
            return (_dataPresenterStateFlags & mask) != 0;
        }

        private void OnGetValue(DataPresenterState dataPresenterState)
        {
            if (_isUpdatingTarget)
            {
                int mask = GetMask(dataPresenterState);
                _dataPresenterStateFlags |= mask;
            }
        }

        public event EventHandler BindingsReset;

        private void OnUpdated(DataPresenterState dataPresenterState)
        {
            if (IsConsumed(dataPresenterState))
                OnBindingsReset();
        }

        private void OnBindingsReset()
        {
            var bindingsReset = BindingsReset;
            if (bindingsReset != null)
                bindingsReset(this, EventArgs.Empty);
        }

        internal void OnRowAdded(int index)
        {
            if (CurrentRow == null)
                CurrentRow = this[0];

            if (IsConsumed(RowPresenterState.Index))
            {
                for (int i = index + 1; i < Count; i++)
                    this[i].OnBindingsReset();
            }

            LayoutManager.OnRowAdded(index);
            OnUpdated(DataPresenterState.Rows);
        }

        internal void OnRowRemoved(int index, RowPresenter row)
        {
            if (CurrentRow == row)
                CurrentRow = Count == 0 ? null : this[Math.Min(Count - 1, index)];

            if (IsConsumed(RowPresenterState.Index))
            {
                for (int i = index; i < Count; i++)
                    this[i].OnBindingsReset();
            }

            LayoutManager.OnRowRemoved(index, row);
            OnUpdated(DataPresenterState.Rows);
        }

        private void OnRowUpdated(int index)
        {
            this[index].OnBindingsReset();
        }

        private readonly RowPresenter _parent;
        public RowPresenter Parent
        {
            get { return _parent; }
        }

        #region IReadOnlyList<RowPresenter>

        RowPresenterCollection _rows;
        private RowPresenterCollection Rows
        {
            get
            {
                OnGetValue(DataPresenterState.Rows);
                return _rows;
            }
        }

        public IEnumerator<RowPresenter> GetEnumerator()
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

        public RowPresenter this[int index]
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

        private RowPresenter _currentRow;
        public RowPresenter CurrentRow
        {
            get
            {
                OnGetValue(DataPresenterState.CurrentRow);
                return _currentRow;
            }
            set
            {
                if (_currentRow == value)
                    return;

                if (value != null && value.DataPresenter != this)
                    throw new ArgumentException(Strings.DataPresenter_InvalidCurrentRow, nameof(value));

                var oldValue = _currentRow;
                if (_currentRow != null)
                    _currentRow.IsCurrent = false;

                _currentRow = value;

                if (_currentRow != null)
                    _currentRow.IsCurrent = true;

                if (LayoutManager != null)
                    LayoutManager.OnCurrentRowChanged();
                OnUpdated(DataPresenterState.CurrentRow);
            }
        }

        private HashSet<RowPresenter> _selectedRows = new HashSet<RowPresenter>();
        public IReadOnlyCollection<RowPresenter> SelectedRows
        {
            get
            {
                OnGetValue(DataPresenterState.SelectedRows);
                return _selectedRows;
            }
        }

        internal void AddSelectedRow(RowPresenter row)
        {
            _selectedRows.Add(row);
            OnUpdated(DataPresenterState.SelectedRows);
        }

        internal void RemoveSelectedRow(RowPresenter row)
        {
            _selectedRows.Remove(row);
            OnUpdated(DataPresenterState.SelectedRows);
        }

        internal LayoutManager LayoutManager { get; private set; }

        public int FlowCount { get; internal set; }
    }
}
