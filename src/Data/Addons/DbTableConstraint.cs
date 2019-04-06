using System;
using System.Threading;

namespace DevZest.Data.Addons
{
    /// <summary>
    /// Base class to represent database table constraint.
    /// </summary>
    public abstract class DbTableConstraint : IAddon
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DbTableConstraint"/> class.
        /// </summary>
        /// <param name="name">The explicit name of this constraint, can be null or empty.</param>
        /// <param name="description">The description of this constraint.</param>
        protected DbTableConstraint(string name, string description)
        {
            Name = name;
            Description = description;
        }

        /// <summary>
        /// Gets the explicit name of this constraint.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the description of this constraint.
        /// </summary>
        public string Description { get; private set; }

        object IAddon.Key
        {
            get { return typeof(DbTableConstraint).FullName + "." + SystemName; }
        }

        private string _systemName;
        /// <summary>
        /// Gets the system name of this constraint.
        /// </summary>
        /// <remarks>If <see cref="Name"/> is <see langword="null"/> or empty, an automatically generated name will be returned,
        /// otherwise the <see cref="Name"/> will be returned.</remarks>
        public virtual string SystemName
        {
            get { return LazyInitializer.EnsureInitialized(ref _systemName, () => string.IsNullOrEmpty(Name) ? Guid.NewGuid().ToString() : Name); }
        }

        /// <summary>
        /// Gets a value indicates whether this constraint will be applied for database table.
        /// </summary>
        public abstract bool IsValidOnTable { get; }

        /// <summary>
        /// Gets a value indicates whether this constraint will be applied for database temporary table.
        /// </summary>
        public abstract bool IsValidOnTempTable { get; }
    }
}
