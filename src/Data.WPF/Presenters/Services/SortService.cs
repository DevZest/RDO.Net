using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace DevZest.Data.Presenters.Services
{
    internal class SortService : ServiceBase, ISortService
    {
        private IColumnComparer[] _columnComparers;
        private ConditionalWeakTable<Model, IReadOnlyList<ColumnSort>> _orderBy;

        private IColumnComparer[] ColumnComparers
        {
            set
            {
                _columnComparers = value;
                _orderBy = null;
                DataPresenter.InvalidateView();
            }
        }


        private Model Model
        {
            get { return DataPresenter.DataSet.Model; }
        }

        public void Apply(IReadOnlyList<IColumnComparer> orderBy)
        {
            ColumnComparers = Convert(orderBy, nameof(orderBy));
        }

        private IColumnComparer[] Convert(IReadOnlyList<IColumnComparer> orderByList, string paramName)
        {
            if (orderByList == null || orderByList.Count == 0)
                return null;

            var result = new IColumnComparer[orderByList.Count];
            for (int i = 0; i < orderByList.Count; i++)
            {
                var orderBy = orderByList[i];
                if (orderBy == null)
                    throw new ArgumentNullException(string.Format(CultureInfo.InvariantCulture, "{0}[{1}]", paramName, i));
                result[i] = orderBy;
            }
            return result;
        }

        public IReadOnlyList<ColumnSort> OrderBy
        {
            get
            {
                if (_orderBy == null)
                    _orderBy = new ConditionalWeakTable<Model, IReadOnlyList<ColumnSort>>();
                return _orderBy.GetValue(Model, CreateOrderBy);
            }
            set { ColumnComparers = Convert(value); }
        }

        private IReadOnlyList<ColumnSort> CreateOrderBy(Model model)
        {
            if (_columnComparers == null || _columnComparers.Length == 0)
                return Array<ColumnSort>.Empty;

            var result = new ColumnSort[_columnComparers.Length];
            for (int i = 0; i < _columnComparers.Length; i++)
            {
                var columnComparer = _columnComparers[i];
                var column = columnComparer.GetColumn(model);
                var direction = columnComparer.Direction;
                result[i] = direction == SortDirection.Descending ? column.Desc() : column.Asc();
            }
            return result;
        }

        private IColumnComparer[] Convert(IReadOnlyList<ColumnSort> orderByList)
        {
            if (orderByList == null || orderByList.Count == 0)
                return Array<IColumnComparer>.Empty;

            var result = new IColumnComparer[orderByList.Count];
            for (int i = 0; i < orderByList.Count; i++)
            {
                var orderBy = orderByList[i];
                result[i] = DataRow.OrderBy(orderBy.Column, orderBy.Direction);
            }
            return result;
        }
    }
}
