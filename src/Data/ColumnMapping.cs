using DevZest.Data.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace DevZest.Data
{
    /// <summary>
    /// Defines the mapping between source column and target column.
    /// </summary>
    public struct ColumnMapping
    {
        /// <summary>
        /// Maps between two columns.
        /// </summary>
        /// <typeparam name="T">The data type of the column.</typeparam>
        /// <param name="source">The source column.</param>
        /// <param name="target">The target column.</param>
        /// <returns>The mapping between these two columns.</returns>
        public static ColumnMapping Map<T>(Column<T> source, Column<T> target)
        {
            source.VerifyNotNull(nameof(source));
            target.VerifyNotNull(nameof(target));
            return new ColumnMapping(source, target);
        }

        /// <summary>
        /// Maps between two columns unsafely.
        /// </summary>
        /// <param name="source">The source column.</param>
        /// <param name="target">The target column.</param>
        /// <returns>The mapping between these two columns.</returns>
        /// <remarks>You must ensure type safety of the columns, otherwise an exception may be thrown when using returned <see cref="ColumnMapping" />.</remarks>
        public static ColumnMapping UnsafeMap(Column source, Column target)
        {
            target.VerifyNotNull(nameof(target));
            return new ColumnMapping(source, target);
        }

        /// <summary>
        /// Maps between two entities.
        /// </summary>
        /// <typeparam name="TSource">Type of the source model.</typeparam>
        /// <typeparam name="TTarget">Type of the target model.</typeparam>
        /// <param name="source">The source model.</param>
        /// <param name="target">The target model.</param>
        /// <param name="columnMapper">Delegate to map between entities. If <see langword="null"/>, default value will be used to map insertable columns.</param>
        /// <param name="isInsertable">Specifies whether the columns are insertable.</param>
        /// <returns>A list of <see cref="ColumnMapping"/> between two entities.</returns>
        public static IReadOnlyList<ColumnMapping> Map<TSource, TTarget>(TSource source, TTarget target, Action<ColumnMapper, TSource, TTarget> columnMapper, bool isInsertable)
            where TSource : Model, new()
            where TTarget : Model, new()
        {
            target.VerifyNotNull(nameof(target));
            source.VerifyNotNull(nameof(source));

            if (columnMapper == null)
                return GetColumnMappings(source, target, isInsertable);

            var result = new ColumnMapper(source, target).Build(builder => columnMapper(builder, source, target));
            var columns = isInsertable ? target.GetInsertableColumns() : target.GetUpdatableColumns();
            var targetModelIds = new HashSet<ColumnId>(columns.Select(x => x.Id));
            foreach (var resultItem in result)
            {
                if (!targetModelIds.Contains(resultItem.Target.Id))
                    throw new InvalidOperationException(DiagnosticMessages.ColumnMapper_InvalidTarget(resultItem.Target));
            }

            return result;
        }

        private static List<ColumnMapping> GetColumnMappings(Model sourceModel, Model targetModel, bool isInsertable)
        {
            targetModel.VerifyNotNull(nameof(targetModel));
            sourceModel.VerifyNotNull(nameof(sourceModel));

            var result = new List<ColumnMapping>();
            var sourceColumns = sourceModel.Columns;
            var targetColumns = isInsertable ? targetModel.GetInsertableColumns() : targetModel.GetUpdatableColumns();
            foreach (var targetColumn in targetColumns)
            {
                if (targetColumn.IsSystem)
                    continue;
                var sourceColumn = sourceColumns.AutoSelect(targetColumn);
                if (sourceColumn != null)
                    result.Add(new ColumnMapping(sourceColumn, targetColumn));
            }

            return result;
        }

        internal ColumnMapping(Column source, Column target)
        {
            if (source == null)
                _source = DbConstantExpression.Null;
            else
                _source = source;

            Debug.Assert(target != null);
            _target = target;
        }

        internal ColumnMapping(DbExpression source, Column target)
        {
            Debug.Assert(source != null);
            Debug.Assert(target != null);
            _source = source;
            _target = target;
        }

        private readonly object _source;
        private Column _target;

        /// <summary>Gets the source <see cref="Column"/> of this mapping.</summary>
        /// <value>Returns <see langword="null"/> if source is expression.</value>
        public Column Source
        {
            get
            {
                var source = _source as Column;
                return source ?? CalculatedSource;
            }
        }

        private Column CalculatedSource
        {
            get
            {
                var columnExpression = SourceExpression as DbColumnExpression;
                return columnExpression == null ? null : columnExpression.Column;
            }
        }

        /// <summary>Gets the source <see cref="DbExpression"/> of this mapping.</summary>
        public DbExpression SourceExpression
        {
            get
            {
                var sourceExpression = _source as DbExpression;
                return sourceExpression ?? Source.DbExpression;
            }
        }

        private DbExpression CalculatedSourceExpression
        {
            get
            {
                var sourceColumn = Source;
                return sourceColumn == null ? null : sourceColumn.DbExpression;
            }
        }

        /// <summary>Gets the target <see cref="Column"/> of this mapping.</summary>
        public Column Target
        {
            get { return _target; }
        }

        /// <summary>Gets the target <see cref="DbColumnExpression"/> of this mapping.</summary>
        public DbColumnExpression TargetExpression
        {
            get { return _target.DbExpression as DbColumnExpression; }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "({0}, {1})", _source, _target);
        }

        /// <summary>
        /// Copies value from source DataRow to target DataRow.
        /// </summary>
        /// <param name="sourceDataRow">The source DataRow.</param>
        /// <param name="targetDataRow">The target DataRow.</param>
        public void CopyValue(DataRow sourceDataRow, DataRow targetDataRow)
        {
            Source.CopyValue(sourceDataRow, Target, targetDataRow);
        }
    }
}
