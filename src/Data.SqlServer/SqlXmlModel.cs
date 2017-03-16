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

        internal _SqlXml Xml { get; private set; }

        internal DbExpression SourceData { get; private set; }

        internal string XPath { get; private set; }

        [ExpressionConverterGenerics(typeof(XmlValueFunction<>.Converter), Id = "SqlXmlModel.Value(string, Column)")]
        private sealed class XmlValueFunction<T> : ScalarFunctionExpression<T>
        {
            private sealed class Converter : ConverterBase<Column<SqlXml>, Column, Column, XmlValueFunction<T>>
            {
                protected override XmlValueFunction<T> MakeExpression(Column<SqlXml> param1, Column param2, Column param3)
                {
                    return new XmlValueFunction<T>(param1, param2, param3);
                }
            }

            public XmlValueFunction(Column<SqlXml> column, Column xPath, Column targetColumn)
                : base(column, xPath, targetColumn)
            {
            }

            protected override Type[] ArgColumnTypes
            {
                get { return new Type[] { Owner.GetType() }; }
            }

            protected override FunctionKey FunctionKey
            {
                get { return FunctionKeys.XmlValue; }
            }

            protected override IModelSet GetScalarBaseModels()
            {
                return Parameters[0].ScalarBaseModels;
            }

            protected sealed override IModelSet GetAggregateBaseModels()
            {
                return Parameters[0].AggregateBaseModels;
            }

            protected override T this[DataRow dataRow]
            {
                get { throw new NotSupportedException(); }
            }
        }

        private sealed class ValueFunctionInvoker<T> : GenericInvoker<T, _SqlXml, Column>
        {
            public static readonly ValueFunctionInvoker<T> Singleton = new ValueFunctionInvoker<T>();

            private ValueFunctionInvoker()
                : base(typeof(SqlXmlModel).GetStaticMethodInfo(nameof(_Value)), () => typeof(T).ResolveColumnDataType())
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
            return ValueFunctionInvoker<T>.Singleton.Invoke(asColumn, xmlColumn, _String.Const(xPath).AsVarCharMax());
        }

        private static Func<_SqlXml, string, Column, Column> BuildValueInvoker(Type columnType)
        {
            var methodInfo = typeof(SqlXmlModel).GetStaticMethodInfo(nameof(_InvokeValue));
            methodInfo = methodInfo.MakeGenericMethod(columnType);
            var param0 = Expression.Parameter(typeof(_SqlXml), methodInfo.GetParameters()[0].Name);
            var param1 = Expression.Parameter(typeof(string), methodInfo.GetParameters()[1].Name);
            var param2 = Expression.Parameter(typeof(Column), methodInfo.GetParameters()[2].Name);
            var call = Expression.Call(methodInfo, param0, param1, Expression.Convert(param2, columnType));
            return Expression.Lambda<Func<_SqlXml, string, Column, Column>>(call, param0, param1, param2).Compile();
        }
    }
}
