using System;
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

        public Func<T, string> DescriptionProvider { get; set; }
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

        public sealed override _String CastToString()
        {
            return new CastToStringExpression(this).MakeColumn<_String>();
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
