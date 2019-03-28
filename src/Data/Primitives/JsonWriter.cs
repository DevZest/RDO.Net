using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace DevZest.Data.Primitives
{
    public abstract class JsonWriter
    {
        private sealed class StringJsonWriter : JsonWriter
        {
            public StringJsonWriter(StringBuilder stringBuilder, IJsonCustomizer customizer)
                : base(customizer)
            {
                _stringBuilder = stringBuilder;
            }

            private readonly StringBuilder _stringBuilder;

            private void Write(char value)
            {
                _stringBuilder.Append(value);
            }

            private void Write(string value)
            {
                _stringBuilder.Append(value);
            }

            private void Write(string value, int startIndex, int num)
            {
                _stringBuilder.Append(value, startIndex, num);
            }

            protected override void PerformWriteStartObject()
            {
                Write('{');
            }

            protected override void PerformWriteEndObject()
            {
                Write('}');
            }

            protected override void PerformWriteStartArray()
            {
                Write('[');
            }

            protected override void PerformWriteEndArray()
            {
                Write(']');
            }

            protected override void PerformWriteComma()
            {
                Write(',');
            }

            protected override void PerformWritePropertyName(string name)
            {
                WriteValue(JsonValue.String(name));
                Write(':');
            }

            protected override void PerformWriteValue(JsonValue value)
            {
                if (value.Type == JsonValueType.String)
                    Write('"');

                if (value.Type != JsonValueType.String)
                    Write(value.Text);
                else
                    WriteEscapedText(value.Text);

                if (value.Type == JsonValueType.String)
                    Write('"');
            }

            private void WriteEscapedText(string value)
            {
                if (string.IsNullOrEmpty(value))
                    return;

                int startIndex = 0;
                int num = 0;
                int i = 0;
                for (i = 0; i < value.Length; i++)
                {
                    char c = value[i];
                    if (!RequiresEscape(c))
                    {
                        num++;
                        continue;
                    }

                    if (num > 0)
                        Write(value, startIndex, num);

                    startIndex = i + 1;
                    num = 0;
                    WriteEscape(c);
                }

                if (num > 0)
                    Write(value, startIndex, num);
            }

            private static bool RequiresEscape(char c)
            {
                return c < ' ' || c == '"' || c == '\\' || c == '\u0085' || c == '\u2028' || c == '\u2029';
            }

            private void WriteEscape(char c)
            {
                switch (c)
                {
                    case '"':
                        Write("\\\"");
                        break;
                    case '\\':
                        Write("\\\\");
                        break;
                    case '\b':
                        Write("\\b");
                        break;
                    case '\f':
                        Write("\\f");
                        break;
                    case '\n':
                        Write("\\n");
                        break;
                    case '\r':
                        Write("\\r");
                        break;
                    case '\t':
                        Write("\\t");
                        break;
                    default:
                        Write("\\u");
                        int num = (int)c;
                        Write(num.ToString("x4", CultureInfo.InvariantCulture));
                        break;
                }
            }

            public override string ToString(bool isPretty)
            {
                var result = _stringBuilder.ToString();
                if (isPretty)
                    result = PrettyPrint(result);
                return result;
            }
        }

        public static JsonWriter Create(IJsonCustomizer customizer)
        {
            return Create(new StringBuilder(), customizer);
        }

        public static JsonWriter Create(StringBuilder stringBuilder, IJsonCustomizer customizer)
        {
            return new StringJsonWriter(stringBuilder, customizer);
        }

        protected JsonWriter(IJsonCustomizer customizer)
        {
            Customizer = customizer;
        }

        protected IJsonCustomizer Customizer { get; }

        public JsonValue Serialize(Column column, int rowOrdinal)
        {
            var jsonConverter = Customizer?.GetConverter(column);
            if (jsonConverter != null)
                return jsonConverter.Serialize(column, rowOrdinal);
            else
                return column.Serialize(rowOrdinal);
        }

        public JsonWriter WriteStartObject()
        {
            PerformWriteStartObject();
            return this;
        }

        protected abstract void PerformWriteStartObject();

        public JsonWriter WriteEndObject()
        {
            PerformWriteEndObject();
            return this;
        }

        protected abstract void PerformWriteEndObject();

        public JsonWriter WriteStartArray()
        {
            PerformWriteStartArray();
            return this;
        }

        protected abstract void PerformWriteStartArray();

        public JsonWriter WriteEndArray()
        {
            PerformWriteEndArray();
            return this;
        }

        protected abstract void PerformWriteEndArray();

        public JsonWriter WriteNameStringPair(string name, string value)
        {
            return WriteNameValuePair(name, JsonValue.String(value));
        }

        public JsonWriter WriteNameValuePair(string name, JsonValue value)
        {
            return WritePropertyName(name).WriteValue(value);
        }

        public JsonWriter WriteComma()
        {
            PerformWriteComma();
            return this;
        }

        protected abstract void PerformWriteComma();

        public JsonWriter WritePropertyName(string name)
        {
            PerformWritePropertyName(name);
            return this;
        }

        protected abstract void PerformWritePropertyName(string name);

        public JsonWriter WriteValue(JsonValue value)
        {
            PerformWriteValue(value);
            return this;
        }

        protected abstract void PerformWriteValue(JsonValue value);

        public JsonWriter WriteArray<T>(IEnumerable<T> array, Action<JsonWriter, T> writeItemAction)
        {
            int count = 0;
            WriteStartArray();
            foreach (var item in array)
            {
                if (count > 0)
                    WriteComma();
                writeItemAction(this, item);
                count++;
            }
            WriteEndArray();
            return this;
        }

        public JsonWriter WriteNameArrayPair<T>(string name, IEnumerable<T> array, Action<JsonWriter, T> writeItemAction)
        {
            return WritePropertyName(name).WriteArray<T>(array, writeItemAction);
        }

        private const string INDENT = "   ";

        private static void AppendIndent(StringBuilder stringBuilder, int count)
        {
            for (; count > 0; --count)
                stringBuilder.Append(INDENT);
        }

        private static string PrettyPrint(string input)
        {
            var output = new StringBuilder();
            int depth = 0;
            int len = input.Length;
            char[] chars = input.ToCharArray();
            for (int i = 0; i < len; ++i)
            {
                char ch = chars[i];

                if (ch == '\"') // found string span
                {
                    bool str = true;
                    while (str)
                    {
                        output.Append(ch);
                        ch = chars[++i];
                        if (ch == '\\')
                        {
                            output.Append(ch);
                            ch = chars[++i];
                        }
                        else if (ch == '\"')
                            str = false;
                    }
                }

                if (ch == '[' && chars[i + 1] == ']')
                {
                    output.Append(ch);
                    output.Append(chars[++i]);
                    continue;
                }

                switch (ch)
                {
                    case '{':
                    case '[':
                        output.Append(ch);
                        output.AppendLine();
                        AppendIndent(output, ++depth);
                        break;
                    case '}':
                    case ']':
                        output.AppendLine();
                        AppendIndent(output, --depth);
                        output.Append(ch);
                        break;
                    case ',':
                        output.Append(ch);
                        output.AppendLine();
                        AppendIndent(output, depth);
                        break;
                    case ':':
                        output.Append(" : ");
                        break;
                    default:
                        if (!char.IsWhiteSpace(ch))
                            output.Append(ch);
                        break;
                }
            }

            return output.ToString();
        }

        public override string ToString()
        {
            return ToString(true);
        }

        public abstract string ToString(bool isPretty);
    }
}
