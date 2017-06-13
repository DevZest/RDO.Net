using DevZest.Data;
using DevZest.Data.Primitives;
using System;
using System.Collections.Generic;

namespace DevZest.Windows
{
    public class ColumnSortDescriptor
    {
        public static ColumnSortDescriptor Create<T>(Column<T> column, SortDirection direction = SortDirection.Unspecified, IComparer<T> comparer = null)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            return new TypedColumnSortDescriptor<T>(column, direction, comparer);
        }

        private ColumnSortDescriptor(Column column, SortDirection direction)
        {
            _columnOrdinal = column.Ordinal;
            _isLocalColumn = column.IsLocal;
            _direction = direction;
        }

        private readonly int _columnOrdinal;
        private readonly bool _isLocalColumn;

        private readonly SortDirection _direction;
        public SortDirection Direction
        {
            get { return _direction; }
        }

        public Column GetColumn(Model model)
        {
            VerifyModel(model, nameof(model));
            return _isLocalColumn ? model.GetLocalColumns()[_columnOrdinal] : model.GetColumns()[_columnOrdinal];
        }

        public ColumnSort GetColumnSort(Model model)
        {
            VerifyModel(model, nameof(model));
            var column = GetColumn(model);
            if (Direction == SortDirection.Unspecified)
                return column;
            else if (Direction == SortDirection.Ascending)
                return column.Asc();
            else
                return column.Desc();
        }

        public virtual IDataRowComparer GetDataRowComparer(Model model)
        {
            VerifyModel(model, nameof(model));
            return DataRow.OrderBy(GetColumn(model), _direction);
        }

        internal void VerifyModel(Model model, string paramName)
        {
            if (model == null)
                throw new ArgumentNullException(paramName);
        }

        private sealed class TypedColumnSortDescriptor<T> : ColumnSortDescriptor
        {
            public TypedColumnSortDescriptor(Column<T> column, SortDirection direction, IComparer<T> comparer)
                : base(column, direction)
            {
                _comparer = comparer;
            }

            private readonly IComparer<T> _comparer;
            public IComparer<T> Comparer
            {
                get { return _comparer; }
            }

            public override IDataRowComparer GetDataRowComparer(Model model)
            {
                VerifyModel(model, nameof(model));
                return DataRow.OrderBy((Column<T>)GetColumn(model), Direction, _comparer);
            }
        }
    }
}
