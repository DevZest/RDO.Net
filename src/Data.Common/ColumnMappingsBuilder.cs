using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data
{
    /// <summary>Class to build a collection of <see cref="ColumnMapping"/> objects.</summary>
    public sealed class ColumnMappingsBuilder
    {
        public ColumnMappingsBuilder(Model sourceModel, Model targetModel)
        {
            Debug.Assert(targetModel != null);

            _sourceModel = sourceModel;
            _targetModel = targetModel;
        }

        private Model _sourceModel;
        private Model _targetModel;

        private List<ColumnMapping> _result = new List<ColumnMapping>();
        internal IList<ColumnMapping> Build(Action<ColumnMappingsBuilder> buildAction)
        {
            Debug.Assert(buildAction != null);

            buildAction(this);
            if (_result.Count == 0)
                throw new InvalidOperationException(Strings.ColumnMappingsBuilder_NoColumnMapping);
            return _result;
        }

        /// <summary>Build the <see cref="ColumnMapping"/> between source column and target column.</summary>
        /// <typeparam name="T">Data type of the columns.</typeparam>
        /// <param name="targetColumn">The target column.</param>
        /// <param name="sourceColumn">The source column.</param>
        /// <returns>This <see cref="ColumnMappingsBuilder"/>.</returns>
        /// <overloads>Build the <see cref="ColumnMapping"/>.</overloads>
        public ColumnMappingsBuilder Select<T>(Column<T> sourceColumn, Column<T> targetColumn)
        {
            VerifySource(sourceColumn);
            VerifyTarget(targetColumn);
            _result.Add(ColumnMapping.Map(sourceColumn, targetColumn));
            return this;
        }

        /// <summary>Build the <see cref="ColumnMapping"/> between source column and target column ordinal.</summary>
        /// <param name="targetColumnOrdinal">The target column ordinal.</param>
        /// <param name="sourceColumn">The source column.</param>
        /// <returns>This <see cref="ColumnMappingsBuilder"/>.</returns>
        public ColumnMappingsBuilder Select(Column sourceColumn, int targetColumnOrdinal)
        {
            var targetColumns = _targetModel.Columns;

            if (targetColumnOrdinal < 0 || targetColumnOrdinal >= targetColumns.Count)
                throw new ArgumentOutOfRangeException(nameof(targetColumnOrdinal));

            var targetColumn = targetColumns[targetColumnOrdinal];
            VerifySource(sourceColumn);
            if (sourceColumn.DataType != targetColumn.DataType)
                throw new ArgumentException(Strings.ColumnMappingsBuilder_InvalidSourceDataType(sourceColumn.DataType, targetColumn.DataType), nameof(sourceColumn));

            _result.Add(new ColumnMapping(sourceColumn, targetColumn));
            return this;
        }

        private void VerifyTarget(Column targetColumn)
        {
            Check.NotNull(targetColumn, nameof(targetColumn));
            if (targetColumn.ParentModel != _targetModel)
                throw new ArgumentException(Strings.ColumnMappingsBuilder_InvalidTarget(targetColumn), nameof(targetColumn));
        }

        private void VerifySource(Column sourceColumn)
        {
            Check.NotNull(sourceColumn, nameof(sourceColumn));

            var parentModelSet = sourceColumn.ParentModelSet;
            foreach (var parentModel in parentModelSet)
            {
                if (parentModel != _sourceModel)
                    throw new ArgumentException(Strings.ColumnMappingsBuilder_InvalidSourceParentModelSet(parentModel), nameof(sourceColumn));
            }
        }
    }
}
