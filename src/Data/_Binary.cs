using DevZest.Data.Primitives;
using System;

namespace DevZest.Data
{
    /// <summary>
    /// Represents column of binary data.
    /// </summary>
    public sealed class _Binary : Column<Binary>, IColumn<DbReader, Binary>
    {
        private sealed class CastToStringExpression : CastExpression<Binary, String>
        {
            public CastToStringExpression(Column<Binary> x)
                : base(x)
            {
            }

            protected override string Cast(Binary value)
            {
                return value == null ? null : Convert.ToBase64String(value.ToArray());
            }
        }

        /// <inheritdoc />
        public override _String CastToString()
        {
            return new CastToStringExpression(this).MakeColumn<_String>();
        }

        /// <summary>Converts the supplied <see cref="_Binary" /> to <see cref="_String" />.</summary>
        /// <returns>A <see cref="_String" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Binary" /> object. </param>
        public static explicit operator _String(_Binary x)
        {
            x.VerifyNotNull(nameof(x));
            return x.CastToString();
        }

        private sealed class FromStringExpression : CastExpression<String, Binary>
        {
            public FromStringExpression(Column<String> x)
                : base(x)
            {
            }

            protected override Binary Cast(String value)
            {
                if (value == null)
                    return null;
                return new Binary(Convert.FromBase64String(value));
            }
        }

        /// <summary>Converts the supplied <see cref="_String" /> to <see cref="_Binary" />.</summary>
        /// <returns>A <see cref="_Binary" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_String" /> object. </param>
        public static explicit operator _Binary(_String x)
        {
            x.VerifyNotNull(nameof(x));
            return new FromStringExpression(x).MakeColumn<_Binary>();
        }

        /// <inheritdoc />
        public override bool AreEqual(Binary x, Binary y)
        {
            if (x == null)
                return y == null;
            return x.Equals(y);
        }

        /// <inheritdoc/>
        protected sealed override Column<Binary> CreateParam(Binary value)
        {
            return Param(value, this);
        }

        /// <inheritdoc/>
        protected internal sealed override Column<Binary> CreateConst(Binary value)
        {
            return Const(value);
        }

        /// <inheritdoc/>
        protected internal override JsonValue SerializeValue(Binary value)
        {
            if (value == null)
                return JsonValue.Null;
            var bytes = value.ToArray();
            return JsonValue.String(Convert.ToBase64String(bytes));
        }

        /// <inheritdoc/>
        protected internal override Binary DeserializeValue(JsonValue value)
        {
            return value.Type == JsonValueType.Null ? null : new Binary(Convert.FromBase64String(value.Text));
        }

        /// <summary>Gets the value of this column from <see cref="DbReader"/>'s current row.</summary>
        /// <param name="reader">The <see cref="DbReader"/> object.</param>
        /// <returns>The value of this column from <see cref="DbReader"/>'s current row.</returns>
        public Binary this[DbReader reader]
        {
            get
            {
                VerifyDbReader(reader);
                return GetValue(reader);
            }
        }

        private Binary GetValue(DbReader reader)
        {
            var result = reader[Ordinal];
            if (result == null)
                return null;
            else
                return (byte[])result;
        }

        void IColumn<DbReader>.Read(DbReader dbReader, DataRow dataRow)
        {
            this[dataRow] = GetValue(dbReader);
        }

        /// <inheritdoc/>
        protected internal override bool IsNull(Binary value)
        {
            return value == null;
        }

        /// <summary>Creates a column of parameter expression.</summary>
        /// <param name="x">The value of the parameter expression.</param>
        /// <param name="sourceColumn">The value which will be passed to <see cref="DbParamExpression.SourceColumn"/>.</param>
        /// <returns>The column of parameter expression.</returns>
        public static _Binary Param(Binary x, _Binary sourceColumn = null)
        {
            return new ParamExpression<Binary>(x, sourceColumn).MakeColumn<_Binary>();
        }

        /// <summary>Creates a column of constant expression.</summary>
        /// <param name="x">The value of the constant expression.</param>
        /// <returns>The column of constant expression.</returns>
        public static _Binary Const(Binary x)
        {
            return new ConstantExpression<Binary>(x).MakeColumn<_Binary>();
        }

        /// <summary>Implicitly converts the supplied value to a column of parameter expression.</summary>
        /// <param name="x">The value of the parameter expression.</param>
        /// <returns>The column of parameter expression.</returns>
        public static implicit operator _Binary(Binary x)
        {
            return Param(x);
        }
    }
}
