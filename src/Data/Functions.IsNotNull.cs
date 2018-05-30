using DevZest.Data.Primitives;
using DevZest.Data.Utilities;

namespace DevZest.Data
{
    public static partial class Functions
    {
        #region IsNotNull

        private sealed class IsNotNullFunction : ScalarFunctionExpression<bool?>
        {
            public IsNotNullFunction(Column column)
                : base(column)
            {
                _column = column;
            }

            private Column _column;

            protected override FunctionKey FunctionKey
            {
                get { return FunctionKeys.IsNotNull; }
            }

            public override bool? this[DataRow dataRow]
            {
                get { return !_column.IsNull(dataRow); }
            }
        }

        public static _Boolean IsNotNull(this Column x)
        {
            x.VerifyNotNull(nameof(x));
            return new IsNotNullFunction(x).MakeColumn<_Boolean>();
        }

        #endregion
    }
}
