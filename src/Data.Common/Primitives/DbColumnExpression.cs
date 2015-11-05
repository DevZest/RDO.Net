using System;
using System.Diagnostics;

namespace DevZest.Data.Primitives
{
    public sealed class DbColumnExpression : DbExpression
    {
        internal DbColumnExpression(Column column)
        {
            Debug.Assert(column != null);
            Column = column;
        }

        public Column Column { get; private set; }

        public string DbColumnName
        {
            get { return Column.DbColumnName; }
        }

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
