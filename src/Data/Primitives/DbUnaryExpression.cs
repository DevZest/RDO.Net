using System;

namespace DevZest.Data.Primitives
{
    /// <summary>
    /// Represents database expression which has single operand.
    /// </summary>
    public sealed class DbUnaryExpression : DbExpression
    {
        /// <summary>
        /// Initializes a new instance of <see cref="DbUnaryExpression"/> class.
        /// </summary>
        /// <param name="dataType">The data type.</param>
        /// <param name="kind">Specifies the operation of the expression.</param>
        /// <param name="operand">The operand of the expression.</param>
        public DbUnaryExpression(Type dataType, DbUnaryExpressionKind kind, DbExpression operand)
            : base(dataType)
        {
            Kind = kind;
            Operand = operand;
        }

        /// <summary>
        /// Gets the operation of the expression.
        /// </summary>
        public DbUnaryExpressionKind Kind { get; private set; }

        /// <summary>
        /// Gets the operand of the expression.
        /// </summary>
        public DbExpression Operand { get; private set; }

        /// <inheritdoc/>
        public override void Accept(DbExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }

        /// <inheritdoc/>
        public override T Accept<T>(DbExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
