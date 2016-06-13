using System;

namespace DevZest.Data.Primitives
{
    [ExpressionConverterGenerics(typeof(ParamExpression<>.Converter<>))]
    public sealed class ParamExpression<T> : ValueExpression<T>
    {
        private sealed class Converter<TColumn> : ConverterBase<TColumn>
            where TColumn : Column<T>, new()
        {
            protected override ValueExpression<T> MakeExpression(T value)
            {
                return new ParamExpression<T>(value, null);
            }
        }

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

        internal sealed override Column<T> GetCounterpart(Model model)
        {
            return Owner;
        }
    }
}
