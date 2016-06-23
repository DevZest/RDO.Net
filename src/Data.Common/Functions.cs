using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;

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

            protected internal override bool? this[DataRow dataRow]
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

        #region IsNotNull

        [ExpressionConverterNonGenerics(typeof(IsNotNullFunction.Converter), Id = "IsNotNull(Column)")]
        private sealed class IsNotNullFunction : ScalarFunctionExpression<bool?>
        {
            private sealed class Converter : ConverterBase<Column, IsNotNullFunction>
            {
                protected override IsNotNullFunction MakeExpression(Column param)
                {
                    return new IsNotNullFunction(param);
                }
            }

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

            protected internal override bool? this[DataRow dataRow]
            {
                get { return !_column.IsNull(dataRow); }
            }
        }

        public static _Boolean IsNotNull(this Column x)
        {
            Check.NotNull(x, nameof(x));
            return new IsNotNullFunction(x).MakeColumn<_Boolean>();
        }

        #endregion

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

            protected internal override T this[DataRow dataRow]
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

        #region GetDate

        [ExpressionConverterNonGenerics(typeof(GetDateFunction.Converter), Id = "GetDate()")]
        private sealed class GetDateFunction : ScalarFunctionExpression<DateTime?>
        {
            private sealed class Converter : ConverterBase<GetDateFunction>
            {
                protected override GetDateFunction MakeExpression()
                {
                    return new GetDateFunction();
                }
            }

            protected internal override DateTime? this[DataRow dataRow]
            {
                get { return DateTime.Now; }
            }

            protected override FunctionKey FunctionKey
            {
                get { return FunctionKeys.GetDate; }
            }
        }

        public static _DateTime GetDate()
        {
            return new GetDateFunction().MakeColumn<_DateTime>();
        }

        #endregion

        #region GetUtcDate

        [ExpressionConverterNonGenerics(typeof(GetUtcDateFunction.Converter), Id = "GetUtcDate()")]
        private class GetUtcDateFunction : ScalarFunctionExpression<DateTime?>
        {
            private sealed class Converter : ConverterBase<GetUtcDateFunction>
            {
                protected override GetUtcDateFunction MakeExpression()
                {
                    return new GetUtcDateFunction();
                }
            }

            protected internal override DateTime? this[DataRow dataRow]
            {
                get { return DateTime.UtcNow; }
            }

            protected override FunctionKey FunctionKey
            {
                get { return FunctionKeys.GetUtcDate; }
            }
        }

        public static _DateTime GetUtcDate()
        {
            return new GetUtcDateFunction().MakeColumn<_DateTime>();
        }

        #endregion

        #region NewGuid

        [ExpressionConverterNonGenerics(typeof(NewGuidFunction.Converter), Id = "NewGuid()")]
        private class NewGuidFunction : ScalarFunctionExpression<Guid?>
        {
            private sealed class Converter : ConverterBase<NewGuidFunction>
            {
                protected override NewGuidFunction MakeExpression()
                {
                    return new NewGuidFunction();
                }
            }

            protected internal override Guid? this[DataRow dataRow]
            {
                get { return Guid.NewGuid(); }
            }

            protected override FunctionKey FunctionKey
            {
                get { return FunctionKeys.NewGuid; }
            }
        }

        public static _Guid NewGuid()
        {
            return new NewGuidFunction().MakeColumn<_Guid>();
        }

        #endregion
    }
}
