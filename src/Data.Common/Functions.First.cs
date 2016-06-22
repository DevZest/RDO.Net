﻿using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;
using System.Reflection;

namespace DevZest.Data
{
    public static partial class Functions
    {
        #region First

        [ExpressionConverterGenerics(typeof(FirstFunction<>.Converter), Id = "First(Column)")]
        private sealed class FirstFunction<T> : AggregateFunctionExpression<T>
        {
            private sealed class Converter : ConverterBase<Column<T>, FirstFunction<T>>
            {
                protected override FirstFunction<T> MakeExpression(Column<T> param)
                {
                    return new FirstFunction<T>(param);
                }
            }

            public FirstFunction(Column<T> x)
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
                get { return FunctionKeys.First; }
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
                if (dataSetChain.RowCount == 0)
                    return;
                var firstRow = dataSetChain[0];
                if (dataSetChain.HasNext)
                    EvalTraverse(dataSetChain.Next(firstRow));
                else
                    EvalAccumulate(firstRow);
            }
        }

        private sealed class FirstFunctionInvoker<T> : GenericInvoker<T>
        {
            public static readonly FirstFunctionInvoker<T> Singleton = new FirstFunctionInvoker<T>();

            private FirstFunctionInvoker()
                : base(typeof(Functions).GetStaticMethodInfo(nameof(_First)), () => typeof(T).ResolveColumnDataType())
            {
            }
        }

        public static T First<T>(this T x)
            where T : Column
        {
            return FirstFunctionInvoker<T>.Singleton.Invoke(x);
        }

        private static T _First<T, TValue>(this T x)
            where T : Column<TValue>, new()
        {
            return new FirstFunction<TValue>(x).MakeColumn<T>();
        }

        #endregion
    }
}
