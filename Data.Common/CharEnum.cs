using DevZest.Data.Primitives;
using System;
using System.Diagnostics.CodeAnalysis;

namespace DevZest.Data
{
    /// <summary>Base class for enumerations column, which values are stored as nullable <see cref="Char"/> values.</summary>
    /// <typeparam name="T">The type of the enumerations value.</typeparam>
    public abstract class CharEnum<T> : Column<T>, IColumn<DbReader, T>
    {
        /// <inheritdoc/>
        protected internal sealed override JsonValue SerializeValue(T value)
        {
            var c = ConvertToChar(value);
            return c.HasValue ? JsonValue.Char(c.GetValueOrDefault()) : JsonValue.Null;
        }

        /// <inheritdoc/>
        protected internal override T DeserializeValue(JsonValue value)
        {
            return value.Type == JsonValueType.Null ? default(T) : ConvertToEnum(Convert.ToChar(value.Text));
        }

        /// <summary>Converts the enumeration value to nullable <see cref="Char"/> value.</summary>
        /// <param name="value">The enumeration value.</param>
        /// <returns>The nullable <see cref="Char"/> value.</returns>
        public abstract Char? ConvertToChar(T value);

        /// <summary>Converts the nullable <see cref="Char"/> value to the enumeration value.</summary>
        /// <param name="value">The nullable <see cref="Char"/> value.</param>
        /// <returns>The enumeration value.</returns>
        public abstract T ConvertToEnum(Char? value);

        /// <inheritdoc cref="P:DevZest.Data._Binary.Item(DevZest.Data.DbReader)"/>
        public T this[DbReader reader]
        {
            get
            {
                VerifyDbReader(reader);
                return GetValue(reader);
            }
        }

        private T GetValue(DbReader reader)
        {
            return ConvertToEnum(reader.GetChar(Ordinal));
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        void IColumn<DbReader>.Read(DbReader reader, DataRow dataRow)
        {
            this[dataRow] = GetValue(reader);
        }
    }
}
