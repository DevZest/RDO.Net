using DevZest.Data.Primitives;
using System.Data.SqlTypes;
using System;
using System.IO;
using System.Xml;

namespace DevZest.Data.SqlServer
{
    /// <summary>
    /// Represents a <see cref="SqlXml"/> column.
    /// </summary>
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
                return value?.Value;
            }
        }

        /// <inheritdoc/>
        public override _String CastToString()
        {
            return new CastToStringExpression(this).MakeColumn<_String>();
        }

        /// <summary>Converts the supplied <see cref="_SqlXml" /> to <see cref="_String" />.</summary>
        /// <returns>A <see cref="_String" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_SqlXml" /> object. </param>
        public static explicit operator _String(_SqlXml x)
        {
            x.VerifyNotNull(nameof(x));
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
            x.VerifyNotNull(nameof(x));
            return new FromStringCast(x).MakeColumn<_SqlXml>();
        }

        /// <inheritdoc/>
        public override bool AreEqual(SqlXml x, SqlXml y)
        {
            return x == y;
        }

        /// <inheritdoc/>
        protected override Column<SqlXml> CreateConst(SqlXml value)
        {
            return Const(value);
        }

        /// <inheritdoc/>
        protected override Column<SqlXml> CreateParam(SqlXml value)
        {
            return Param(value, this);
        }

        /// <inheritdoc/>
        protected override bool IsNull(SqlXml value)
        {
            return value == null;
        }

        /// <inheritdoc/>
        protected override JsonValue SerializeValue(SqlXml value)
        {
            if (value == null || value.IsNull)
                return JsonValue.Null;

            return JsonValue.String(value.Value);
        }

        /// <summary>
        /// Create <see cref="SqlXml"/> object from string value.
        /// </summary>
        /// <param name="value">The string value.</param>
        /// <returns>The created result object.</returns>
        public static SqlXml CreateSqlXml(string value)
        {
            using (var xmlreader = XmlReader.Create(new StringReader(value)))
            {
                return new SqlXml(xmlreader);
            }
        }

        /// <inheritdoc/>
        protected override SqlXml DeserializeValue(JsonValue value)
        {
            return value.Type == JsonValueType.Null ? null : CreateSqlXml(value.Text);
        }

        /// <summary>Gets the value of this column from <see cref="SqlReader"/>'s current row.</summary>
        /// <param name="reader">The <see cref="SqlReader"/> object.</param>
        /// <returns>The value of this column from <see cref="SqlReader"/>'s current row.</returns>
        public SqlXml this[SqlReader reader]
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

        /// <summary>Creates a column of parameter expression.</summary>
        /// <param name="x">The value of the parameter expression.</param>
        /// <param name="sourceColumn">The value which will be passed to <see cref="DbParamExpression.SourceColumn"/>.</param>
        /// <returns>The column of parameter expression.</returns>
        public static _SqlXml Param(SqlXml x, _SqlXml sourceColumn = null)
        {
            return new ParamExpression<SqlXml>(x, sourceColumn).MakeColumn<_SqlXml>();
        }

        /// <summary>Creates a column of constant expression.</summary>
        /// <param name="x">The value of the constant expression.</param>
        /// <returns>The column of constant expression.</returns>
        public static _SqlXml Const(SqlXml x)
        {
            return new ConstantExpression<SqlXml>(x).MakeColumn<_SqlXml>();
        }

        void IColumn<SqlReader>.Read(SqlReader reader, DataRow dataRow)
        {
            this[dataRow] = GetValue(reader);
        }

        /// <summary>Converts the supplied <see cref="SqlXml"/> to <see cref="_SqlXml" /> expression.</summary>
        /// <returns>A new <see cref="_SqlXml" /> expression from the provided value.</returns>
        /// <param name="x">A <see cref="SqlXml"/> value.</param>
        public static implicit operator _SqlXml(SqlXml x)
        {
            return Param(x, null);
        }
    }
}
