using DevZest.Data.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace DevZest.Data.Primitives
{
    internal abstract class DataRowComparer : IDataRowComparer
    {
        public static DataRowComparer Create(IDataRowComparer[] comparers)
        {
            return new CompositeComparer(comparers);
        }

        public static DataRowComparer Create(Column column, SortDirection direction)
        {
            if (column.ParentModel != null)
            {
                if (column.IsLocal)
                    return new LocalColumnComparer(column, direction);
                else
                    return new SimpleColumnComparer(column, direction);
            }
            else
                return new ExpressionColumnComparer(column, direction);
        }

        public static DataRowComparer Create<T>(Column<T> column, SortDirection direction, IComparer<T> comparer)
        {
            if (column.ParentModel != null)
            {
                if (column.IsLocal)
                    return new LocalColumnComparer<T>(column, direction, comparer);
                else
                    return new SimpleColumnComparer<T>(column, direction, comparer);
            }
            else
                return new ExpressionColumnComparer<T>(column, direction, comparer);

        }

        public abstract int Compare(DataRow x, DataRow y);

        public abstract Type ModelType { get; }

        protected Model Verify(DataRow x, DataRow y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            var model = x.Model;
            if (model == null || model.GetType() != ModelType)
                throw new ArgumentException(Strings.DataRowComparer_InvalidDataRowModel, nameof(x));
            if (y.Model != model)
                throw new ArgumentException(Strings.DataRowComparer_DifferentDataRowModel, nameof(y));
            return model;
        }

        private sealed class CompositeComparer : DataRowComparer
        {
            public CompositeComparer(IDataRowComparer[] comparers)
            {
                Debug.Assert(comparers.Length > 1);
                _comparers = comparers;
            }

            private readonly IDataRowComparer[] _comparers;

            public override int Compare(DataRow x, DataRow y)
            {
                Verify(x, y);
                for (int i = 0; i < _comparers.Length; i++)
                {
                    var comparer = _comparers[i];
                    var result = comparer.Compare(x, y);
                    if (result != 0)
                        return result;
                }

                return 0;
            }

            public override Type ModelType
            {
                get { return _comparers[0].ModelType; }
            }
        }

        private abstract class ColumnComparerBase : DataRowComparer
        {
            protected ColumnComparerBase(Column column, SortDirection direction)
            {
                Debug.Assert(column.ScalarSourceModels.Count == 1);
                _modelType = ((Model)column.ScalarSourceModels).GetType();
                Direction = direction;
            }

            private readonly Type _modelType;
            public sealed override Type ModelType
            {
                get { return _modelType; }
            }

            protected SortDirection Direction { get; private set; }
        }

        private abstract class ColumnComparer : ColumnComparerBase
        {
            protected ColumnComparer(Column column, SortDirection direction)
                : base(column, direction)
            {
            }

            protected abstract Column GetColumn(Model model);

            public sealed override int Compare(DataRow x, DataRow y)
            {
                var model = Verify(x, y);
                var result = GetColumn(model).Compare(x, y, Direction);
                return result;
            }
        }

        private abstract class MemberColumnComparer : ColumnComparer
        {
            public MemberColumnComparer(Column column, SortDirection direction)
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

        private sealed class SimpleColumnComparer : MemberColumnComparer
        {
            public SimpleColumnComparer(Column column, SortDirection direction)
                : base(column, direction)
            {
            }

            protected override IReadOnlyList<Column> GetColumnList(Model model)
            {
                return model.Columns;
            }
        }

        private sealed class LocalColumnComparer : MemberColumnComparer
        {
            public LocalColumnComparer(Column column, SortDirection direction)
                : base(column, direction)
            {
            }

            protected override IReadOnlyList<Column> GetColumnList(Model model)
            {
                return model.LocalColumns;
            }
        }

        private sealed class ExpressionColumnComparer : ColumnComparer
        {
            public ExpressionColumnComparer(Column column, SortDirection direction)
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

        private abstract class ColumnComparer<T> : ColumnComparerBase
        {
            protected ColumnComparer(Column<T> column, SortDirection direction, IComparer<T> comparer)
                : base(column, direction)
            {
                _comparer = comparer;
            }

            private readonly IComparer<T> _comparer;

            protected abstract Column<T> GetColumn(Model model);

            public sealed override int Compare(DataRow x, DataRow y)
            {
                var model = Verify(x, y);
                var result = GetColumn(model).Compare(x, y, Direction, _comparer);
                return result;
            }
        }

        private abstract class MemberColumnComparer<T> : ColumnComparer<T>
        {
            protected MemberColumnComparer(Column<T> column, SortDirection direction, IComparer<T> comparer)
                : base(column, direction, comparer)
            {
                _ordinal = column.Ordinal;
            }

            private readonly int _ordinal;

            protected abstract IReadOnlyList<Column> GetColumnList(Model model);

            protected sealed override Column<T> GetColumn(Model model)
            {
                return (Column<T>)GetColumnList(model)[_ordinal];
            }
        }

        private sealed class SimpleColumnComparer<T> : MemberColumnComparer<T>
        {
            public SimpleColumnComparer(Column<T> column, SortDirection direction, IComparer<T> comparer)
                : base(column, direction, comparer)
            {
            }

            protected override IReadOnlyList<Column> GetColumnList(Model model)
            {
                return model.Columns;
            }
        }

        private sealed class LocalColumnComparer<T> : MemberColumnComparer<T>
        {
            public LocalColumnComparer(Column<T> column, SortDirection direction, IComparer<T> comparer)
                : base(column, direction, comparer)
            {
            }

            protected override IReadOnlyList<Column> GetColumnList(Model model)
            {
                return model.LocalColumns;
            }
        }

        private sealed class ExpressionColumnComparer<T> : ColumnComparer<T>
        {
            public ExpressionColumnComparer(Column<T> column, SortDirection direction, IComparer<T> comparer)
                : base(column, direction, comparer)
            {
                _json = column.ToJson(false);
            }

            private readonly string _json;
            private readonly ConditionalWeakTable<Model, Column<T>> _columnsByModel = new ConditionalWeakTable<Model, Column<T>>();

            protected override Column<T> GetColumn(Model model)
            {
                return _columnsByModel.GetValue(model, CreateColumn);
            }

            private Column<T> CreateColumn(Model model)
            {
                return Column.ParseJson<Column<T>>(model, _json);
            }
        }
    }
}
