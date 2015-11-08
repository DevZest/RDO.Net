
using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;
using System.Collections.Concurrent;
using System.Data.SqlTypes;
using System.Linq.Expressions;

namespace DevZest.Data.SqlServer
{
    public sealed class SqlXmlModel : Model
    {
        static SqlXmlModel()
        {
            RegisterColumn((SqlXmlModel x) => x.Xml);
        }

        public SqlXmlModel()
        {
        }

        internal void Initialize(SqlXml sourceData, string xPath)
        {
            SourceData = _SqlXml.Param(sourceData).DbExpression;
            XPath = xPath;
        }

        public _SqlXml Xml { get; private set; }

        internal DbExpression SourceData { get; private set; }

        internal string XPath { get; private set; }

        private static ConcurrentDictionary<Type, Func<_SqlXml, string, Column, Column>> s_valueColumnInvokers =
            new ConcurrentDictionary<Type, Func<_SqlXml, string, Column, Column>>();

        public Column this[string xPath, Column asColumn]
        {
            get
            {
                var columnType = asColumn.GetType();
                var invoker = s_valueColumnInvokers.GetOrAdd(columnType, BuildValueColumnInvoker(columnType));
                return invoker(Xml, xPath, asColumn);
            }
        }

        private static T _Value<T>(_SqlXml xmlColumn, string xPath, T asColumn)
            where T : Column, new()
        {
            return _SqlXml.ValueFunctionInvoker<T>.Singleton.Invoke(asColumn, xmlColumn, _String.Const(xPath).AsVarCharMax());
        }

        private static Func<_SqlXml, string, Column, Column> BuildValueColumnInvoker(Type columnType)
        {
            var methodInfo = typeof(SqlXmlModel).GetStaticMethodInfo(nameof(_Value));
            methodInfo = methodInfo.MakeGenericMethod(columnType);
            var param0 = Expression.Parameter(typeof(_SqlXml), methodInfo.GetParameters()[0].Name);
            var param1 = Expression.Parameter(typeof(string), methodInfo.GetParameters()[1].Name);
            var param2 = Expression.Parameter(typeof(Column), methodInfo.GetParameters()[2].Name);
            var call = Expression.Call(methodInfo, param0, param1, Expression.Convert(param2, columnType));
            return Expression.Lambda<Func<_SqlXml, string, Column, Column>>(call, param0, param1, param2).Compile();
        }
    }
}
