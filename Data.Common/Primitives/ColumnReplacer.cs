using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data.Primitives
{
    internal class ColumnReplacer : DbExpressionVisitor<DbExpression>
    {
        public ColumnReplacer(DbSelectStatement selectStatement)
        {
            Debug.Assert(selectStatement != null);
            _selectStatement = selectStatement;
        }

        private readonly DbSelectStatement _selectStatement;

        public DbExpression Replace(DbExpression expression)
        {
            return expression.Accept(this);
        }

        public override DbExpression Visit(DbBinaryExpression expression)
        {
            return new DbBinaryExpression(expression.Kind, expression.Left.Accept(this), expression.Right.Accept(this));
        }

        DbExpression[] Replace(IList<DbExpression> expressions)
        {
            var result = new DbExpression[expressions.Count];
            for (int i = 0; i < expressions.Count; i++)
                result[i] = expressions[i].Accept(this);
            return result;
        }

        public override DbExpression Visit(DbCaseExpression expression)
        {
            return new DbCaseExpression(expression.On.Accept(this), Replace(expression.When), Replace(expression.Then), expression.Else.Accept(this));
        }

        public override DbExpression Visit(DbCastExpression expression)
        {
            return new DbCastExpression(expression.Operand.Accept(this), expression.SourceDataType, expression.TargetColumn);
        }

        private Column Replace(Column column)
        {
            Debug.Assert(column.ParentModel == _selectStatement.Model);

            return _selectStatement.Select[column.Ordinal].Source;
        }

        public override DbExpression Visit(DbColumnExpression expression)
        {
            return new DbColumnExpression(Replace(expression.Column));
        }

        public override DbExpression Visit(DbConstantExpression expression)
        {
            return expression;
        }

        public override DbExpression Visit(DbFunctionExpression expression)
        {
            return expression.ParamList.Count == 0 ? expression : new DbFunctionExpression(expression.FunctionKey, Replace(expression.ParamList));
        }

        public override DbExpression Visit(DbParamExpression expression)
        {
            return expression;
        }

        public override DbExpression Visit(DbUnaryExpression expression)
        {
            return new DbUnaryExpression(expression.Kind, expression.Operand.Accept(this));
        }
    }
}
