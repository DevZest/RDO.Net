using DevZest.Data.Primitives;
using System.Data.SqlTypes;
using System;
using System.IO;
using System.Xml;
using DevZest.Data.Utilities;

namespace DevZest.Data.SqlServer
{
    public sealed class _SqlXml : Column<SqlXml>, IColumn<SqlReader>
    {
        protected override Column<SqlXml> CreateConst(SqlXml value)
        {
            return Const(value);
        }

        protected override Column<SqlXml> CreateParam(SqlXml value)
        {
            return Param(value, this);
        }

        protected override bool IsNull(SqlXml value)
        {
            return value == null;
        }

        protected override JsonValue SerializeValue(SqlXml value)
        {
            if (value == null || value.IsNull)
                return JsonValue.Null;

            return new JsonValue(value.Value, false, JsonValueType.String);
        }

        public static SqlXml CreateSqlXml(string s)
        {
            using (var reader = new StringReader(s))
            {
                using (var xmlreader = XmlReader.Create(reader))
                {
                    return new SqlXml(xmlreader);
                }
            }
        }

        protected override SqlXml DeserializeValue(JsonValue value)
        {
            return value.Type == JsonValueType.Null ? null : CreateSqlXml(value.Text);
        }

        public _SqlXml this[SqlReader reader]
        {
            get
            {
                VerifyDbReader(reader);
                return GetValue(reader);
            }
        }

        private SqlXml GetValue(SqlReader reader)
        {
            return reader.GetSqlXml(Ordinal);
        }

        public static _SqlXml Param(SqlXml x, _SqlXml sourceColumn = null)
        {
            return new ParamExpression<SqlXml>(x, sourceColumn).MakeColumn<_SqlXml>();
        }

        public static _SqlXml Const(SqlXml x)
        {
            return new ConstantExpression<SqlXml>(x).MakeColumn<_SqlXml>();
        }

        void IColumn<SqlReader>.Read(SqlReader reader, DataRow dataRow)
        {
            this[dataRow] = GetValue(reader);
        }

        public static implicit operator _SqlXml(SqlXml x)
        {
            return Param(x, null);
        }

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

        internal sealed class ValueFunctionInvoker<T> : GenericInvoker<T, _SqlXml, Column>
        {
            public static readonly ValueFunctionInvoker<T> Singleton = new ValueFunctionInvoker<T>();

            private ValueFunctionInvoker()
                : base(typeof(_SqlXml).GetStaticMethodInfo(nameof(_Value)), () => typeof(T).ResolveColumnDataType())
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
            return ValueFunctionInvoker<T>.Singleton.Invoke(asColumn, this, _String.Param(xPath, null));
        }
    }
}
