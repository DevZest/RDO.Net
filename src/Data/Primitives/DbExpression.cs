using System;

namespace DevZest.Data.Primitives
{
    /// <summary>
    /// Represents a database expression.
    /// </summary>
    public abstract class DbExpression
    {
        internal DbExpression(Type dataType)
        {
            dataType.VerifyNotNull(nameof(dataType));
            DataType = dataType;
        }

        /// <summary>
        /// Dispatches to the specific visit method for this node type.
        /// </summary>
        /// <param name="visitor">The vistor.</param>
        public abstract void Accept(DbExpressionVisitor visitor);

        /// <summary>
        /// Dispatches to the specific visit method for this node type.
        /// </summary>
        /// <param name="visitor">The vistor.</param>
        /// <returns>The visitor result.</returns>
        public abstract T Accept<T>(DbExpressionVisitor<T> visitor);

        /// <summary>
        /// Gets the data type of this expression.
        /// </summary>
        public Type DataType { get; }
    }
}
