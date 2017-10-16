using System;
using System.Text;

namespace DevZest.Data.Primitives
{
    public sealed class ParamExpression<T> : ValueExpression<T>
    {
        public ParamExpression(T value, Column<T> sourceColumn)
            : base(value)
        {
            SourceColumn = sourceColumn;
        }

        public Column<T> SourceColumn { get; private set; }

        public override DbExpression GetDbExpression()
        {
            object exprValue;
            if (Owner.IsNull(Value))
                exprValue = null;
            else
                exprValue = Value;
            return new DbParamExpression(Owner, SourceColumn, exprValue);
        }
    }
}
