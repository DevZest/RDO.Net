using DevZest.Data.Primitives;
using System;

namespace DevZest.Data
{
    public sealed class _CharEnum<T> : EnumColumn<T, char?>
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

        public static _CharEnum<T> Param(T? x, _CharEnum<T> sourceColumn = null)
        {
            return new ParamExpression<T?>(x, sourceColumn).MakeColumn<_CharEnum<T>>();
        }

        public static _ByteEnum<T> Const(T? x)
        {
            return new ConstantExpression<T?>(x).MakeColumn<_ByteEnum<T>>();
        }

        public static implicit operator _CharEnum<T>(T? x)
        {
            return Param(x);
        }

        public override char? ConvertToDbValue(T? value)
        {
            if (!value.HasValue)
                return null;
            return value.Value.ToChar(null);
        }

        public override T? ConvertToEnum(char? dbValue)
        {
            if (!dbValue.HasValue)
                return null;
            else
                return (T)Enum.ToObject(typeof(T), dbValue.GetValueOrDefault());
        }

        protected override JsonValue SerializeDbValue(char? value)
        {
            return JsonValue.Char(value);
        }

        protected override char? DeserializeDbValue(JsonValue value)
        {
            return value.Type == JsonValueType.Null ? null : new Char?(Convert.ToChar(value.Text));
        }

        protected override char? Read(DbReader reader)
        {
            return reader.GetChar(Ordinal);
        }
    }
}
