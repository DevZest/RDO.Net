using DevZest.Data.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace DevZest.Data
{
    /// <summary>Represents a collection of <see cref="Column"/> objects contained by <see cref="Model"/>.</summary>
    public sealed class ColumnCollection : ReadOnlyCollection<Column>
    {
        private class InnerCollection : KeyedCollection<ColumnId, Column>
        {
            protected override ColumnId GetKeyForItem(Column item)
            {
                return item.ModelId;
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
            Debug.Assert(Model.DataSource == null);

            var columns = Columns;
            var modelId = item.ModelId;
            if (columns.Contains(modelId))
                throw new InvalidOperationException(Strings.ColumnCollection_DuplicateModelId(modelId.OwnerType, modelId.Name));

            columns.Add(item);
            if (item.IsSystem)
                SystemColumnCount ++;
        }

        /// <summary>Gets the <see cref="Column"/> with specified <see cref="ColumnId"/>.</summary>
        /// <param name="columnKey">The <see cref="ColumnId"/> which uniquely identifies the column.</param>
        /// <returns>Column with the specified <see cref="ColumnId"/>, <see langword="null"/> if no column found.</returns>
        public Column this[ColumnId columnKey]
        {
            get
            {
                var columns = Columns;
                return columns.Contains(columnKey) ? columns[columnKey] : null;
            }
        }

        public Column this[string name]
        {
            get
            {
                Check.NotEmpty(name, nameof(name));
                return Model[name] as Column;
            }
        }

        internal void InitDbColumnNames()
        {
            var columnNameSuffixes = new Dictionary<string, int>();
            var columns = Columns;
            foreach (var column in columns)
                column.InitDbColumnName(columnNameSuffixes);
        }

        internal IReadOnlyDictionary<ColumnId, IColumns> ByOriginalId()
        {
            var result = new Dictionary<ColumnId, IColumns>();

            for (int i = 0; i < Count; i++)
            {
                var column = this[i];
                var originalId = column.OriginalId;
                IColumns columns;
                if (result.TryGetValue(originalId, out columns))
                    result[originalId] = columns.Add(column);
                else
                    result[originalId] = column;
            }

            var keyValuePairs = result.ToArray();
            foreach (var keyValuePair in keyValuePairs)
                result[keyValuePair.Key] = keyValuePair.Value.Seal();
            return result;
        }
    }
}
