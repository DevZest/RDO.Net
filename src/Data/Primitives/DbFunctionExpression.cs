using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DevZest.Data.Primitives
{
    /// <summary>
    /// Represents function call in database expression.
    /// </summary>
    public sealed class DbFunctionExpression : DbExpression
    {
        private static readonly DbExpression[] s_emptyParamList = new DbExpression[0];

        /// <summary>
        /// Initializes a new instance of <see cref="DbFunctionExpression"/> class.
        /// </summary>
        /// <param name="dataType">The data type.</param>
        /// <param name="functionKey">The function key.</param>
        public DbFunctionExpression(Type dataType, FunctionKey functionKey)
            : this(dataType, functionKey, s_emptyParamList)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="DbFunctionExpression"/> class.
        /// </summary>
        /// <param name="dataType">The data type.</param>
        /// <param name="functionKey">The function key.</param>
        /// <param name="paramList">The parameter list.</param>
        public DbFunctionExpression(Type dataType, FunctionKey functionKey, params DbExpression[] paramList)
            : this(dataType, functionKey, (IList<DbExpression>)paramList)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="DbFunctionExpression"/> class.
        /// </summary>
        /// <param name="dataType">The data type.</param>
        /// <param name="functionKey">The function key.</param>
        /// <param name="paramList">The parameter list.</param>
        public DbFunctionExpression(Type dataType, FunctionKey functionKey, IList<DbExpression> paramList)
            : base(dataType)
        {
            functionKey.VerifyNotNull(nameof(functionKey));
            paramList.VerifyNotNull(nameof(paramList));

            FunctionKey = functionKey;
            var readonlyCollection = paramList as ReadOnlyCollection<DbExpression>;
            ParamList = readonlyCollection != null ? readonlyCollection : new ReadOnlyCollection<DbExpression>(paramList);
        }

        /// <summary>
        /// Gets the function key.
        /// </summary>
        public FunctionKey FunctionKey { get; private set; }

        /// <summary>
        /// Gets the parameter list.
        /// </summary>
        public ReadOnlyCollection<DbExpression> ParamList { get; private set; }

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
