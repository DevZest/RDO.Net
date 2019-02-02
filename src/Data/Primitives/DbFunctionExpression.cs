using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DevZest.Data.Primitives
{
    public sealed class DbFunctionExpression : DbExpression
    {
        private static readonly DbExpression[] s_emptyParamList = new DbExpression[0];

        public DbFunctionExpression(Type dataType, FunctionKey functionKey)
            : this(dataType, functionKey, s_emptyParamList)
        {
        }

        public DbFunctionExpression(Type dataType, FunctionKey functionKey, params DbExpression[] paramList)
            : this(dataType, functionKey, (IList<DbExpression>)paramList)
        {
        }

        public DbFunctionExpression(Type dataType, FunctionKey functionKey, IList<DbExpression> paramList)
            : base(dataType)
        {
            functionKey.VerifyNotNull(nameof(functionKey));
            paramList.VerifyNotNull(nameof(paramList));

            FunctionKey = functionKey;
            var readonlyCollection = paramList as ReadOnlyCollection<DbExpression>;
            ParamList = readonlyCollection != null ? readonlyCollection : new ReadOnlyCollection<DbExpression>(paramList);
        }

        public FunctionKey FunctionKey { get; private set; }

        public ReadOnlyCollection<DbExpression> ParamList { get; private set; }

        public override void Accept(DbExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override T Accept<T>(DbExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
