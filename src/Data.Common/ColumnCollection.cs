using DevZest.Data.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace DevZest.Data
{
    /// <summary>Represents a collection of <see cref="Column"/> objects contained by <see cref="Model"/>.</summary>
    public sealed class ColumnCollection : ReadOnlyCollection<Column>
    {
        private class InnerCollection : KeyedCollection<ColumnKey, Column>
        {
            protected override ColumnKey GetKeyForItem(Column item)
            {
                return item.Key;
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
            var columnKey = item.Key;
            if (columns.Contains(columnKey))
                throw new InvalidOperationException(Strings.ColumnCollection_DuplicateColumnKey(columnKey.OriginalOwnerType, columnKey.OriginalName));

            columns.Add(item);
            if (item.IsSystem)
                SystemColumnCount ++;
        }

        /// <summary>Gets the <see cref="Column"/> with specified <see cref="ColumnKey"/>.</summary>
        /// <param name="columnKey">The <see cref="ColumnKey"/> which uniquely identifies the column.</param>
        /// <returns>Column with the specified <see cref="ColumnKey"/>, <see langword="null"/> if no column found.</returns>
        public Column this[ColumnKey columnKey]
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

        internal void Seal()
        {
            var columnNameSuffixes = new Dictionary<string, int>();
            var columns = Columns;
            foreach (var column in columns)
                column.Seal(columnNameSuffixes);
        }
    }
}
