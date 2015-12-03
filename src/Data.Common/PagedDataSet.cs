namespace DevZest.Data
{
    public sealed class PagedDataSet<T> : ScalarData
        where T : Model, new()
    {
        public PagedDataSet()
        {
            _page = CreateAccessor<_Int32>(nameof(Page));
            _pageSize = CreateAccessor<_Int32>(nameof(PageSize));
            _totalCount = CreateAccessor<_Int32>(nameof(TotalCount));
            _data = CreateAccessor<_DataSet<T>>(nameof(Data));

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
            return ScalarData.ParseJson<PagedDataSet<T>>(json);
        }
    }
}
