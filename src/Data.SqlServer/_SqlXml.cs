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
        private sealed class CastToStringExpression : CastExpression<SqlXml, string>
        {
            public CastToStringExpression(Column<SqlXml> x)
                : base(x)
            {
            }

            protected override string Cast(SqlXml value)
            {
                return value == null ? null : value.Value;
            }
        }

        public override _String CastToString()
        {
            return new CastToStringExpression(this).MakeColumn<_String>();
        }

        /// <summary>Converts the supplied <see cref="_SqlXml" /> to <see cref="_String" />.</summary>
        /// <returns>A <see cref="_String" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_SqlXml" /> object. </param>
        public static explicit operator _String(_SqlXml x)
        {
            Check.NotNull(x, nameof(x));
            return x.CastToString();
        }

        private sealed class FromStringCast : CastExpression<String, SqlXml>
        {
            public FromStringCast(Column<String> x)
                : base(x)
            {
            }

            protected override SqlXml Cast(String value)
            {
                return value == null ? null : CreateSqlXml(value);
            }
        }

        /// <summary>Converts the supplied <see cref="_String" /> to <see cref="_DateTimeOffset" />.</summary>
        /// <returns>A <see cref="_DateTimeOffset" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_String" /> object. </param>
        public static explicit operator _SqlXml(_String x)
        {
            Check.NotNull(x, nameof(x));
            return new FromStringCast(x).MakeColumn<_SqlXml>();
        }

        public override bool AreEqual(SqlXml x, SqlXml y)
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
