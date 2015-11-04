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
        private class InnerCollection : KeyedCollection<string, Column>
        {
            protected override string GetKeyForItem(Column item)
            {
                return item.ColumnName;
            }
        }

        Dictionary<string, int> _columnNameSuffixes = new Dictionary<string, int>();
        Dictionary<ColumnKey, Column> _columns = new Dictionary<ColumnKey, Column>();

        internal ColumnCollection(Model model)
            : base(new InnerCollection())
        {
            Model = model;
        }

        private Model Model { get; set; }

        internal int SystemColumnCount { get; private set; }

        internal void Add(Column item)
        {
            Debug.Assert(Model.DataSource == null);

            var columnKey = item.Key;
            if (_columns.ContainsKey(columnKey))
                throw new InvalidOperationException(Strings.ColumnCollection_DuplicateColumnKey(columnKey.OriginalOwnerType, columnKey.OriginalName));

            item.ColumnName = _columnNameSuffixes.GetUniqueName(item.ColumnName); // Ensure ColumnName is unique
            _columns.Add(columnKey, item);
            base.Items.Add(item);
            if (item.IsSystem)
                SystemColumnCount ++;
        }

        /// <summary>Gets the <see cref="Column"/> with specified <see cref="ColumnKey"/>.</summary>
        /// <param name="columnKey">The <see cref="ColumnKey"/> which uniquely identifies the column.</param>
        /// <returns>Column with the specified <see cref="ColumnKey"/>, <see langword="null"/> if no column found.</returns>
        public Column this[ColumnKey columnKey]
        {
            get { return _columns.ContainsKey(columnKey) ? _columns[columnKey] : null; }
        }

        public Column this[string columnName]
        {
            get { return ((InnerCollection)Items)[columnName]; }
        }

        internal void Seal()
        {
            _columnNameSuffixes = null;
            foreach (var column in _columns.Values)
            {
                column.Seal();
            }
        }
    }
}
