using DevZest.Data.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data
{
    /// <summary>Class to build a collection of <see cref="ColumnMapping"/> objects.</summary>
    public sealed class ColumnMapper
    {
        public ColumnMapper(Model sourceModel, Model targetModel)
        {
            Debug.Assert(targetModel != null);

            _sourceModel = sourceModel;
            _targetModel = targetModel;
        }

        private Model _sourceModel;
        private Model _targetModel;

        private List<ColumnMapping> _result = new List<ColumnMapping>();
        internal IReadOnlyList<ColumnMapping> Build(Action<ColumnMapper> buildAction)
        {
            Debug.Assert(buildAction != null);

            buildAction(this);
            if (_result.Count == 0)
                throw new InvalidOperationException(DiagnosticMessages.ColumnMapper_EmptyResult);
            return _result;
        }

        /// <summary>Build the <see cref="ColumnMapping"/> between source column and target column.</summary>
        /// <typeparam name="T">Data type of the columns.</typeparam>
        /// <param name="targetColumn">The target column.</param>
        /// <param name="sourceColumn">The source column.</param>
        /// <returns>This <see cref="ColumnMapper"/>.</returns>
        /// <overloads>Build the <see cref="ColumnMapping"/>.</overloads>
        public ColumnMapper Select<T>(Column<T> sourceColumn, Column<T> targetColumn)
        {
            VerifySource(sourceColumn, nameof(sourceColumn));
            VerifyTarget(targetColumn, nameof(targetColumn));
            _result.Add(ColumnMapping.Map(sourceColumn, targetColumn));
            return this;
        }

        /// <summary>Build the <see cref="ColumnMapping"/> between source column and target column ordinal.</summary>
        /// <param name="targetColumnOrdinal">The target column ordinal.</param>
        /// <param name="sourceColumn">The source column.</param>
        /// <returns>This <see cref="ColumnMapper"/>.</returns>
        public ColumnMapper Select(Column sourceColumn, int targetColumnOrdinal)
        {
            var targetColumns = _targetModel.Columns;

            if (targetColumnOrdinal < 0 || targetColumnOrdinal >= targetColumns.Count)
                throw new ArgumentOutOfRangeException(nameof(targetColumnOrdinal));

            var targetColumn = targetColumns[targetColumnOrdinal];
            VerifySource(sourceColumn, nameof(sourceColumn));
            if (sourceColumn.DataType != targetColumn.DataType)
                throw new ArgumentException(DiagnosticMessages.ColumnMapper_InvalidSourceDataType(sourceColumn.DataType, targetColumn.DataType), nameof(sourceColumn));

            _result.Add(new ColumnMapping(sourceColumn, targetColumn));
            return this;
        }

        public ColumnMapper AutoSelectInsertable()
        {
            return AutoSelect(_targetModel.GetInsertableColumns());
        }

        public ColumnMapper AutoSelectUpdatable()
        {
            return AutoSelect(_targetModel.GetUpdatableColumns());
        }

        public ColumnMapper AutoSelect(IEnumerable<Column> targetColumns)
        {
            var sourceColumns = _sourceModel.Columns;
            foreach (var targetColumn in targetColumns)
            {
                if (targetColumn.IsSystem)
                    continue;
                var sourceColumn = sourceColumns.AutoSelect(targetColumn);
                if (sourceColumn != null)
                    _result.Add(new ColumnMapping(sourceColumn, targetColumn));
            }
            return this;
        }

        private void VerifyTarget(Column targetColumn, string paramName)
        {
            targetColumn.VerifyNotNull(paramName);
            if (targetColumn.ParentModel != _targetModel)
                throw new ArgumentException(DiagnosticMessages.ColumnMapper_InvalidTarget(targetColumn), paramName);
        }

        private void VerifySource(Column sourceColumn, string paramName)
        {
            sourceColumn.VerifyNotNull(paramName);

            var sourceModels = sourceColumn.ScalarSourceModels;
            foreach (var sourceModel in sourceModels)
            {
                if (sourceModel != _sourceModel && sourceModel != _targetModel)
                    throw new ArgumentException(DiagnosticMessages.ColumnMapper_InvalidSourceParentModelSet(sourceModel), paramName);
            }
        }

        public static void AutoSelectInsertable<T>(ColumnMapper columnMapper, T source, T target)
        {
            columnMapper.AutoSelectInsertable();
        }

        public static void AutoSelectUpdatable<T>(ColumnMapper columnMapper, T source, T target)
        {
            columnMapper.AutoSelectUpdatable();
        }
    }
}
