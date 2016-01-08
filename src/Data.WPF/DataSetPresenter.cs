using DevZest.Data.Primitives;
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections;
using System;
using System.Linq;
using DevZest.Data.Windows.Factories;

namespace DevZest.Data.Windows
{
    public sealed partial class DataSetPresenter : IReadOnlyList<DataRowPresenter>
    {
        internal static DataSetPresenter Create<T>(DataRowPresenter owner, T childModel, Action<DataSetPresenterBuilder, T> builder)
            where T : Model, new()
        {
            Debug.Assert(owner != null);
            Debug.Assert(childModel != null);
            Debug.Assert(builder != null);

            var result = new DataSetPresenter(owner, owner.DataRow[childModel]);
            using (var presenterBuilder = new DataSetPresenterBuilder(result))
            {
                builder(presenterBuilder, childModel);
            }

            result.Initialize();
            return result;
        }

        public static DataSetPresenter Create<T>(DataSet<T> dataSet, Action<DataSetPresenterBuilder, T> builderAction = null)
            where T : Model, new()
        {
            if (dataSet == null)
                throw new ArgumentNullException(nameof(dataSet));

            return Create<T>(null, dataSet, builderAction);
        }

        private static DataSetPresenter Create<T>(DataRowPresenter owner, DataSet<T> dataSet, Action<DataSetPresenterBuilder, T> builderAction)
            where T : Model, new()
        {
            var model = dataSet._;
            var result = new DataSetPresenter(owner, dataSet);
            using (var builder = new DataSetPresenterBuilder(result))
            {
                if (builderAction != null)
                    builderAction(builder, model);
                else
                    DefaultBuilder(builder, model);
            }

            result.Initialize();
            return result;
        }

        private static void DefaultBuilder(DataSetPresenterBuilder builder, Model model)
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

        private DataSetPresenter(DataRowPresenter owner, DataSet dataSet)
        {
            Debug.Assert(dataSet != null);
            Debug.Assert(dataSet.ParentRow == null || dataSet.ParentRow == owner.DataRow);

            _owner = owner;
            _dataSet = dataSet;
            _template = new GridTemplate(this);
            IsVirtualizing = true;
        }

        private void Initialize()
        {
            _rows = new RowCollection(this);
            if (Count > 0)
                CurrentRow = this[0];
            LayoutManager = LayoutManager.Create(this);

            _dataSet.RowUpdated += (sender, e) => OnRowUpdated(e.DataRow.Index);
        }

        public bool IsVirtualizing { get; private set; }

        internal void InitIsVirtualizing(bool value)
        {
            IsVirtualizing = value;
        }


        public bool IsEofVisible { get; private set; }

        internal void InitIsEofVisible(bool value)
        {
            IsEofVisible = value;
        }

        internal bool IsUpdatingTarget { get; private set; }

        internal void EnterUpdatingTarget()
        {
            Debug.Assert(!IsUpdatingTarget);
            IsUpdatingTarget = true;
        }

        internal void ExitUpdatingTarget()
        {
            Debug.Assert(IsUpdatingTarget);
            IsUpdatingTarget = false;
        }

        int _shouldFireRowUpdatedEventFlags;

        private static int GetMask(RowProperty rowProperty)
        {
            return 1 << (int)rowProperty;
        }

        internal bool ShouldFireRowUpdatedEvent(RowProperty rowProperty)
        {
            int mask = GetMask(rowProperty);
            return (_shouldFireRowUpdatedEventFlags & mask) != 0;
        }

        internal void OnGetRowProperty(RowProperty rowProperty)
        {
            if (IsUpdatingTarget)
            {
                int mask = GetMask(rowProperty);
                _shouldFireRowUpdatedEventFlags |= mask;
            }
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

            if (ShouldFireRowUpdatedEvent(RowProperty.Index))
            {
                for (int i = index + 1; i < Count; i++)
                    this[i].OnUpdated();
            }
        }

        private void OnRowRemoved(int index, DataRowPresenter row)
        {
            if (CurrentRow == row)
                CurrentRow = Count == 0 ? null : this[Math.Min(Count - 1, index)];

            if (ShouldFireRowUpdatedEvent(RowProperty.Index))
            {
                for (int i = index; i < Count; i++)
                    this[i].OnUpdated();
            }
        }

        private void OnRowUpdated(int index)
        {
            this[index].OnUpdated();
        }

        private readonly DataRowPresenter _owner;
        public DataRowPresenter Owner
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

        #region IReadOnlyList<DataRowPresenter>

        RowCollection _rows;

        public IEnumerator<DataRowPresenter> GetEnumerator()
        {
            return _rows.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _rows.GetEnumerator();
        }

        public int Count
        {
            get { return _rows.Count; }
        }

        public DataRowPresenter this[int index]
        {
            get { return _rows[index]; }
        }
        #endregion

        private DataRowPresenter _currentRow;
        public DataRowPresenter CurrentRow
        {
            get { return _currentRow; }
            set
            {
                if (_currentRow == value)
                    return;

                if (value != null && value.Owner != this)
                    throw new ArgumentException(Strings.DataSetPresenter_InvalidCurrentRow, nameof(value));

                if (_currentRow != null)
                    _currentRow.IsCurrent = false;

                _currentRow = value;

                if (_currentRow != null)
                    _currentRow.IsCurrent = true;
            }
        }

        internal HashSet<DataRowPresenter> _selectedRows = new HashSet<DataRowPresenter>();
        public IReadOnlyCollection<DataRowPresenter> SelectedRows
        {
            get { return _selectedRows; }
        }

        internal LayoutManager LayoutManager { get; private set; }
    }
}
