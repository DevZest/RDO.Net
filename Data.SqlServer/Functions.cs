using DevZest.Data.Primitives;
using System;
using System.Data.SqlTypes;
using System.Collections.Generic;
using DevZest.Data.Utilities;
using System.Collections.Concurrent;
using System.Reflection;
using System.Linq.Expressions;

namespace DevZest.Data.SqlServer
{
    public static class Functions
    {
        private sealed class XmlValueFunction<T> : ScalarFunctionExpression<T>
        {
            public XmlValueFunction(Column<SqlXml> column, Column xPath, Column targetColumn)
                : base(column, xPath, targetColumn)
            {
            }

            protected override FunctionKey FunctionKey
            {
                get { return FunctionKeys.XmlValue; }
            }

            protected override IModelSet GetParentModelSet()
            {
                return Parameters[0].ParentModelSet;
            }

            protected sealed override IModelSet GetAggregateModelSet()
            {
                return Parameters[0].AggregateModelSet;
            }

            public override T Eval(DataRow dataRow)
            {
                throw new NotSupportedException();
            }
        }

        private sealed class ValueFunctionInvoker<T> : GenericInvoker<T, _SqlXml, Column>
        {
            public static readonly ValueFunctionInvoker<T> Singleton = new ValueFunctionInvoker<T>();

            private ValueFunctionInvoker()
                : base(typeof(Functions).GetStaticMethodInfo(nameof(_Value)), () => typeof(T).ResolveColumnDataType())
            {
            }
        }

        private static TColumn _Value<TColumn, TValue>(TColumn targetColumn, _SqlXml xmlColumn, Column xPath)
            where TColumn : Column<TValue>, new()
        {
            return new XmlValueFunction<TValue>(xmlColumn, xPath, targetColumn).MakeColumn<TColumn>();
        }

        public static T Value<T>(this _SqlXml xmlColumn, string xPath, T targetColumn)
            where T : Column, new()
        {
            return ValueFunctionInvoker<T>.Singleton.Invoke(targetColumn, xmlColumn, _String.Param(xPath, null));
        }

        private static ConcurrentDictionary<Type, Func<_SqlXml, string, Column, Column>> s_valueColumnInvokers =
            new ConcurrentDictionary<Type, Func<_SqlXml, string, Column, Column>>();

        internal static Column ValueColumn(this _SqlXml xmlColumn, string xPath, Column targetColumn)
        {
            var columnType = targetColumn.GetType();
            var invoker = s_valueColumnInvokers.GetOrAdd(columnType, BuildValueColumnInvoker(columnType));
            return invoker(xmlColumn, xPath, targetColumn);
        }

        private static T _ValueColumn<T>(this _SqlXml xmlColumn, string xPath, T targetColumn)
            where T : Column, new()
        {
            return ValueFunctionInvoker<T>.Singleton.Invoke(targetColumn, xmlColumn, _String.Const(xPath).AsVarCharMax());
        }

        private static Func<_SqlXml, string, Column, Column> BuildValueColumnInvoker(Type columnType)
        {
            var methodInfo = typeof(Functions).GetStaticMethodInfo(nameof(Functions._ValueColumn));
            methodInfo = methodInfo.MakeGenericMethod(columnType);
            var param0 = Expression.Parameter(typeof(_SqlXml), methodInfo.GetParameters()[0].Name);
            var param1 = Expression.Parameter(typeof(string), methodInfo.GetParameters()[1].Name);
            var param2 = Expression.Parameter(typeof(Column), methodInfo.GetParameters()[2].Name);
            var call = Expression.Call(methodInfo, param0, param1, Expression.Convert(param2, columnType));
            return Expression.Lambda<Func<_SqlXml, string, Column, Column>>(call, param0, param1, param2).Compile();
        }
    }
}
