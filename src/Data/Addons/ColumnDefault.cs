using DevZest.Data.Primitives;

namespace DevZest.Data.Addons
{
    /// <summary>
    /// Base class to represent default constraint of <see cref="Column"/>.
    /// </summary>
    /// <remarks><see cref="ColumnDefault"/> implements <see cref="IAddon"/> with key of <see cref="ColumnDefault"/> type.
    /// Retrieve <see cref="ColumnDefault"/> by calling <see cref="Column.GetDefault"/> method.</remarks>
    public abstract class ColumnDefault : IAddon
    {
        internal ColumnDefault(string name, string description)
        {
            Name = name;
            Description = description;
        }

        /// <summary>
        /// Gets the name of the default constraint.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the description of the default constraint.
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Gets the <see cref="DbExpression"/> of the default constraint.
        /// </summary>
        public abstract DbExpression DbExpression { get; }

        object IAddon.Key
        {
            get { return typeof(ColumnDefault); }
        }
    }
}
