using System.Collections.Generic;

namespace DevZest.Data.Presenters.Services
{
    internal class SortService : ISortService
    {
        public DataPresenter DataPresenter { get; set; }

        private IReadOnlyList<IColumnComparer> _orderBy;
        public IReadOnlyList<IColumnComparer> OrderBy
        {
            get { return _orderBy; }
            set
            {
                _orderBy = value;
                DataPresenter.OrderBy = GetOrderBy(_orderBy);
            }
        }

        private static IComparer<DataRow> GetOrderBy(IReadOnlyList<IColumnComparer> orderBy)
        {
            if (orderBy == null || orderBy.Count == 0)
                return null;

            IDataRowComparer result = orderBy[0];
            for (int i = 1; i < orderBy.Count; i++)
                result = result.ThenBy(orderBy[i]);

            return result;
        }
    }
}
