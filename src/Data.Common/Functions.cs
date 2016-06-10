using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;

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

            protected internal override bool? this[DataRow dataRow]
            {
                get { return _column.IsNull(dataRow); }
            }

            protected internal override bool? Eval()
            {
                return _column.IsEvalNull;
            }
        }

        public static _Boolean IsNull(this Column x)
        {
            Check.NotNull(x, nameof(x));
            return new IsNullFunction(x).MakeColumn<_Boolean>();
        }

        #endregion

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

            protected internal override bool? this[DataRow dataRow]
            {
                get { return !_column.IsNull(dataRow); }
            }

            protected internal override bool? Eval()
            {
                return !_column.IsEvalNull;
            }
        }

        public static _Boolean IsNotNull(this Column x)
        {
            Check.NotNull(x, nameof(x));
            return new IsNotNullFunction(x).MakeColumn<_Boolean>();
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

            protected internal override T this[DataRow dataRow]
            {
                get
                {
                    var result = _column[dataRow];
                    return _column.IsNull(dataRow) ? _replaceColumn[dataRow] : _column[dataRow];
                }
            }

            protected internal override T Eval()
            {
                var result = _column.Eval();
                return _column.IsEvalNull ? _replaceColumn.Eval() : _column.Eval();
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
            protected internal override DateTime? this[DataRow dataRow]
            {
                get { return DateTime.Now; }
            }

            protected internal override DateTime? Eval()
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
            protected internal override DateTime? this[DataRow dataRow]
            {
                get { return DateTime.UtcNow; }
            }

            protected internal override DateTime? Eval()
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
            protected internal override Guid? this[DataRow dataRow]
            {
                get { return Guid.NewGuid(); }
            }

            protected internal override Guid? Eval()
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
    }
}
