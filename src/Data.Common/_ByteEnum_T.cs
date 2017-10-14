using DevZest.Data.Primitives;
using System;

namespace DevZest.Data
{
    public sealed class _ByteEnum<T> : EnumColumn<T, byte?>
        where T : struct, IConvertible
    {
        private sealed class Converter : ConverterBase<_ByteEnum<T>>
        {
        }

        public override _String CastToString()
        {
            throw new NotImplementedException();
        }

        protected override Column<T?> CreateParam(T? value)
        {
            return Param(value, this);
        }

        protected internal override Column<T?> CreateConst(T? value)
        {
            return Const(value);
        }

        public static _ByteEnum<T> Param(T? x, _ByteEnum<T> sourceColumn = null)
        {
            return new ParamExpression<T?>(x, sourceColumn).MakeColumn<_ByteEnum<T>>();
        }

        public static _ByteEnum<T> Const(T? x)
        {
            return new ConstantExpression<T?>(x).MakeColumn<_ByteEnum<T>>();
        }

        public static implicit operator _ByteEnum<T>(T? x)
        {
            return Param(x);
        }

        public override byte? ConvertToDbValue(T? value)
        {
            if (!value.HasValue)
                return null;
            return value.Value.ToByte(null);
        }

        public override T? ConvertToEnum(byte? dbValue)
        {
            if (!dbValue.HasValue)
                return null;
            else
                return (T)Enum.ToObject(typeof(T), dbValue.GetValueOrDefault());
        }

        protected override JsonValue SerializeDbValue(byte? value)
        {
            return JsonValue.Number(value);
        }

        protected override byte? DeserializeDbValue(JsonValue value)
        {
            return value.Type == JsonValueType.Null ? null : new Byte?(Convert.ToByte(value.Text));
        }

        protected override byte? Read(DbReader reader)
        {
            return reader.GetByte(Ordinal);
        }
    }
}
