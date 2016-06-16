using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DevZest.Data.Primitives
{
    public sealed class UniqueConstraint : DbTableConstraint, IIndexConstraint
    {
        internal UniqueConstraint(string name, bool isClustered, IList<OrderBy> columns)
            : base(name)
        {
            IsClustered = isClustered;
            Columns = new ReadOnlyCollection<OrderBy>(columns);
        }

        public bool IsClustered { get; private set; }

        public ReadOnlyCollection<OrderBy> Columns { get; private set; }

        void IIndexConstraint.AsNonClustered()
        {
            IsClustered = false;
        }
    }
}
