using DevZest.Data.Primitives;

namespace DevZest.Data
{
    public static partial class Functions
    {
        #region IfNull

        private sealed class IfNullFunction<T> : ScalarFunctionExpression<T>
        {
            public IfNullFunction(Column<T> column, Column<T> replaceColumn)
                : base(column, replaceColumn)
            {
                _column = column;
                _replaceColumn = replaceColumn;
            }

            protected override FunctionKey FunctionKey
            {
                get { return FunctionKeys.IfNull; }
            }

            Column<T> _column;
            Column<T> _replaceColumn;

            public override T this[DataRow dataRow]
            {
                get
                {
                    var result = _column[dataRow];
                    return _column.IsNull(dataRow) ? _replaceColumn[dataRow] : _column[dataRow];
                }
            }
        }

        /// <summary>
        /// Returns a specified value if the specified column value is null.
        /// </summary>
        /// <typeparam name="T">Type of the column.</typeparam>
        /// <param name="x">The column.</param>
        /// <param name="valueForNull">The specified value.</param>
        /// <returns>The column contains the result value.</returns>
        public static T IfNull<T>(this T x, T valueForNull)
            where T : Column, new()
        {
            x.VerifyNotNull(nameof(x));
            valueForNull.VerifyNotNull(nameof(valueForNull));

            return IfNullFunctionInvoker<T>.Singleton.Invoke(x, valueForNull);
        }

        private sealed class IfNullFunctionInvoker<T> : ColumnInvoker<T, T>
            where T : Column
        {
            public static readonly IfNullFunctionInvoker<T> Singleton = new IfNullFunctionInvoker<T>();

            private IfNullFunctionInvoker()
                : base(typeof(Functions).GetStaticMethodInfo(nameof(_IfNull)))
            {
            }
        }

        private static T _IfNull<T, TValue>(T x, T replaceColumn)
            where T : Column<TValue>, new()
        {
            return new IfNullFunction<TValue>(x, replaceColumn).MakeColumn<T>();
        }


        #endregion
    }
}
