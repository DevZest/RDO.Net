using DevZest.Data.Primitives;
using System;
using System.Globalization;
using System.Text;

namespace DevZest.Data
{
    public struct JsonValue
    {
        public static readonly JsonValue True = new JsonValue("true", JsonValueType.True);
        public static readonly JsonValue False = new JsonValue("false", JsonValueType.False);
        public static readonly JsonValue Null = new JsonValue("null", JsonValueType.Null);

        public static JsonValue Char(char? x)
        {
            return x.HasValue ? String(new string(x.GetValueOrDefault(), 1)) : Null;
        }

        private const string IsoDateTimeFormat = "yyyy-MM-ddTHH:mm:ss.FFFFFFFK";

        public static JsonValue DateTime(DateTime? x)
        {
            return DateTime(x, IsoDateTimeFormat);
        }

        public static JsonValue DateTime(DateTime? x, string format)
        {
            if (!x.HasValue)
                return Null;

            var value = x.GetValueOrDefault();
            return String(value.ToString(format, CultureInfo.InvariantCulture));
        }

        public static JsonValue Guid(Guid? x)
        {
            return x.HasValue ? String(x.GetValueOrDefault().ToString()) : Null;
        }

        public static JsonValue Number(byte? x)
        {
            return x.HasValue ? Number(x.Value) : Null;
        }

        public static JsonValue Number(byte x)
        {
            return Number(x.ToString(NumberFormatInfo.InvariantInfo));
        }

        public static JsonValue Number(short? x)
        {
            return x.HasValue ? Number(x.Value) : Null;
        }

        public static JsonValue Number(short x)
        {
            return Number(x.ToString(NumberFormatInfo.InvariantInfo));
        }

        public static JsonValue Number(int? x)
        {
            return x.HasValue ? Number(x.Value) : Null;
        }

        public static JsonValue Number(int x)
        {
            return Number(x.ToString(NumberFormatInfo.InvariantInfo));
        }

        public static JsonValue Number(long? x)
        {
            return x.HasValue ? Number(x.Value) : Null;
        }

        public static JsonValue Number(long x)
        {
            return Number(x.ToString(NumberFormatInfo.InvariantInfo));
        }

        public static JsonValue Number(float? x)
        {
            return x.HasValue ? Number(x.Value) : Null;
        }

        public static JsonValue Number(float x)
        {
            return Number(x.ToString(NumberFormatInfo.InvariantInfo));
        }

        public static JsonValue Number(double? x)
        {
            return x.HasValue ? Number(x.Value) : Null;
        }

        public static JsonValue Number(double x)
        {
            return Number(x.ToString(NumberFormatInfo.InvariantInfo));
        }

        internal static JsonValue Number(decimal? x)
        {
            return x.HasValue ? Number(x.Value) : Null;
        }

        public static JsonValue Number(decimal x)
        {
            return Number(x.ToString(NumberFormatInfo.InvariantInfo));
        }

        public static JsonValue Number(string x)
        {
            return new JsonValue(x, JsonValueType.Number);
        }

        public static JsonValue String(string x)
        {
            return x == null ? Null : new JsonValue(x, JsonValueType.String);
        }

        internal JsonValue(string text, JsonValueType type)
        {
            text.VerifyNotNull(nameof(text));

            Text = text;
            Type = type;
        }

        public readonly string Text;

        public readonly JsonValueType Type;

        public bool IsDefault
        {
            get { return Type == 0 && Text == null; }
        }

        public override string ToString()
        {
            return Text;
        }
    }
}
