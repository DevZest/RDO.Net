using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DevZest.Data.Primitives
{
    public sealed class UniqueConstraint : DbTableConstraint, IIndexConstraint
    {
        internal UniqueConstraint(string name, bool isClustered, IList<ColumnSort> columns)
            : base(name)
        {
            IsClustered = isClustered;
            Columns = new ReadOnlyCollection<ColumnSort>(columns);
        }

        public bool IsClustered { get; private set; }

        public ReadOnlyCollection<ColumnSort> Columns { get; private set; }

        void IIndexConstraint.AsNonClustered()
        {
            IsClustered = false;
        }
    }
}
