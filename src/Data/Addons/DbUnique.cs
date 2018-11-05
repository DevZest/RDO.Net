using DevZest.Data.Primitives;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DevZest.Data.Addons
{
    public sealed class DbUnique : DbTableConstraint, IIndexConstraint
    {
        internal DbUnique(string name, string description, bool isClustered, IList<ColumnSort> columns)
            : base(name, description)
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

        public override bool IsValidOnTable
        {
            get { return true; }
        }

        public override bool IsValidOnTempTable
        {
            get { return true; }
        }
    }
}
