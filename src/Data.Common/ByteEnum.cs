using DevZest.Data.Primitives;
using System;
using System.Diagnostics.CodeAnalysis;

namespace DevZest.Data
{
    /// <summary>Base class for enumerations column, which values are stored as nullable <see cref="Byte"/> values.</summary>
    /// <typeparam name="T">The type of the enumerations value.</typeparam>
    public abstract class ByteEnum<T> : Column<T>, IColumn<DbReader, T>
    {
        public sealed override bool AreEqual(T x, T y)
        {
            return ConvertToByte(x) == ConvertToByte(y);
        }

        /// <inheritdoc/>
        protected internal override sealed JsonValue SerializeValue(T value)
        {
            var b = ConvertToByte(value);
            return b.HasValue ? JsonValue.Number(b.GetValueOrDefault()) : JsonValue.Null;
        }

        /// <inheritdoc/>
        protected internal override T DeserializeValue(JsonValue value)
        {
            return value.Type == JsonValueType.Null ? default(T) : ConvertToEnum(Convert.ToByte(value.Text));
        }

        /// <summary>Converts the enumeration value to nullable <see cref="Byte"/> value.</summary>
        /// <param name="value">The enumeration value.</param>
        /// <returns>The nullable <see cref="Byte"/> value.</returns>
        public abstract Byte? ConvertToByte(T value);

        /// <summary>Converts the nullable <see cref="Byte"/> value to the enumeration value.</summary>
        /// <param name="value">The nullable <see cref="Byte"/> value.</param>
        /// <returns>The enumeration value.</returns>
        public abstract T ConvertToEnum(Byte? value);

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
            return ConvertToEnum(reader.GetByte(Ordinal));
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        void IColumn<DbReader>.Read(DbReader reader, DataRow dataRow)
        {
            this[dataRow] = GetValue(reader);
        }
    }
}
