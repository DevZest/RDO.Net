using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DevZest.Data.Addons
{
    /// <summary>
    /// Represents database unique constraint.
    /// </summary>
    public sealed class DbUniqueConstraint : DbTableConstraint, IIndexConstraint
    {
        internal DbUniqueConstraint(string name, string description, bool isClustered, IList<ColumnSort> columns)
            : base(name, description)
        {
            IsClustered = isClustered;
            Columns = new ReadOnlyCollection<ColumnSort>(columns);
        }

        /// <summary>
        /// Gets a value indicates whether this unique constraint is clustered.
        /// </summary>
        public bool IsClustered { get; private set; }

        /// <summary>
        /// Gets the columns with sort direction of this unique constraint.
        /// </summary>
        public ReadOnlyCollection<ColumnSort> Columns { get; private set; }

        void IIndexConstraint.AsNonClustered()
        {
            IsClustered = false;
        }

        /// <inheritdoc />
        public override bool IsValidOnTable
        {
            get { return true; }
        }

        /// <inheritdoc />
        public override bool IsValidOnTempTable
        {
            get { return true; }
        }
    }
}
