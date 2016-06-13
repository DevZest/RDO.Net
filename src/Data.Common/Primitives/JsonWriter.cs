using System.Text;

namespace DevZest.Data.Primitives
{
    internal static class JsonWriter
    {
        internal static StringBuilder WriteStartObject(this StringBuilder stringBuilder)
        {
            stringBuilder.Append('{');
            return stringBuilder;
        }

        internal static StringBuilder WriteEndObject(this StringBuilder stringBuilder)
        {
            stringBuilder.Append('}');
            return stringBuilder;
        }

        internal static StringBuilder WriteStartArray(this StringBuilder stringBuilder)
        {
            stringBuilder.Append('[');
            return stringBuilder;
        }

        internal static StringBuilder WriteEndArray(this StringBuilder stringBuilder)
        {
            stringBuilder.Append(']');
            return stringBuilder;
        }

        internal static StringBuilder WriteNameStringPair(this StringBuilder stringBuilder, string name, string value)
        {
            return stringBuilder.WriteNameValuePair(name, JsonValue.String(value));
        }

        internal static StringBuilder WriteNameValuePair(this StringBuilder stringBuilder, string name, JsonValue value)
        {
            return stringBuilder.WriteObjectName(name).WriteValue(value);
        }

        internal static StringBuilder WriteComma(this StringBuilder stringBuilder)
        {
            return stringBuilder.Append(',');
        }

        internal static StringBuilder WriteObjectName(this StringBuilder stringBuilder, string name)
        {
            JsonValue.FastString(name).Write(stringBuilder);
            stringBuilder.Append(":");
            return stringBuilder;
        }

        internal static StringBuilder WriteValue(this StringBuilder stringBuilder, JsonValue value)
        {
            value.Write(stringBuilder);
            return stringBuilder;
        }
    }
}
