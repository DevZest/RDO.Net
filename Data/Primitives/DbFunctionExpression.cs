using DevZest.Data.Utilities;
using System.Collections.ObjectModel;

namespace DevZest.Data.Primitives
{
    public sealed class DbFunctionExpression : DbExpression
    {
        private static readonly DbExpression[] s_emptyParamList = new DbExpression[0];

        public DbFunctionExpression(FunctionKey functionKey)
            : this(functionKey, s_emptyParamList)
        {
        }

        public DbFunctionExpression(FunctionKey functionKey, params DbExpression[] paramList)
        {
            Check.NotNull(functionKey, nameof(functionKey));
            Check.NotNull(paramList, nameof(paramList));

            FunctionKey = functionKey;
            ParamList = new ReadOnlyCollection<DbExpression>(paramList);
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
