using System;
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

        internal static StringBuilder Write(this StringBuilder stringBuilder, Action<StringBuilder> action)
        {
            action(stringBuilder);
            return stringBuilder;
        }

        internal static StringBuilder WritePair(this StringBuilder stringBuilder, string name, Action<StringBuilder> action)
        {
            stringBuilder.WriteObjectName(name);
            action(stringBuilder);
            return stringBuilder;
        }

        internal static StringBuilder WriteObjectName(this StringBuilder stringBuilder, string name)
        {
            stringBuilder.Append("\"");
            stringBuilder.Append(name);
            stringBuilder.Append("\"");
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
