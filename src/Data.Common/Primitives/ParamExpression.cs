using System;

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
            return new DbParamExpression(Owner, SourceColumn, Value);
        }

        internal sealed override Column GetParallelColumn(Model model)
        {
            return Owner;
        }
    }
}
