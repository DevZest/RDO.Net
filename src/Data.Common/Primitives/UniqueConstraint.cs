using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DevZest.Data.Primitives
{
    public sealed class UniqueConstraint : DbTableConstraint, IIndexConstraint
    {
        internal UniqueConstraint(string name, bool isClustered, bool isMemberOfTable, bool isMemberOfTempTable, IList<ColumnSort> columns)
            : base(name)
        {
            IsClustered = isClustered;
            Columns = new ReadOnlyCollection<ColumnSort>(columns);
            _isMemberOfTable = isMemberOfTable;
            _isMemberOfTempTable = isMemberOfTempTable;
        }

        public bool IsClustered { get; private set; }

        public ReadOnlyCollection<ColumnSort> Columns { get; private set; }

        void IIndexConstraint.AsNonClustered()
        {
            IsClustered = false;
        }

        private bool _isMemberOfTable;
        public override bool IsMemberOfTable
        {
            get { return _isMemberOfTable; }
        }

        private bool _isMemberOfTempTable;
        public override bool IsMemberOfTempTable
        {
            get { return _isMemberOfTempTable; }
        }
    }
}
