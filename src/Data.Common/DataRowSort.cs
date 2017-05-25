using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace DevZest.Data
{
    public abstract class DataRowSort : DataRowCriteria
    {
        public static DataRowSort Create<T>(Func<T, DataRow, DataRow, int> comparer)
            where T : Model
        {
            Check.NotNull(comparer, nameof(comparer));
            if (comparer.Target != null)
                throw new ArgumentException(Strings.DataRowCriteria_ExpressionMustBeStatic, nameof(comparer));
            return new DelegateSort<T>(comparer);
        }

        public static DataRowSort Create<T>(Func<DataRowComparing, T, DataRowCompared> comparer)
            where T : Model
        {
            Check.NotNull(comparer, nameof(comparer));
            if (comparer.Target != null)
                throw new ArgumentException(Strings.DataRowCriteria_ExpressionMustBeStatic, nameof(comparer));
            return new FluentDelegateSort<T>(comparer);
        }

        public static DataRowSort Create(Column column, SortDirection direction = SortDirection.Ascending)
        {
            Verify(column, nameof(column));
            return InternalCreate(column, direction);
        }

        private static DataRowSort InternalCreate(Column column, SortDirection direction = SortDirection.Ascending)
        {
            if (column.ParentModel != null)
            {
                if (column.IsLocal)
                    return new SortByLocalColumn(column, direction);
                else
                    return new SortBySimpleColumn(column, direction);
            }
            else
                return new SortByExpressionColumn(column, direction);
        }

        private static void Verify(Column column, string paramName, int index = -1)
        {
            if (column == null)
            {
                if (index == -1)
                    throw new ArgumentNullException(paramName);
                else
                    throw new ArgumentException(Strings.DataRowSort_NullColumn, GetParamName(paramName, index));
            }
            if (column.ScalarSourceModels.Count != 1)
                throw new ArgumentException(Strings.DataRowSort_InvalidColumnScalarSourceModels, GetParamName(paramName, index));
        }

        private static string GetParamName(string paramName, int index)
        {
            return index == -1 ? paramName : string.Format(CultureInfo.InvariantCulture, "{0}[{1}]", paramName, index);
        }

        public static DataRowSort Create(params ColumnSort[] orderBy)
        {
            Check.NotNull(orderBy, nameof(orderBy));
            if (orderBy.Length == 0)
                throw new ArgumentException(Strings.DataRowSort_EmptyOrderBy, nameof(orderBy));

            var sortItems = new DataRowSort[orderBy.Length];
            for (int i = 0; i < orderBy.Length; i++)
            {
                var columnSort = orderBy[i];
                Verify(columnSort.Column, nameof(orderBy), i);
                sortItems[i] = InternalCreate(columnSort.Column, columnSort.Direction);
                if (i > 0 && sortItems[i].ModelType != sortItems[0].ModelType)
                    throw new ArgumentException(Strings.DataRowSort_DifferentSortModelType, GetParamName(nameof(orderBy), i));
            }
            return new CompositeSort(sortItems);
        }

        private DataRowSort()
        {
        }

        public int Evaluate(DataRow x, DataRow y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            var model = x.Model;
            if (model == null || model.GetType() != ModelType)
                throw new ArgumentException(Strings.DataRowSort_InvalidDataRowModel, nameof(x));
            if (y.Model != model)
                throw new ArgumentException(Strings.DataRowSort_DifferentDataRowModel, nameof(y));
            return EvaluateCore(model, x, y);
        }

        protected abstract int EvaluateCore(Model model, DataRow x, DataRow y);

        private sealed class DelegateSort<T> : DataRowSort
            where T : Model
        {
            public DelegateSort(Func<T, DataRow, DataRow, int> comparer)
            {
                Debug.Assert(comparer != null && comparer.Target == null);
                _comparer = comparer;
            }

            private readonly Func<T, DataRow, DataRow, int> _comparer;

            protected override int EvaluateCore(Model model, DataRow x, DataRow y)
            {
                var _ = (T)model;
                return _comparer(_, x, y);
            }

            public override Type ModelType
            {
                get { return typeof(T); }
            }
        }

        private sealed class FluentDelegateSort<T> : DataRowSort
            where T : Model
        {
            public FluentDelegateSort(Func<DataRowComparing, T, DataRowCompared> comparer)
            {
                Debug.Assert(comparer != null && comparer.Target == null);
                _comparer = comparer;
            }

            private readonly Func<DataRowComparing, T, DataRowCompared> _comparer;

            protected override int EvaluateCore(Model model, DataRow x, DataRow y)
            {
                var _ = (T)model;
                return _comparer(new DataRowComparing(x, y), _).Result;
            }

            public override Type ModelType
            {
                get { return typeof(T); }
            }
        }

        private abstract class SortByColumn : DataRowSort
        {
            private readonly Type _modelType;

            protected SortByColumn(Column column, SortDirection direction)
            {
                Debug.Assert(column.ScalarSourceModels.Count == 1);
                _modelType = ((Model)column.ScalarSourceModels).GetType();
                _direction = direction;
            }

            private readonly SortDirection _direction;

            protected abstract Column GetColumn(Model model);

            public sealed override Type ModelType
            {
                get { return _modelType; }
            }

            protected sealed override int EvaluateCore(Model model, DataRow x, DataRow y)
            {
                var result = GetColumn(model).Compare(x, y);
                if (_direction == SortDirection.Descending)
                    result *= -1;
                return result;
            }
        }

        private abstract class SortByMemberColumn : SortByColumn
        {
            public SortByMemberColumn(Column column, SortDirection direction)
                : base(column, direction)
            {
                _ordinal = column.Ordinal;
            }

            private readonly int _ordinal;

            protected abstract IReadOnlyList<Column> GetColumnList(Model model);

            protected sealed override Column GetColumn(Model model)
            {
                return GetColumnList(model)[_ordinal];
            }
        }

        private sealed class SortBySimpleColumn : SortByMemberColumn
        {
            public SortBySimpleColumn(Column column, SortDirection direction)
                : base(column, direction)
            {
            }

            protected override IReadOnlyList<Column> GetColumnList(Model model)
            {
                return model.Columns;
            }
        }

        private sealed class SortByLocalColumn : SortByMemberColumn
        {
            public SortByLocalColumn(Column column, SortDirection direction)
                : base(column, direction)
            {
            }

            protected override IReadOnlyList<Column> GetColumnList(Model model)
            {
                return model.LocalColumns;
            }
        }

        private sealed class SortByExpressionColumn : SortByColumn
        {
            public SortByExpressionColumn(Column column, SortDirection direction)
                : base(column, direction)
            {
                _json = column.ToJson(false);
            }

            private readonly string _json;
            private readonly ConditionalWeakTable<Model, Column> _columnsByModel = new ConditionalWeakTable<Model, Column>();

            protected override Column GetColumn(Model model)
            {
                return _columnsByModel.GetValue(model, CreateColumn);
            }

            private Column CreateColumn(Model model)
            {
                return Column.ParseJson<Column>(model, _json);
            }
        }

        private sealed class CompositeSort : DataRowSort
        {
            public CompositeSort(DataRowSort[] sortItems)
            {
                _sortItems = sortItems;
            }

            private readonly DataRowSort[] _sortItems;

            protected sealed override int EvaluateCore(Model model, DataRow x, DataRow y)
            {
                for (int i = 0; i < _sortItems.Length; i++)
                {
                    var result = _sortItems[i].EvaluateCore(model, x, y);
                    if (result != 0)
                        return result;
                }
                return 0;
            }

            public override Type ModelType
            {
                get { return _sortItems[0].GetType(); }
            }
        }
    }
}
