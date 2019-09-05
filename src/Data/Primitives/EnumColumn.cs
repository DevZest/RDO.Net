using System;
using System.Collections.Generic;

namespace DevZest.Data.Primitives
{
    /// <summary>
    /// Base class for column which contains enum values.
    /// </summary>
    /// <typeparam name="T">The enum type.</typeparam>
    public abstract class EnumColumn<T> : Column<T?>
        where T : struct, IConvertible
    {
        /// <inheritdoc/>
        protected internal sealed override bool IsNull(T? value)
        {
            return !value.HasValue;
        }

        /// <summary>
        /// Gets or sets the delegate to provide description for enum values.
        /// </summary>
        public Func<T, string> DescriptionProvider { get; set; }
        
        /// <summary>
        /// Gets the enum items which can be used in combobox.
        /// </summary>
        public IEnumerable<EnumItem<T?>> EnumItems
        {
            get
            {
                if (IsNullable)
                    yield return new EnumItem<T?>();
                var values = Enum.GetValues(typeof(T));
                foreach (var obj in values)
                {
                    var value = (T)obj;
                    yield return new EnumItem<T?>(value, DescriptionProvider == null ? value.ToString() : DescriptionProvider(value));
                }
            }
        }

        private sealed class CastToStringExpression : CastExpression<T?, String>
        {
            public CastToStringExpression(Column<T?> x)
                : base(x)
            {
            }

            protected override String Cast(T? value)
            {
                return value.HasValue ? value.GetValueOrDefault().ToString() : null;
            }
        }

        /// <inheritdoc/>
        public sealed override _String CastToString()
        {
            return new CastToStringExpression(this).MakeColumn<_String>();
        }
    }

    /// <summary>
    /// Base class for column which contains enum values.
    /// </summary>
    /// <typeparam name="T">The enum type.</typeparam>
    /// <typeparam name="TDbValue">The data type which is used in database.</typeparam>
    public abstract class EnumColumn<T, TDbValue> : EnumColumn<T>, IColumn<DbReader, T?>
        where T : struct, IConvertible
    {
        T? IColumn<DbReader, T?>.this[DbReader reader]
        {
            get
            {
                VerifyDbReader(reader);
                return GetValue(reader);
            }
        }

        private T? GetValue(DbReader reader)
        {
            return ConvertToEnum(Read(reader));
        }

        /// <summary>
        /// Reads the value from database reader.
        /// </summary>
        /// <param name="reader">The database reader.</param>
        /// <returns>The result value.</returns>
        protected abstract TDbValue Read(DbReader reader);

        /// <summary>
        /// Converts from database value to enum value.
        /// </summary>
        /// <param name="dbValue">The database value.</param>
        /// <returns>The enum value.</returns>
        public abstract T? ConvertToEnum(TDbValue dbValue);

        /// <summary>
        /// Converts from enum value to database value.
        /// </summary>
        /// <param name="value">The enum value.</param>
        /// <returns>The database value.</returns>
        public abstract TDbValue ConvertToDbValue(T? value);

        void IColumn<DbReader>.Read(DbReader reader, DataRow dataRow)
        {
            this[dataRow] = GetValue(reader);
        }

        /// <inheritdoc/>
        protected internal override sealed JsonValue SerializeValue(T? value)
        {
            return SerializeDbValue(ConvertToDbValue(value));
        }

        /// <summary>
        /// Serializes the database value into JSON value.
        /// </summary>
        /// <param name="value">The database value.</param>
        /// <returns>The serialized JSON value.</returns>
        protected abstract JsonValue SerializeDbValue(TDbValue value);

        /// <inheritdoc/>
        protected internal override T? DeserializeValue(JsonValue value)
        {
            return value.Type == JsonValueType.Null ? new T?() : ConvertToEnum(DeserializeDbValue(value));
        }

        /// <summary>
        /// Deserializes JSON value into database value.
        /// </summary>
        /// <param name="value">The JSON value.</param>
        /// <returns>The deserialized database value.</returns>
        protected abstract TDbValue DeserializeDbValue(JsonValue value);
    }
}
