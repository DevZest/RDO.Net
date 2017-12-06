using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace DevZest.Data.Primitives
{
    public sealed class DbIndex : DbTableElement, IIndexConstraint, IExtension
    {
        internal DbIndex(string name, bool isUnique, bool isClustered, bool isMemberOfTable, bool isMemberOfTempTable, IList<ColumnSort> columns)
        {
            Debug.Assert(!string.IsNullOrEmpty(name));
            Name = name;
            IsUnique = isUnique;
            IsClustered = isClustered;
            _isMemberOfTable = isMemberOfTable;
            _isMemberOfTempTable = isMemberOfTempTable;
            Columns = new ReadOnlyCollection<ColumnSort>(columns);
        }

        public string Name { get; private set; }

        public bool IsUnique { get; private set; }

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

        string IIndexConstraint.SystemName
        {
            get { return Name; }
        }

        object IExtension.Key
        {
            get { return Name; }
        }
    }
}
