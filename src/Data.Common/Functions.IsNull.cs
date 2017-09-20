using DevZest.Data.Primitives;
using DevZest.Data.Utilities;

namespace DevZest.Data
{
    public static partial class Functions
    {
        #region IsNull

        [ExpressionConverterNonGenerics(typeof(IsNullFunction.Converter), Id = "IsNull(Column)")]
        private sealed class IsNullFunction : ScalarFunctionExpression<bool?>
        {
            private sealed class Converter : ConverterBase<Column, IsNullFunction>
            {
                protected override IsNullFunction MakeExpression(Column param)
                {
                    return new IsNullFunction(param);
                }
            }

            public IsNullFunction(Column column)
                : base(column)
            {
                _column = column;
            }

            private Column _column;

            protected override FunctionKey FunctionKey
            {
                get { return FunctionKeys.IsNull; }
            }

            public override bool? this[DataRow dataRow]
            {
                get { return _column.IsNull(dataRow); }
            }
        }

        public static _Boolean IsNull(this Column x)
        {
            Check.NotNull(x, nameof(x));
            return new IsNullFunction(x).MakeColumn<_Boolean>();
        }

        #endregion
    }
}
