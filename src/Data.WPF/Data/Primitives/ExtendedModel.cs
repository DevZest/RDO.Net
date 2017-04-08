using DevZest.Data;
using DevZest.Data.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Windows.Data.Primitives
{
    internal class ExtendedModel : Adhoc
    {
        private sealed class ExtendedColumn<T> : Column<T>
        {
            public override _String CastToString()
            {
                throw new NotSupportedException();
            }

            protected override Column<T> CreateConst(T value)
            {
                throw new NotSupportedException();
            }

            protected override Column<T> CreateParam(T value)
            {
                throw new NotSupportedException();
            }

            protected override T DeserializeValue(JsonValue value)
            {
                throw new NotSupportedException();
            }

            internal Func<T, bool> IsNullChecker;
            protected override bool IsNull(T value)
            {
                return IsNullChecker == null ? base.IsNull(value) : IsNullChecker(value);
            }

            protected override JsonValue SerializeValue(T value)
            {
                throw new NotImplementedException();
            }
        }

        private sealed class ExtendedDataRow : DataRow
        {
            public ExtendedDataRow()
            {
            }

            public DataRow Origin { get; private set; }

            public void Initialize(DataRow origin)
            {
                Origin = origin;
            }
        }

        private Dictionary<DataRow, ExtendedDataRow> _extendedDataRows;

        internal Template Template { get; private set; }

        internal void Initialize(Template template)
        {
            Template = template;
        }

        private RowManager RowManager
        {
            get { return Template.RowManager; }
        }

        internal Column<T> AddColumn<T>(Func<T, bool> isNullChecker)
        {
            return base.AddColumn<ExtendedColumn<T>>(x => x.IsNullChecker = isNullChecker);
        }

        internal DataRow GetExtendedDataRow(DataRow dataRow)
        {
            ExtendedDataRow result;
            _extendedDataRows.TryGetValue(dataRow, out result);
            return result;
        }

        internal void Initialize(IEnumerable<DataRow> dataRows)
        {
            if (_extendedDataRows == null)
                _extendedDataRows = new Dictionary<DataRow, ExtendedDataRow>();
            else
            {
                _extendedDataRows.Clear();
                DataSet.Clear();
            }

            foreach (var dataRow in dataRows)
            {
                var extendedDataRow = new ExtendedDataRow();
                DataSet.Add(extendedDataRow);
                _extendedDataRows.Add(dataRow, extendedDataRow);
                extendedDataRow.Initialize(dataRow);
            }
        }

        internal void OnOriginalDataRowAdding(DataRow dataRow)
        {
            var extendedDataRow = new ExtendedDataRow();
            DataSet.Add(extendedDataRow);
            _extendedDataRows.Add(dataRow, extendedDataRow);
        }

        internal void OnOriginalDataRowAdded(DataRow dataRow)
        {
            Debug.Assert(_extendedDataRows.ContainsKey(dataRow));
            var extendedDataRow = _extendedDataRows[dataRow];
            extendedDataRow.Initialize(dataRow);
        }

        internal void OnOriginalDataRowRemoved(DataRow dataRow)
        {
            Debug.Assert(_extendedDataRows.ContainsKey(dataRow));
            var extendedDataRow = _extendedDataRows[dataRow];
            DataSet.Remove(extendedDataRow);
            _extendedDataRows.Remove(dataRow);
        }

        protected override IDataRowProxy DataRowProxy
        {
            get { return RowManager; }
        }

        protected override bool RefreshComputation<T>(Column<T> column, DataRow dataRow)
        {
            var extendedDataRow = (ExtendedDataRow)dataRow;
            return extendedDataRow.Origin == null ? false : base.RefreshComputation<T>(column, extendedDataRow.Origin);
        }
    }
}
