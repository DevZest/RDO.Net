using DevZest.Data;
using DevZest.Data.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Windows
{
    public class ColumnComparer
    {
        public static ColumnComparer Create(Column column)
        {
            VerifyColumn(column, nameof(column));
            return new ColumnComparer(column);
        }

        public static ColumnComparer Create<T>(Column<T> column, IComparer<T> comparer = null)
        {
            VerifyColumn(column, nameof(column));
            return new TypedColumnComparer<T>(column, comparer);
        }

        private static void VerifyColumn(Column column, string paramName)
        {
            if (column == null)
                throw new ArgumentNullException(paramName);
            if (column.GetParentModel() == null)
                throw new ArgumentException(Strings.ColumnComparer_InvalidColumn, paramName);
        }

        private ColumnComparer(Column column)
        {
            _modelType = column.GetParentModel().GetType();
            _columnOrdinal = column.Ordinal;
            _isLocalColumn = column.IsLocal;
        }

        private readonly Type _modelType;
        private readonly int _columnOrdinal;
        private readonly bool _isLocalColumn;

        private Column GetColumn(Model model)
        {
            Debug.Assert(model != null);
            return _isLocalColumn ? model.GetLocalColumns()[_columnOrdinal] : model.GetColumns()[_columnOrdinal];
        }

        public bool Contains(Column column)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            return column.GetParentModel().GetType() == _modelType && column.Ordinal == _columnOrdinal && column.IsLocal == _isLocalColumn;
        }

        public ColumnSort GetColumnSort(Model model, SortDirection direction)
        {
            VerifyModel(model, nameof(model));
            var column = GetColumn(model);
            if (direction == SortDirection.Unspecified)
                return column;
            else if (direction == SortDirection.Ascending)
                return column.Asc();
            else
                return column.Desc();
        }

        public virtual IDataRowComparer GetDataRowComparer(Model model, SortDirection direction)
        {
            VerifyModel(model, nameof(model));
            return DataRow.OrderBy(GetColumn(model), direction);
        }

        private void VerifyModel(Model model, string paramName)
        {
            if (model == null)
                throw new ArgumentNullException(paramName);
            if (model.GetType() != _modelType)
                throw new ArgumentException(Strings.ColumnComparer_InvalidModelType(_modelType), paramName);
        }

        private sealed class TypedColumnComparer<T> : ColumnComparer
        {
            public TypedColumnComparer(Column<T> column, IComparer<T> comparer)
                : base(column)
            {
                _comparer = comparer;
            }

            private readonly IComparer<T> _comparer;
            public IComparer<T> Comparer
            {
                get { return _comparer; }
            }

            public override IDataRowComparer GetDataRowComparer(Model model, SortDirection direction)
            {
                VerifyModel(model, nameof(model));
                return DataRow.OrderBy((Column<T>)GetColumn(model), direction, _comparer);
            }
        }
    }
}
