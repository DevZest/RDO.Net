using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System.Diagnostics;
using System.Globalization;

namespace DevZest.Data
{
    /// <summary>
    /// Defines the mapping between two columns.
    /// </summary>
    public struct ColumnMapping
    {
        internal ColumnMapping(Column source, Column target)
        {
            Debug.Assert(target != null);
            Debug.Assert(source == null || source.DataType == target.DataType);
            Source = source;
            Target = target;
        }

        /// <summary>Gets the source column of this mapping.</summary>
        public readonly Column Source;

        /// <summary>Gets the source column expression of this mapping.</summary>
        public DbExpression SourceExpression
        {
            get { return Source == null ? DbConstantExpression.Null : Source.DbExpression; }
        }

        /// <summary>Gets the target column of this mapping.</summary>
        public readonly Column Target;

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "({0}, {1})", Source, Target);
        }
    }
}
