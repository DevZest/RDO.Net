using DevZest.Data.Primitives;
using System;

namespace DevZest.Data
{
    public sealed class PagedDataSet<T>
        where T : Model, new()
    {
        private sealed class _Model : Model
        {
        }

        private readonly _Model _model = new _Model();
        private DataSet<_Model> _dataSet;

        private TColumn CreateColumn<TColumn>(string name, Action<TColumn> initializer = null)
            where TColumn : Column, new()
        {
            var result = new TColumn();
            if (initializer != null)
                initializer(result);
            result.Initialize(_model, typeof(_Model), name, ColumnKind.ModelProperty, null);
            return result;
        }

        private void EnsureDataSetInitialized()
        {
            if (_dataSet == null)
            {
                _dataSet = DataSet<_Model>.Create(_model);
                _dataSet.AddRow();
            }
        }

        private TColumn GetValue<TColumn>(Column<TColumn> column)
        {
            EnsureDataSetInitialized();
            return column[0];
        }

        private void SetValue<TColumn>(Column<TColumn> column, TColumn value)
        {
            EnsureDataSetInitialized();
            column[0] = value;
        }

        public override string ToString()
        {
            return ToJsonString(true);
        }

        public string ToJsonString(bool isPretty)
        {
            EnsureDataSetInitialized();
            return _dataSet.ToJsonString(isPretty);
        }

        public PagedDataSet()
        {
            _page = CreateColumn<_Int32>(nameof(Page));
            _pageSize = CreateColumn<_Int32>(nameof(PageSize));
            _totalCount = CreateColumn<_Int32>(nameof(TotalCount));
            _data = CreateColumn<_DataSet<T>>(nameof(Data));

            Page = PageSize = TotalCount = 0;
        }

        _Int32 _page;
        public int Page
        {
            get { return GetValue(_page).Value; }
            set { SetValue(_page, value); }
        }

        _Int32 _pageSize;
        public int PageSize
        {
            get { return GetValue(_pageSize).Value; }
            set { SetValue(_pageSize, value); }
        }

        _Int32 _totalCount;
        public int TotalCount
        {
            get { return GetValue(_totalCount).Value; }
            set { SetValue(_totalCount, value); }
        }

        _DataSet<T> _data;
        public DataSet<T> Data
        {
            get { return GetValue(_data); }
            set { SetValue(_data, value); }
        }

        public static PagedDataSet<T> ParseJson(string json)
        {
            json.VerifyNotEmpty(nameof(json));

            var result = new PagedDataSet<T>();
            var dataSet = result._dataSet;
            if (dataSet != null)
                dataSet.RemoveAt(0);
            new JsonParser(json).Parse(result._dataSet, true);
            return dataSet.Count == 1 ? result : null;
        }
    }
}
