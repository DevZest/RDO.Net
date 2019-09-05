using DevZest.Data.Annotations;
using DevZest.Data.Primitives;
using System;
using System.Collections.Concurrent;
using System.Data.SqlTypes;
using System.Linq.Expressions;
using System.Reflection;

namespace DevZest.Data.SqlServer
{
    [InvisibleToDbDesigner]
    internal sealed class SqlXmlNode : Model
    {
        static SqlXmlNode()
        {
            RegisterColumn((SqlXmlNode x) => x.Xml);
        }

        public SqlXmlNode()
        {
        }

        internal void Initialize(string dbSetName, SqlXml sourceData, string xPath)
        {
            this.dbSetName = dbSetName;
            SourceData = _SqlXml.Param(sourceData).DbExpression;
            XPath = xPath;
        }

        internal _SqlXml Xml { get; private set; }

        internal DbExpression SourceData { get; private set; }

        internal string XPath { get; private set; }

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

            protected override IModels GetScalarSourceModels()
            {
                return Parameters[0].ScalarSourceModels;
            }

            protected sealed override IModels GetAggregateBaseModels()
            {
                return Parameters[0].AggregateSourceModels;
            }

            public sealed override T this[DataRow dataRow]
            {
                get { throw new NotSupportedException(); }
            }
        }

        private abstract class ColumnInvoker<T, T1, T2>
            where T : Column
        {
            private Func<T, T1, T2, T> _func;

            protected ColumnInvoker(MethodInfo methodInfo, bool bypassNullable = false)
            {
                methodInfo.VerifyNotNull(nameof(methodInfo));
                BuildFunc(methodInfo, typeof(T).ResolveColumnDataType(bypassNullable));
            }

            private void BuildFunc(MethodInfo methodInfo, Type resolvedType)
            {
                methodInfo = methodInfo.MakeGenericMethod(typeof(T), resolvedType);
                var param0 = Expression.Parameter(typeof(T), methodInfo.GetParameters()[0].Name);
                var param1 = Expression.Parameter(typeof(T1), methodInfo.GetParameters()[1].Name);
                var param2 = Expression.Parameter(typeof(T2), methodInfo.GetParameters()[2].Name);
                var call = Expression.Call(methodInfo, param0, param1, param2);
                _func = Expression.Lambda<Func<T, T1, T2, T>>(call, param0, param1, param2).Compile();
            }

            public T Invoke(T arg, T1 param1, T2 param2)
            {
                return _func(arg, param1, param2);
            }
        }

        private sealed class ValueFunctionInvoker<T> : ColumnInvoker<T, _SqlXml, Column>
            where T : Column
        {
            public static readonly ValueFunctionInvoker<T> Singleton = new ValueFunctionInvoker<T>();

            private ValueFunctionInvoker()
                : base(typeof(SqlXmlNode).GetStaticMethodInfo(nameof(_Value)))
            {
            }
        }

        private static TColumn _Value<TColumn, TValue>(TColumn targetColumn, _SqlXml xmlColumn, Column xPath)
            where TColumn : Column<TValue>, new()
        {
            return new XmlValueFunction<TValue>(xmlColumn, xPath, targetColumn).MakeColumn<TColumn>();
        }

        public T Value<T>(string xPath, T asColumn)
            where T : Column, new()
        {
            return ValueFunctionInvoker<T>.Singleton.Invoke(asColumn, Xml, _String.Param(xPath, null));
        }

        private static ConcurrentDictionary<Type, Func<_SqlXml, string, Column, Column>> s_valueInvokers =
            new ConcurrentDictionary<Type, Func<_SqlXml, string, Column, Column>>();

        public Column this[string xPath, Column asColumn]
        {
            get
            {
                var columnType = asColumn.GetType();
                var invoker = s_valueInvokers.GetOrAdd(columnType, BuildValueInvoker(columnType));
                return invoker(Xml, xPath, asColumn);
            }
        }

        private static T _InvokeValue<T>(_SqlXml xmlColumn, string xPath, T asColumn)
            where T : Column, new()
        {
            return ValueFunctionInvoker<T>.Singleton.Invoke(asColumn, xmlColumn, _String.Const(xPath).AsSqlVarCharMax());
        }

        private static Func<_SqlXml, string, Column, Column> BuildValueInvoker(Type columnType)
        {
            var methodInfo = typeof(SqlXmlNode).GetStaticMethodInfo(nameof(_InvokeValue));
            methodInfo = methodInfo.MakeGenericMethod(columnType);
            var param0 = Expression.Parameter(typeof(_SqlXml), methodInfo.GetParameters()[0].Name);
            var param1 = Expression.Parameter(typeof(string), methodInfo.GetParameters()[1].Name);
            var param2 = Expression.Parameter(typeof(Column), methodInfo.GetParameters()[2].Name);
            var call = Expression.Call(methodInfo, param0, param1, Expression.Convert(param2, columnType));
            return Expression.Lambda<Func<_SqlXml, string, Column, Column>>(call, param0, param1, param2).Compile();
        }

        private string dbSetName;
        protected override string DbAlias
        {
            get { return dbSetName; }
        }
    }
}
