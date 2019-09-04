using System;
using System.Globalization;

namespace DevZest.Data
{
    /// <summary>
    /// Represents a primitive JSON value, which can be a JSON number, a string, or one of the literals true, false, and null.
    /// </summary>
    public struct JsonValue
    {
        /// <summary>
        /// Gets the JSON literal true.
        /// </summary>
        public static readonly JsonValue True = new JsonValue("true", JsonValueType.True);

        /// <summary>
        /// Gets the JSON literal false.
        /// </summary>
        public static readonly JsonValue False = new JsonValue("false", JsonValueType.False);

        /// <summary>
        /// Gets the JSON literal null.
        /// </summary>
        public static readonly JsonValue Null = new JsonValue("null", JsonValueType.Null);

        /// <summary>
        /// Converts a char value into JSON value.
        /// </summary>
        /// <param name="x">The value.</param>
        /// <returns>The result JSON value.</returns>
        public static JsonValue Char(char? x)
        {
            return x.HasValue ? String(new string(x.GetValueOrDefault(), 1)) : Null;
        }

        private const string IsoDateTimeFormat = "yyyy-MM-ddTHH:mm:ss.FFFFFFFK";

        /// <summary>
        /// Converts a DateTime value into JSON value.
        /// </summary>
        /// <param name="x">The value.</param>
        /// <returns>The result JSON value.</returns>
        public static JsonValue DateTime(DateTime? x)
        {
            return DateTime(x, IsoDateTimeFormat);
        }

        /// <summary>
        /// Converts a char value into JSON value, with specified format.
        /// </summary>
        /// <param name="x">The value.</param>
        /// <param name="format">The format.</param>
        /// <returns>The result JSON value.</returns>
        public static JsonValue DateTime(DateTime? x, string format)
        {
            if (!x.HasValue)
                return Null;

            var value = x.GetValueOrDefault();
            return String(value.ToString(format, CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Converts a Guid value into JSON value.
        /// </summary>
        /// <param name="x">The value.</param>
        /// <returns>The result JSON value.</returns>
        public static JsonValue Guid(Guid? x)
        {
            return x.HasValue ? String(x.GetValueOrDefault().ToString()) : Null;
        }

        /// <summary>
        /// Converts a byte value into number JSON value.
        /// </summary>
        /// <param name="x">The value.</param>
        /// <returns>The result JSON value.</returns>
        public static JsonValue Number(byte? x)
        {
            return x.HasValue ? Number(x.Value) : Null;
        }

        /// <summary>
        /// Converts a byte value into number JSON value.
        /// </summary>
        /// <param name="x">The value.</param>
        /// <returns>The result JSON value.</returns>
        public static JsonValue Number(byte x)
        {
            return Number(x.ToString(NumberFormatInfo.InvariantInfo));
        }

        /// <summary>
        /// Converts a Int16 value into number JSON value.
        /// </summary>
        /// <param name="x">The value.</param>
        /// <returns>The result JSON value.</returns>
        public static JsonValue Number(short? x)
        {
            return x.HasValue ? Number(x.Value) : Null;
        }

        /// <summary>
        /// Converts a Int16 value into number JSON value.
        /// </summary>
        /// <param name="x">The value.</param>
        /// <returns>The result JSON value.</returns>
        public static JsonValue Number(short x)
        {
            return Number(x.ToString(NumberFormatInfo.InvariantInfo));
        }

        /// <summary>
        /// Converts a Int32 value into number JSON value.
        /// </summary>
        /// <param name="x">The value.</param>
        /// <returns>The result JSON value.</returns>
        public static JsonValue Number(int? x)
        {
            return x.HasValue ? Number(x.Value) : Null;
        }

        /// <summary>
        /// Converts a Int32 value into number JSON value.
        /// </summary>
        /// <param name="x">The value.</param>
        /// <returns>The result JSON value.</returns>
        public static JsonValue Number(int x)
        {
            return Number(x.ToString(NumberFormatInfo.InvariantInfo));
        }

        /// <summary>
        /// Converts a Int64 value into number JSON value.
        /// </summary>
        /// <param name="x">The value.</param>
        /// <returns>The result JSON value.</returns>
        public static JsonValue Number(long? x)
        {
            return x.HasValue ? Number(x.Value) : Null;
        }

        /// <summary>
        /// Converts a Int64 value into number JSON value.
        /// </summary>
        /// <param name="x">The value.</param>
        /// <returns>The result JSON value.</returns>
        public static JsonValue Number(long x)
        {
            return Number(x.ToString(NumberFormatInfo.InvariantInfo));
        }

        /// <summary>
        /// Converts a Single value into number JSON value.
        /// </summary>
        /// <param name="x">The value.</param>
        /// <returns>The result JSON value.</returns>
        public static JsonValue Number(float? x)
        {
            return x.HasValue ? Number(x.Value) : Null;
        }

        /// <summary>
        /// Converts a Single value into number JSON value.
        /// </summary>
        /// <param name="x">The value.</param>
        /// <returns>The result JSON value.</returns>
        public static JsonValue Number(float x)
        {
            return Number(x.ToString(NumberFormatInfo.InvariantInfo));
        }

        /// <summary>
        /// Converts a Double value into number JSON value.
        /// </summary>
        /// <param name="x">The value.</param>
        /// <returns>The result JSON value.</returns>
        public static JsonValue Number(double? x)
        {
            return x.HasValue ? Number(x.Value) : Null;
        }

        /// <summary>
        /// Converts a Double value into number JSON value.
        /// </summary>
        /// <param name="x">The value.</param>
        /// <returns>The result JSON value.</returns>
        public static JsonValue Number(double x)
        {
            return Number(x.ToString(NumberFormatInfo.InvariantInfo));
        }

        /// <summary>
        /// Converts a Decimal value into number JSON value.
        /// </summary>
        /// <param name="x">The value.</param>
        /// <returns>The result JSON value.</returns>
        public static JsonValue Number(decimal? x)
        {
            return x.HasValue ? Number(x.Value) : Null;
        }

        /// <summary>
        /// Converts a Decimal value into number JSON value.
        /// </summary>
        /// <param name="x">The value.</param>
        /// <returns>The result JSON value.</returns>
        public static JsonValue Number(decimal x)
        {
            return Number(x.ToString(NumberFormatInfo.InvariantInfo));
        }

        /// <summary>
        /// Converts a String value into number JSON value.
        /// </summary>
        /// <param name="x">The value.</param>
        /// <returns>The result JSON value.</returns>
        public static JsonValue Number(string x)
        {
            return new JsonValue(x, JsonValueType.Number);
        }

        /// <summary>
        /// Converts a value into string JSON value.
        /// </summary>
        /// <param name="x">The value.</param>
        /// <returns>The result JSON value.</returns>
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

        /// <summary>
        /// Gets the text of this value.
        /// </summary>
        public readonly string Text;

        /// <summary>
        /// Gets the type of this value.
        /// </summary>
        public readonly JsonValueType Type;

        /// <summary>
        /// Gets a value indicates whether this JSON value is default.
        /// </summary>
        public bool IsDefault
        {
            get { return Type == 0 && Text == null; }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Text;
        }
    }
}
