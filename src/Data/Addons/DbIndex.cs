using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace DevZest.Data.Addons
{
    /// <summary>
    /// Represents database index constraint.
    /// </summary>
    public sealed class DbIndex : IIndexConstraint, IAddon
    {
        internal DbIndex(string name, string description, bool isUnique, bool isClustered, bool isValidOnTable, bool isValidOnTempTable, IList<ColumnSort> columns)
        {
            Debug.Assert(!string.IsNullOrEmpty(name));
            Name = name;
            Description = description;
            IsUnique = isUnique;
            IsClustered = isClustered;
            IsValidOnTable = isValidOnTable;
            IsValidOnTempTable = isValidOnTempTable;
            Columns = new ReadOnlyCollection<ColumnSort>(columns);
        }

        /// <summary>
        /// Gets the name of this index.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the description of this index.
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Gets a value indicates whether this index is unique.
        /// </summary>
        public bool IsUnique { get; private set; }

        /// <summary>
        /// Gets a value indicates whether this is a clustered index.
        /// </summary>
        public bool IsClustered { get; private set; }

        /// <summary>
        /// Gets the columns with sort direction of this index.
        /// </summary>
        public ReadOnlyCollection<ColumnSort> Columns { get; private set; }

        void IIndexConstraint.AsNonClustered()
        {
            IsClustered = false;
        }

        /// <inheritdoc />
        public bool IsValidOnTable { get; }

        /// <inheritdoc />
        public bool IsValidOnTempTable { get; }

        string IIndexConstraint.SystemName
        {
            get { return Name; }
        }

        object IAddon.Key
        {
            get { return Name; }
        }
    }
}
