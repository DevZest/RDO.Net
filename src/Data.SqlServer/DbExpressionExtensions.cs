using DevZest.Data.Primitives;
using System;

namespace DevZest.Data.SqlServer
{
    internal static class DbExpressionExtensions
    {
        public static void GenerateSql(this DbExpression expression, IndentedStringBuilder sqlBuilder, SqlVersion sqlVersion)
        {
            var generator = new ExpressionGenerator()
            {
                SqlBuilder = sqlBuilder,
                SqlVersion = sqlVersion
            };
            expression.Accept(generator);
        }
    }
}
