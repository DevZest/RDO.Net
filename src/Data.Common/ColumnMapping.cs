using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace DevZest.Data
{
    /// <summary>
    /// Defines the mapping between source column or expression and target column.
    /// </summary>
    public struct ColumnMapping
    {
        public static ColumnMapping Map<T>(Column<T> source, Column<T> target)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(target, nameof(target));
            return new ColumnMapping(source, target);
        }

        public static IList<ColumnMapping> Map<TSource, TTarget>(TSource sourceModel, TTarget targetModel, Action<ColumnMappingsBuilder, TSource, TTarget> columnMappingsBuilder, bool isInsertable)
            where TSource : Model, new()
            where TTarget : Model, new()
        {
            Check.NotNull(targetModel, nameof(targetModel));
            Check.NotNull(sourceModel, nameof(sourceModel));

            if (columnMappingsBuilder == null)
                return GetColumnMappings(sourceModel, targetModel, isInsertable);

            var result = new ColumnMappingsBuilder(sourceModel, targetModel).Build(builder => columnMappingsBuilder(builder, sourceModel, targetModel));
            var columns = isInsertable ? targetModel.GetInsertableColumns() : targetModel.GetUpdatableColumns();
            var columnKeys = new HashSet<ColumnKey>(columns.Select(x => x.Key));
            foreach (var resultItem in result)
            {
                if (!columnKeys.Contains(resultItem.Target.Key))
                    throw new InvalidOperationException(Strings.ColumnMappingsBuilder_InvalidTarget(resultItem.Target));
            }

            return result;
        }

        private static List<ColumnMapping> GetColumnMappings(Model sourceModel, Model targetModel, bool isInsertable)
        {
            Check.NotNull(targetModel, nameof(targetModel));
            Check.NotNull(sourceModel, nameof(sourceModel));

            var result = new List<ColumnMapping>();
            var sourceColumns = sourceModel.Columns;
            var columns = isInsertable ? targetModel.GetInsertableColumns() : targetModel.GetUpdatableColumns();
            foreach (var column in columns)
            {
                if (column.IsSystem)
                    continue;
                var sourceColumn = sourceColumns[column.Key];
                if (sourceColumn != null)
                    result.Add(new ColumnMapping(sourceColumn, column));
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

        public void CopyValue(DataRow sourceDataRow, DataRow targetDataRow)
        {
            Source.CopyValue(sourceDataRow, Target, targetDataRow);
        }
    }
}
