using DevZest.Data.Primitives;
using System;

namespace DevZest.Data
{
    /// <summary>
    /// Represents column of variable-length stream of binary data.
    /// </summary>
    public sealed class _Binary : Column<Binary>, IColumn<DbReader, Binary>
    {
        protected override bool AreEqual(Binary x, Binary y)
        {
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
            return new JsonValue(Convert.ToBase64String(bytes), true, JsonValueType.String);
        }

        /// <inheritdoc/>
        protected internal override Binary DeserializeValue(JsonValue value)
        {
            return value.Type == JsonValueType.Null ? null : new Binary(Convert.FromBase64String(value.Text));
        }

        /// <summary>Gets the value of this column from <see cref="DbReader"/>'s current row.</summary>
        /// <param name="reader">The <see cref="DbReader"/> object.</param>
        /// <returns>The value of this column from <see cref="DbReader"/>'s current row.</returns>
        /// <inheritdoc cref="Column.VerifyDbReader(DbReader)" select="exception"/>
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
