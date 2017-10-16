using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;
using System.Reflection;

namespace DevZest.Data
{
    public static partial class Functions
    {
        #region Last

        private sealed class LastFunction<T> : AggregateFunctionExpression<T>
        {
            public LastFunction(Column<T> x)
                : base(x)
            {
                _column = x;
            }

            protected internal override Type[] ArgColumnTypes
            {
                get { return new Type[] { Owner.GetType() }; }
            }

            private Column<T> _column;

            protected override FunctionKey FunctionKey
            {
                get { return FunctionKeys.Last; }
            }

            T result;

            protected override void EvalInit()
            {
                result = default(T);
            }

            protected override void EvalAccumulate(DataRow dataRow)
            {
                result = _column[dataRow];
            }

            protected override T EvalReturn()
            {
                return result;
            }

            protected override void EvalTraverse(DataSetChain dataSetChain)
            {
                var rowCount = dataSetChain.RowCount;
                if (rowCount == 0)
                    return;
                var lastRow = dataSetChain[rowCount - 1];
                if (dataSetChain.HasNext)
                    EvalTraverse(dataSetChain.Next(lastRow));
                else
                    EvalAccumulate(lastRow);
            }
        }

        private sealed class LastFunctionInvoker<T> : GenericInvoker<T>
        {
            public static readonly LastFunctionInvoker<T> Singleton = new LastFunctionInvoker<T>();

            private LastFunctionInvoker()
                : base(typeof(Functions).GetStaticMethodInfo(nameof(_Last)), () => typeof(T).ResolveColumnDataType())
            {
            }
        }

        public static T Last<T>(this T x)
            where T : Column
        {
            return LastFunctionInvoker<T>.Singleton.Invoke(x);
        }

        private static T _Last<T, TValue>(this T x)
            where T : Column<TValue>, new()
        {
            return new LastFunction<TValue>(x).MakeColumn<T>();
        }

        #endregion
    }
}
