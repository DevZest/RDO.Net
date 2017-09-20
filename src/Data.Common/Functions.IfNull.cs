using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;

namespace DevZest.Data
{
    public static partial class Functions
    {
        #region IfNull

        [ExpressionConverterGenerics(typeof(IfNullFunction<>.Converter), Id = "IfNull(Column, Column)")]
        private sealed class IfNullFunction<T> : ScalarFunctionExpression<T>
        {
            private sealed class Converter : ConverterBase<Column<T>, Column<T>, IfNullFunction<T>>
            {
                protected override IfNullFunction<T> MakeExpression(Column<T> param1, Column<T> param2)
                {
                    return new IfNullFunction<T>(param1, param2);
                }
            }

            protected internal override Type[] ArgColumnTypes
            {
                get { return new Type[] { Owner.GetType() }; }
            }

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

        public static T IfNull<T>(this T x, T replaceColumn)
            where T : Column, new()
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(replaceColumn, nameof(replaceColumn));

            return IfNullFunctionInvoker<T>.Singleton.Invoke(x, replaceColumn);
        }

        private sealed class IfNullFunctionInvoker<T> : GenericInvoker<T, T>
        {
            public static readonly IfNullFunctionInvoker<T> Singleton = new IfNullFunctionInvoker<T>();

            private IfNullFunctionInvoker()
                : base(typeof(Functions).GetStaticMethodInfo(nameof(_IfNull)), () => typeof(T).ResolveColumnDataType())
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
