using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System.Diagnostics;
using System.Globalization;

namespace DevZest.Data
{
    /// <summary>
    /// Defines the mapping between source column or expression and target column.
    /// </summary>
    public struct ColumnMapping
    {
        internal ColumnMapping(Column source, Column target)
            : this(source == null ? DbConstantExpression.Null : source.DbExpression, target)
        {
        }

        internal ColumnMapping(DbExpression source, Column target)
        {
            Debug.Assert(source != null);
            Debug.Assert(target != null);
            Source = source;
            Target = (DbColumnExpression)target.DbExpression;
        }

        /// <summary>Gets the source column of this mapping.</summary>
        /// <value>Returns <see langword="null"/> if source is expression.</value>
        public Column SourceColumn
        {
            get
            {
                var columnExpression = Source as DbColumnExpression;
                return columnExpression == null ? null : columnExpression.Column;
            }
        }

        /// <summary>Gets the source of this mapping.</summary>
        public readonly DbExpression Source;

        /// <summary>Gets the target column of this mapping.</summary>
        public Column TargetColumn
        {
            get { return Target == null ? null : Target.Column; }
        }

        /// <summary>Gets the target of this mapping.</summary>
        public readonly DbColumnExpression Target;

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "({0}, {1})", Source, TargetColumn);
        }
    }
}
