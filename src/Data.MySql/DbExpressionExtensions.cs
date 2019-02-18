using DevZest.Data.Primitives;

namespace DevZest.Data.MySql
{
    internal static class DbExpressionExtensions
    {
        public static void GenerateSql(this DbExpression expression, IndentedStringBuilder sqlBuilder, MySqlVersion mySqlVersion)
        {
            var generator = new ExpressionGenerator()
            {
                SqlBuilder = sqlBuilder,
                MySqlVersion = mySqlVersion
            };
            expression.Accept(generator);
        }
    }
}
