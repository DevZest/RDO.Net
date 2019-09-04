using System;

namespace DevZest.Data.Primitives
{
    /// <summary>
    /// Represents database expression which has two expression operands.
    /// </summary>
    public sealed class DbBinaryExpression : DbExpression
    {
        /// <summary>
        /// Initializes a new instance <see cref="DbBinaryExpression"/> class.
        /// </summary>
        /// <param name="dataType">The data type.</param>
        /// <param name="kind">Specifies the operation of the expression.</param>
        /// <param name="left">The left expression operand.</param>
        /// <param name="right">The right expression operand.</param>
        public DbBinaryExpression(Type dataType, BinaryExpressionKind kind, DbExpression left, DbExpression right)
            : base(dataType)
        {
            left.VerifyNotNull(nameof(left));
            right.VerifyNotNull(nameof(right));

            Kind = kind;
            Left = left;
            Right = right;
        }

        /// <summary>
        /// Gets the operation of the expression.
        /// </summary>
        public BinaryExpressionKind Kind { get; private set; }

        /// <summary>
        /// Gets the left expression operand.
        /// </summary>
        public DbExpression Left { get; private set; }

        /// <summary>
        /// Gets the right expression operand.
        /// </summary>
        public DbExpression Right { get; private set; }

        /// <inheritdoc />
        public override void Accept(DbExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }

        /// <inheritdoc />
        public override T Accept<T>(DbExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
