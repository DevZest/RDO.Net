using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace DevZest.Data.Primitives
{
    internal sealed class SubQueryEliminator : DbExpressionVisitor<DbExpression>
    {
        public SubQueryEliminator(DbSelectStatement selectStatement)
        {
            Debug.Assert(selectStatement != null && selectStatement.Model.DataSource.Kind == DataSourceKind.DbQuery);
            _selectStatement = selectStatement;
        }

        private readonly DbSelectStatement _selectStatement;

        public DbExpression GetExpressioin(DbExpression expression)
        {
            return expression.Accept(this);
        }

        public DbFromClause FromClause
        {
            get { return _selectStatement.From; }
        }

        public DbExpression WhereExpression
        {
            get { return _selectStatement.Where; }
        }

        public override DbExpression Visit(DbBinaryExpression expression)
        {
            var left = expression.Left.Accept(this);
            var right = expression.Right.Accept(this);
            if (expression.Left != left || expression.Right != right)
                return new DbBinaryExpression(expression.Kind, left, right);
            return expression;
        }

        IList<DbExpression> Replace(IList<DbExpression> expressions)
        {
            if (expressions == null)
                return null;

            DbExpression[] result = null;
            for (int i = 0; i < expressions.Count; i++)
            {
                var expression = expressions[i];
                var replacedExpression = expression.Accept(this);
                if (expression != replacedExpression && result == null)
                {
                    result = new DbExpression[expressions.Count];
                    for (int j = 0; j < i; j++)
                        result[j] = expressions[j];
                }
                if (result != null)
                    result[i] = replacedExpression;
            }
            return result ?? expressions;
        }

        public override DbExpression Visit(DbCaseExpression expression)
        {
            var on = expression.On;
            var replacedOn = on == null ? null : on.Accept(this);
            var when = expression.When;
            var replacedWhen = Replace(when);
            var then = expression.Then;
            var replacedThen = Replace(then);
            var @else = expression.Else;
            var replacedElse = @else.Accept(this);

            if (on != replacedOn || when != replacedWhen || then != replacedThen || @else != replacedElse)
                return new DbCaseExpression(expression.On.Accept(this), Replace(expression.When), Replace(expression.Then), expression.Else.Accept(this));
            else
                return expression;
        }

        public override DbExpression Visit(DbCastExpression expression)
        {
            var operand = expression.Operand;
            var replacedOperand = operand.Accept(this);
            if (operand != replacedOperand)
                return new DbCastExpression(expression.Operand.Accept(this), expression.SourceDataType, expression.TargetColumn);
            else
                return expression;
        }

        public override DbExpression Visit(DbColumnExpression expression)
        {
            var column = expression.Column;
            if (column.ParentModel == _selectStatement.Model)
                return _selectStatement.Select[column.Ordinal].SourceExpression;
            else
                return expression;
        }

        public override DbExpression Visit(DbConstantExpression expression)
        {
            return expression;
        }

        public override DbExpression Visit(DbFunctionExpression expression)
        {
            var paramList = expression.ParamList;
            var replacedParamList = Replace(paramList);
            if (paramList != replacedParamList)
                return new DbFunctionExpression(expression.FunctionKey, Replace(expression.ParamList));
            else
                return expression;
        }

        public override DbExpression Visit(DbParamExpression expression)
        {
            return expression;
        }

        public override DbExpression Visit(DbUnaryExpression expression)
        {
            var operand = expression.Operand;
            var replacedOperand = operand.Accept(this);
            if (operand != replacedOperand)
                return new DbUnaryExpression(expression.Kind, expression.Operand.Accept(this));
            else
                return expression;
        }
    }
}
