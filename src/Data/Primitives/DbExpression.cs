using System;
using System.Diagnostics;

namespace DevZest.Data.Primitives
{
    public abstract class DbExpression
    {
        internal DbExpression(Type dataType)
        {
            dataType.VerifyNotNull(nameof(dataType));
            DataType = dataType;
        }

        public abstract void Accept(DbExpressionVisitor visitor);

        public abstract T Accept<T>(DbExpressionVisitor<T> visitor);

        public Type DataType { get; }
    }
}
