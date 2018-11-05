using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace DevZest.Data.Addons
{
    public sealed class DbPrimaryKey : DbTableConstraint, IIndexConstraint
    {
        internal DbPrimaryKey(Model model, string name, string description, bool isClustered, Func<IList<ColumnSort>> getPrimaryKey)
            : base(name, description)
        {
            Debug.Assert(model != null);
            Debug.Assert(getPrimaryKey != null);

            Model = model;
            IsClustered = isClustered;
            _getPrimaryKey = getPrimaryKey;
        }

        public Model Model { get; private set; }

        public bool IsClustered { get; private set; }

        void IIndexConstraint.AsNonClustered()
        {
            IsClustered = false;
        }
        
        private Func<IList<ColumnSort>> _getPrimaryKey;

        private ReadOnlyCollection<ColumnSort> _primaryKey;
        public ReadOnlyCollection<ColumnSort> PrimaryKey
        {
            get
            {
                if (_primaryKey == null)
                {
                    _primaryKey = new ReadOnlyCollection<ColumnSort>(_getPrimaryKey());
                    _getPrimaryKey = null;
                }
                return _primaryKey;
            }
        }

        public override string SystemName
        {
            get { return string.IsNullOrEmpty(Name) ? "PrimaryKey" : Name; }
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
