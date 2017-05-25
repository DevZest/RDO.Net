using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            Check.NotNull(column, nameof(column));
            if (column.ScalarSourceModels.Count != 1)
                throw new ArgumentException(Strings.DataRowSort_InvalidColumnScalarSourceModels, nameof(column));

            if (column.ParentModel != null)
            {
                if (column.IsLocal)
                    return new LocalColumnSort(column, direction);
                else
                    return new SimpleColumnSort(column, direction);
            }
            else
                return new ExpressionColumnSort(column, direction);
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

        private abstract class ColumnSort : DataRowSort
        {
            private readonly Type _modelType;

            protected ColumnSort(Column column, SortDirection direction)
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

        private abstract class MemberColumnSort : ColumnSort
        {
            public MemberColumnSort(Column column, SortDirection direction)
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

        private sealed class SimpleColumnSort : MemberColumnSort
        {
            public SimpleColumnSort(Column column, SortDirection direction)
                : base(column, direction)
            {
            }

            protected override IReadOnlyList<Column> GetColumnList(Model model)
            {
                return model.Columns;
            }
        }

        private sealed class LocalColumnSort : MemberColumnSort
        {
            public LocalColumnSort(Column column, SortDirection direction)
                : base(column, direction)
            {
            }

            protected override IReadOnlyList<Column> GetColumnList(Model model)
            {
                return model.LocalColumns;
            }
        }

        private sealed class ExpressionColumnSort : ColumnSort
        {
            public ExpressionColumnSort(Column column, SortDirection direction)
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
    }
}
