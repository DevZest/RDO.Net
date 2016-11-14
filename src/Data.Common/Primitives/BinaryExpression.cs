using System;
using System.Text;
using DevZest.Data.Utilities;

namespace DevZest.Data.Primitives
{
    /// <summary>Represents column expression which contains two column operands.</summary>
    /// <typeparam name="T">The data type of the column operands and column expression.</typeparam>
    public abstract class BinaryExpression<T> : BinaryExpression<T, T>
    {
        /// <summary>Initializes a new instance of <see cref="BinaryExpression{T}"/> class.</summary>
        /// <param name="left">The left column operand.</param>
        /// <param name="right">The right column operand.</param>
        protected BinaryExpression(Column<T> left, Column<T> right)
            : base(left, right)
        {
        }
    }

    /// <summary>Represents column expression which contains two column operands.</summary>
    /// <typeparam name="T">The data type of column operands.</typeparam>
    /// <typeparam name="TResult">The data type of the column expression.</typeparam>
    public abstract class BinaryExpression<T, TResult> : ColumnExpression<TResult>
    {
        private const string LEFT = nameof(Left);
        private const string RIGHT = nameof(Right);

        protected abstract class ConverterBase : ExpressionConverter
        {
            internal sealed override void WriteJson(JsonWriter jsonWriter, ColumnExpression expression)
            {
                var binaryExpression = (BinaryExpression<T, TResult>)expression;
                jsonWriter.WriteNameColumnPair(LEFT, binaryExpression.Left).WriteComma().WriteNameColumnPair(RIGHT, binaryExpression.Right);
            }

            internal sealed override ColumnExpression ParseJson(Model model, ColumnJsonParser parser)
            {
                var left = parser.ParseNameColumnPair<Column<T>>(LEFT, model);
                parser.ExpectComma();
                var right = parser.ParseNameColumnPair<Column<T>>(RIGHT, model);
                return MakeExpression(left, right);
            }

            protected abstract BinaryExpression<T, TResult> MakeExpression(Column<T> left, Column<T> right);
        }

        /// <summary>Initializes a new instance of <see cref="BinaryExpression{T, TResult}"/> class.</summary>
        /// <param name="left">The left column operand.</param>
        /// <param name="right">The right column operand.</param>
        protected BinaryExpression(Column<T> left, Column<T> right)
        {
            Check.NotNull(left, nameof(left));
            Check.NotNull(right, nameof(right));

            Left = left;
            Right = right;
        }

        /// <summary>Gets the left column operand.</summary>
        public Column<T> Left { get; private set; }

        /// <summary>Gets the right column operand.</summary>
        public Column<T> Right { get; private set; }

        /// <summary>Gets the kind of this binary expression.</summary>
        protected abstract BinaryExpressionKind Kind { get; }

        /// <inheritdoc/>
        protected sealed override IModelSet GetParentModelSet()
        {
            return Left.ParentModelSet.Union(Right.ParentModelSet);
        }

        /// <inheritdoc/>
        protected sealed override IModelSet GetAggregateModelSet()
        {
            return Left.AggregateModelSet.Union(Right.AggregateModelSet);
        }

        /// <inheritdoc/>
        public sealed override DbExpression GetDbExpression()
        {
            return new DbBinaryExpression(Kind, Left.DbExpression, Right.DbExpression);
        }

        /// <inheritdoc/>
        protected internal sealed override TResult this[DataRow dataRow]
        {
            get
            {
                var x = Left[dataRow];
                var y = Right[dataRow];
                return EvalCore(x, y);
            }
        }

        /// <summary>Evaluates the expression against two operand values.</summary>
        /// <param name="x">The left operand value.</param>
        /// <param name="y">The right operand value.</param>
        /// <returns>The result.</returns>
        protected abstract TResult EvalCore(T x, T y);
    }
}
