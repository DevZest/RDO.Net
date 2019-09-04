using DevZest.Data.Primitives;

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

        /// <summary>
        /// Determines whether specified column is not null value.
        /// </summary>
        /// <param name="x">The column.</param>
        /// <returns>The <see cref="_Boolean"/> column which contains the result.</returns>
        public static _Boolean IsNotNull(this Column x)
        {
            x.VerifyNotNull(nameof(x));
            return new IsNotNullFunction(x).MakeColumn<_Boolean>();
        }

        #endregion
    }
}
