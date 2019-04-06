using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace DevZest.Data.Addons
{
    /// <summary>
    /// Represents the database primary key constraint.
    /// </summary>
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

        /// <summary>
        /// Gets the model of the primary key.
        /// </summary>
        public Model Model { get; private set; }

        /// <summary>
        /// Gets a value indicates whether this is a clustered primary key.
        /// </summary>
        public bool IsClustered { get; private set; }

        void IIndexConstraint.AsNonClustered()
        {
            IsClustered = false;
        }
        
        private Func<IList<ColumnSort>> _getPrimaryKey;

        private ReadOnlyCollection<ColumnSort> _primaryKey;
        /// <summary>
        /// Gets the columns with sort direction of this primiary key.
        /// </summary>
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

        /// <inheritdoc />
        public override string SystemName
        {
            get { return string.IsNullOrEmpty(Name) ? "PrimaryKey" : Name; }
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
