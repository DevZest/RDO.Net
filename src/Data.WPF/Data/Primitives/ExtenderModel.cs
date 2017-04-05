using DevZest.Data;
using DevZest.Data.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Windows.Data.Primitives
{
    internal class ExtenderModel : Adhoc
    {
        private sealed class ExtenderColumn<T> : Column<T>
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

            protected override IDataRowProxy DataRowProxy
            {
                get { return ((ExtenderModel)this.GetParentModel()).Template.RowManager; }
            }
        }

        private sealed class ExtenderDataRow : DataRow
        {
            public ExtenderDataRow()
            {
            }

            public DataRow DataRow { get; private set; }

            public void Initialize(DataRow dataRow)
            {
                DataRow = dataRow;
            }
        }

        private readonly Dictionary<DataRow, ExtenderDataRow> _dataRowToExtenderMap = new Dictionary<DataRow, ExtenderDataRow>();

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
            return base.AddColumn<ExtenderColumn<T>>(x => x.IsNullChecker = isNullChecker);
        }

        internal DataRow GetExtenderDataRow(DataRow dataRow)
        {
            ExtenderDataRow result;
            _dataRowToExtenderMap.TryGetValue(dataRow, out result);
            return result;
        }

        internal void Initialize(IEnumerable<DataRow> dataRows)
        {
            _dataRowToExtenderMap.Clear();
            DataSet.Clear();

            foreach (var dataRow in dataRows)
            {
                var extenderDataRow = new ExtenderDataRow();
                DataSet.Add(extenderDataRow);
                _dataRowToExtenderMap.Add(dataRow, extenderDataRow);
                extenderDataRow.Initialize(dataRow);
            }
        }

        internal void OnDataRowAdding(DataRow dataRow)
        {
            var extenderDataRow = new ExtenderDataRow();
            DataSet.Add(extenderDataRow);
            _dataRowToExtenderMap.Add(dataRow, extenderDataRow);
        }

        internal void OnDataRowAdded(DataRow dataRow)
        {
            Debug.Assert(_dataRowToExtenderMap.ContainsKey(dataRow));
            var extenderDataRow = _dataRowToExtenderMap[dataRow];
            extenderDataRow.Initialize(dataRow);
        }

        internal void OnDataRowRemoved(DataRow dataRow)
        {
            Debug.Assert(_dataRowToExtenderMap.ContainsKey(dataRow));
            var extenderDataRow = _dataRowToExtenderMap[dataRow];
            DataSet.Remove(extenderDataRow);
            _dataRowToExtenderMap.Remove(dataRow);
        }
    }
}
