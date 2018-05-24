using System;

namespace DevZest.Data.Primitives
{
    public abstract class DbExpressionVisitor<T>
    {
        public abstract T Visit(DbConstantExpression expression);

        public abstract T Visit(DbParamExpression expression);

        public abstract T Visit(DbColumnExpression expression);

        public abstract T Visit(DbUnaryExpression expression);

        public abstract T Visit(DbBinaryExpression expression);

        public abstract T Visit(DbFunctionExpression expression);

        public abstract T Visit(DbCastExpression expression);

        public abstract T Visit(DbCaseExpression expression);
    }
}
