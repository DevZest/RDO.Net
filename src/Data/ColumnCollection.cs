using DevZest.Data.Primitives;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace DevZest.Data
{
    /// <summary>Represents a collection of <see cref="Column"/> objects contained by <see cref="Model"/>.</summary>
    public sealed class ColumnCollection : ReadOnlyCollection<Column>, IAutoColumnSelector
    {
        private class InnerCollection : KeyedCollection<string, Column>
        {
            protected override string GetKeyForItem(Column item)
            {
                return item.FullName;
            }
        }

        internal ColumnCollection(Model model)
            : base(new InnerCollection())
        {
            Model = model;
        }

        private InnerCollection Columns
        {
            get { return (InnerCollection)Items; }
        }

        private Model Model { get; set; }

        internal int SystemColumnCount { get; private set; }

        internal void Add(Column item)
        {
            Debug.Assert(Model.DesignMode);

            var columns = Columns;
            if (columns.Contains(item.FullName))
                throw new InvalidOperationException(DiagnosticMessages.ColumnCollection_DuplicateModelId(item.DeclaringType, item.FullName));

            columns.Add(item);
            if (item.IsSystem)
                SystemColumnCount ++;
        }

        /// <summary>Gets the <see cref="Column"/> by specified <see cref="ColumnId"/>.</summary>
        /// <param name="columnId">The <see cref="ColumnId"/> which uniquely identifies the column.</param>
        /// <returns>Column with the specified <see cref="ColumnId"/>, <see langword="null"/> if no column found.</returns>
        public Column this[ColumnId columnId]
        {
            get
            {
                var result = this[columnId.Name];
                return result != null && result.DeclaringType == columnId.DeclaringType ? result : null;
            }
        }

        /// <summary>
        /// Gets the <see cref="Column"/> by specified name.
        /// </summary>
        /// <param name="columnFullName">The specified name.</param>
        /// <returns>Column with the specified name, <see langword="null"/> if no column found.</returns>
        public Column this[string columnFullName]
        {
            get
            {
                columnFullName.VerifyNotEmpty(nameof(columnFullName));

                var columns = Columns;
                return columns.Contains(columnFullName) ? columns[columnFullName] : null;
            }
        }

        internal void InitDbColumnNames()
        {
            var columnNameSuffixes = new Dictionary<string, int>();
            var columns = Columns;
            foreach (var column in columns)
                column.InitDbColumnName(columnNameSuffixes);
        }

        private IReadOnlyDictionary<ColumnId, IColumns> _byOriginalId;
        private IReadOnlyDictionary<ColumnId, IColumns> ByOriginalId
        {
            get
            {
                if (_byOriginalId == null)
                    _byOriginalId = GetColumnsByOriginalId();
                return _byOriginalId;
            }
        }

        private IReadOnlyDictionary<ColumnId, IColumns> GetColumnsByOriginalId()
        {
            var result = new Dictionary<ColumnId, IColumns>();

            for (int i = 0; i < Count; i++)
            {
                var column = this[i];
                var originalId = column.OriginalId;
                if (result.TryGetValue(originalId, out var columns))
                    result[originalId] = columns.Add(column);
                else
                    result[originalId] = column;
            }

            var keyValuePairs = result.ToArray();
            foreach (var keyValuePair in keyValuePairs)
                result[keyValuePair.Key] = keyValuePair.Value.Seal();
            return result;
        }

        internal Column AutoSelect(Column column)
        {
            Debug.Assert(column != null);
            var result = this[column.Id];
            if (result != null)
                return result;

            if (ByOriginalId.TryGetValue(column.OriginalId, out var columns))
            {
                if (columns.Count == 1)
                    return columns.Single();
            }

            return null;
        }

        Column IAutoColumnSelector.Select(Column column)
        {
            return AutoSelect(column);
        }

        IAutoColumnSelector IAutoColumnSelector.Merge(IEnumerable<Column> selector)
        {
            return new AutoColumnSelector(this, selector);
        }
    }
}
