using System.Collections.Generic;

namespace DevZest.Data.Presenters.Services
{
    internal class SortService : ServiceBase, ISortService
    {
        private IReadOnlyList<IColumnComparer> _orderBy;
        public IReadOnlyList<IColumnComparer> OrderBy
        {
            get { return _orderBy; }
            set
            {
                _orderBy = value;
                DataPresenter.InvalidateView();
            }
        }
    }
}
