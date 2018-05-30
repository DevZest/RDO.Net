using DevZest.Data.Primitives;

namespace DevZest.Data
{
    public static partial class Functions
    {
        #region IsNull

        private sealed class IsNullFunction : ScalarFunctionExpression<bool?>
        {
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
            x.VerifyNotNull(nameof(x));
            return new IsNullFunction(x).MakeColumn<_Boolean>();
        }

        #endregion
    }
}
