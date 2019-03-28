using System;
using System.Collections.Generic;
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

            protected internal override void Write(char value)
            {
                _stringBuilder.Append(value);
            }

            protected internal override void Write(string value)
            {
                _stringBuilder.Append(value);
            }

            protected internal override void Write(string value, int startIndex, int num)
            {
                _stringBuilder.Append(value, startIndex, num);
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

        protected internal abstract void Write(char value);

        protected internal virtual void Write(string value)
        {
            if (value == null)
                return;

            for (int i = 0; i < value.Length; i++)
                Write(value[i]);
        }

        protected internal virtual void Write(string value, int startIndex, int num)
        {
            for (int i = 0; i < num; i++)
                Write(value[startIndex + i]);
        }

        public JsonWriter WriteStartObject()
        {
            Write('{');
            return this;
        }

        public JsonWriter WriteEndObject()
        {
            Write('}');
            return this;
        }

        public JsonWriter WriteStartArray()
        {
            Write('[');
            return this;
        }

        public JsonWriter WriteEndArray()
        {
            Write(']');
            return this;
        }

        public JsonWriter WriteNameStringPair(string name, string value)
        {
            return WriteNameValuePair(name, JsonValue.String(value));
        }

        public JsonWriter WriteNameValuePair(string name, JsonValue value)
        {
            return WriteObjectName(name).WriteValue(value);
        }

        public JsonWriter WriteComma()
        {
            Write(',');
            return this;
        }

        public JsonWriter WriteObjectName(string name)
        {
            JsonValue.String(name).Write(this);
            Write(':');
            return this;
        }

        public JsonWriter WriteValue(JsonValue value)
        {
            value.Write(this);
            return this;
        }

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
            return WriteObjectName(name).WriteArray<T>(array, writeItemAction);
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
