using System;
using System.Diagnostics;

namespace DevZest.Data.Primitives
{
    public sealed class DbParamExpression : DbExpression
    {
        internal DbParamExpression(Column column, Column sourceColumn, object value)
            : base(column.DataType)
        {
            Debug.Assert(column != null);
            Debug.Assert(sourceColumn == null || sourceColumn.GetType() == column.GetType());
            Column = column;
            SourceColumn = sourceColumn;
            Value = value;
        }

        public Column Column { get; private set; }

        public Column SourceColumn { get; private set; }

        public object Value { get; private set; }

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
