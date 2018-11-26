using DevZest.Data.Primitives;

namespace DevZest.Data
{
    public sealed class PagedDataSet<T>
        where T : Model, new()
    {
        private sealed class _Model : Model
        {
            static _Model()
            {
                RegisterColumn((_Model _) => _.Page);
                RegisterColumn((_Model _) => _.PageSize);
                RegisterColumn((_Model _) => _.TotalCount);
                RegisterColumn((_Model _) => _.Data);
            }

            public _Int32 Page { get; private set; }
            public _Int32 PageSize { get; private set; }
            public _Int32 TotalCount { get; private set; }
            public _DataSet<T> Data { get; private set; }
        }

        public PagedDataSet()
        {
            _dataSet = DataSet<_Model>.New();
            _dataSet.AddRow();
            Page = PageSize = TotalCount = 0;
        }

        private readonly DataSet<_Model> _dataSet;

        public override string ToString()
        {
            return ToJsonString(true);
        }

        public string ToJsonString(bool isPretty)
        {
            return _dataSet.ToJsonString(isPretty);
        }

        private _Model _
        {
            get { return _dataSet._;  }
        }

        public int Page
        {
            get { return (int)_.Page[0]; }
            set { _.Page[0] = value; }
        }

        public int PageSize
        {
            get { return (int)_.PageSize[0]; }
            set { _.PageSize[0] = value; }
        }

        public int TotalCount
        {
            get { return (int)_.TotalCount[0]; }
            set { _.TotalCount[0] = value; }
        }

        public DataSet<T> Data
        {
            get { return _.Data[0]; }
            set { _.Data[0] = value; }
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
