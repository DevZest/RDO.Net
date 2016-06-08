using DevZest.Data.Primitives;
using System.Data.SqlTypes;
using System;
using System.IO;
using System.Xml;
using DevZest.Data.Utilities;

namespace DevZest.Data.SqlServer
{
    [ColumnConverter(typeof(Converter))]
    public sealed class _SqlXml : Column<SqlXml>, IColumn<SqlReader>
    {
        private sealed class Converter : ConverterBase<_SqlXml>
        {
        }

        protected override bool AreEqual(SqlXml x, SqlXml y)
        {
            return x == y;
        }

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
            using (var xmlreader = XmlReader.Create(new StringReader(s)))
            {
                return new SqlXml(xmlreader);
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
    }
}
