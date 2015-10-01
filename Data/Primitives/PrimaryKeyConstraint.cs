using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace DevZest.Data.Primitives
{
    public sealed class PrimaryKeyConstraint : DbTableConstraint, IIndexConstraint
    {
        internal PrimaryKeyConstraint(Model model, string name, bool isClustered, Func<IList<ColumnSort>> getPrimaryKey)
            : base(name)
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
    }
}
