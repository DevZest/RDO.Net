using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DevZest.Data
{
    /// <summary>
    /// Represents the candiate key of model.
    /// </summary>
    public abstract class CandidateKey : ReadOnlyCollection<ColumnSort>, IReadOnlyList<Column>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="CandidateKey"/>.
        /// </summary>
        /// <param name="columns">Consisted columns of this candicate key.</param>
        protected CandidateKey(params ColumnSort[] columns)
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

        /// <summary>
        /// Determines whether the specified column is part of this candidate key.
        /// </summary>
        /// <param name="column">The specified column.</param>
        /// <returns><see langword="true" /> if the specified column is part of this candidate key, otherwise <see langword="false"/>.</returns>
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

        /// <inheritdoc />
        Column IReadOnlyList<Column>.this[int index]
        {
            get { return this[index].Column; }
        }

        /// <summary>
        /// Join with target candidate key.
        /// </summary>
        /// <param name="target">The target candidate key.</param>
        /// <returns>Column mappings between this candidate key and the target candidate key.</returns>
        /// <remarks>You must ensure these two canidate key objects have identical columns, otherwise an exception will be thrown.</remarks>
        public IReadOnlyList<ColumnMapping> UnsafeJoin(CandidateKey target)
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

        /// <inheritdoc />
        IEnumerator<Column> IEnumerable<Column>.GetEnumerator()
        {
            foreach (var columnSort in this)
                yield return columnSort.Column;
        }
    }
}
