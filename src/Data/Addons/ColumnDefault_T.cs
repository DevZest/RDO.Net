using DevZest.Data.Primitives;
using System.Diagnostics;

namespace DevZest.Data.Addons
{
    /// <summary>
    /// Represents default constraint of <see cref="Column"/>.
    /// </summary>
    /// <typeparam name="T">The data type of the <see cref="Column"/>.</typeparam>
    public sealed class ColumnDefault<T> : ColumnDefault
    {
        internal ColumnDefault(Column<T> defaultValue, string name, string description)
            : base(name, description)
        {
            Debug.Assert(defaultValue != null && defaultValue.IsExpression);
            _defaultValue = defaultValue;
        }

        private Column<T> _defaultValue;
        /// <summary>
        /// Gets the default value.
        /// </summary>
        public T Value
        {
            get { return _defaultValue[null]; }
        }

        /// <inheritdoc />
        public override DbExpression DbExpression
        {
            get { return _defaultValue.DbExpression; }
        }
    }
}
