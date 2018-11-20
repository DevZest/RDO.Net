using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DevZest.Data
{
    public abstract class PrimaryKey : ReadOnlyCollection<ColumnSort>, IReadOnlyList<Column>
    {
        protected PrimaryKey(params ColumnSort[] columns)
            : base(columns)
        {
            _parentModel = Verify(columns, nameof(columns));
        }

        private Model Verify(ColumnSort[] columns, string paramName)
        {
            columns.VerifyNotEmpty(paramName);

            Model result = null;
            for (int i = 0; i < columns.Length; i++)
            {
                var column = columns[i].Column;
                if (column == null)
                    throw new ArgumentNullException(string.Format("{0}[{1}].Column", paramName, i));

                var parentModel = column.ParentModel;

                if (i == 0)
                    result = parentModel;
                else if (result != parentModel)
                    throw new ArgumentException(DiagnosticMessages.PrimaryKey_ParentModelNotIdentical, string.Format("{0}[{1}].Column.Model", paramName, i));
            }

            return result;
        }

        public bool Contains(Column column)
        {
            for (int i = 0; i < Count; i++)
            {
                if (this[i].Column == column)
                    return true;
            }
            return false;
        }

        private readonly Model _parentModel;
        internal Model ParentModel
        {
            get { return _parentModel; }
        }

        Column IReadOnlyList<Column>.this[int index]
        {
            get { return this[index].Column; }
        }

        public IReadOnlyList<ColumnMapping> UnsafeJoin(PrimaryKey target)
        {
            target.VerifyNotNull(nameof(target));
            if (Count != target.Count)
                throw new ArgumentException(DiagnosticMessages.PrimaryKey_Join_ColumnsCountMismatch, nameof(target));

            var result = new ColumnMapping[Count];
            for (int i = 0; i < Count; i++)
            {
                var sourceColumn = this[i].Column;
                var targetColumn = target[i].Column;
                if (sourceColumn.DataType != targetColumn.DataType)
                    throw new ArgumentException(DiagnosticMessages.PrimaryKey_Join_ColumnDataTypeMismatch, string.Format("{0}[{1}]", nameof(target), i));
                result[i] = new ColumnMapping(sourceColumn, targetColumn);
            }

            return result;
        }

        IEnumerator<Column> IEnumerable<Column>.GetEnumerator()
        {
            foreach (var columnSort in this)
                yield return columnSort.Column;
        }
    }
}
