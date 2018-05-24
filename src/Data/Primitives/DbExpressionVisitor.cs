using System;

namespace DevZest.Data.Primitives
{
    public abstract class DbExpressionVisitor
    {
        public abstract void Visit(DbConstantExpression expression);

        public abstract void Visit(DbParamExpression expression);

        public abstract void Visit(DbColumnExpression expression);

        public abstract void Visit(DbUnaryExpression expression);

        public abstract void Visit(DbBinaryExpression expression);

        public abstract void Visit(DbFunctionExpression expression);

        public abstract void Visit(DbCastExpression expression);

        public abstract void Visit(DbCaseExpression expression);
    }
}
