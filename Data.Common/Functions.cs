using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;

namespace DevZest.Data
{
    public static partial class Functions
    {
        #region IsNull

        private sealed class IsNullFunction<T> : ScalarFunctionExpression<bool?>
        {
            public IsNullFunction(Column<T> column)
                : base(column)
            {
                _column = column;
            }

            private Column<T> _column;

            protected override FunctionKey FunctionKey
            {
                get { return FunctionKeys.IsNull; }
            }

            public override bool? Eval(DataRow dataRow)
            {
                return _column.IsNull(dataRow);
            }
        }

        public static _Boolean IsNull<T>(this Column<T> x)
        {
            Check.NotNull(x, nameof(x));
            return new IsNullFunction<T>(x).MakeColumn<_Boolean>();
        }

        #endregion

        #region IsNotNull

        private sealed class IsNotNullFunction<T> : ScalarFunctionExpression<bool?>
        {
            public IsNotNullFunction(Column<T> column)
                : base(column)
            {
                _column = column;
            }

            private Column<T> _column;

            protected override FunctionKey FunctionKey
            {
                get { return FunctionKeys.IsNotNull; }
            }

            public override bool? Eval(DataRow dataRow)
            {
                return !_column.IsNull(dataRow);
            }
        }

        public static _Boolean IsNotNull<T>(this Column<T> x)
        {
            Check.NotNull(x, nameof(x));
            return new IsNotNullFunction<T>(x).MakeColumn<_Boolean>();
        }

        #endregion

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

            public override T Eval(DataRow dataRow)
            {
                var result = _column[dataRow];
                return _column.IsNull(dataRow) ? _replaceColumn[dataRow] : _column[dataRow];
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

        private class GetDateFunction : ScalarFunctionExpression<DateTime?>
        {
            public override DateTime? Eval(DataRow dataRow)
            {
                return DateTime.Now;
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

        private class GetUtcDateFunction : ScalarFunctionExpression<DateTime?>
        {
            public override DateTime? Eval(DataRow dataRow)
            {
                return DateTime.UtcNow;
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

        private class NewGuidFunction : ScalarFunctionExpression<Guid?>
        {
            public override Guid? Eval(DataRow dataRow)
            {
                return Guid.NewGuid();
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

        #region When

        public static CaseOnExpression<T, TResult> When<T, TResult>(this Column<T> on, Column<T> condition, Column<TResult> value)
        {
            Check.NotNull(on, nameof(on));
            Check.NotNull(condition, nameof(condition));
            Check.NotNull(value, nameof(value));
            var result = new CaseOnExpression<T, TResult>(on);
            return result.When(condition, value);
        }

        #endregion

        #region Then
        public static CaseExpression<T> Then<T>(this _Boolean condition, Column<T> value)
        {
            Check.NotNull(condition, nameof(condition));
            Check.NotNull(value, nameof(value));

            var result = new CaseExpression<T>();
            result.Then(condition, value);
            return result;
        }

        #endregion
    }
}
