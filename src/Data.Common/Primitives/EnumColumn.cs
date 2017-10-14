using System;
using System.Collections;
using System.Collections.Generic;

namespace DevZest.Data.Primitives
{
    public abstract class EnumColumn<T> : Column<T?>
        where T : struct, IConvertible
    {
        protected internal sealed override bool IsNull(T? value)
        {
            return !value.HasValue;
        }

        protected IEnumerable<EnumItem<T>> EnumItems
        {
            get
            {
                throw new NotImplementedException();
            }
        }

    }

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

        protected abstract TDbValue Read(DbReader reader);

        public abstract T? ConvertToEnum(TDbValue dbValue);

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

        protected abstract JsonValue SerializeDbValue(TDbValue value);

        /// <inheritdoc/>
        protected internal override T? DeserializeValue(JsonValue value)
        {
            return value.Type == JsonValueType.Null ? new T?() : ConvertToEnum(DeserializeDbValue(value));
        }

        protected abstract TDbValue DeserializeDbValue(JsonValue value);
    }
}
