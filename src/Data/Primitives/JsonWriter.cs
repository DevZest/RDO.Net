using System;
using System.Collections.Generic;
using System.Text;

namespace DevZest.Data.Primitives
{
    public struct JsonWriter
    {
        public static JsonWriter New()
        {
            return new JsonWriter(new StringBuilder());
        }

        private JsonWriter(StringBuilder stringBuilder)
        {
            _stringBuilder = stringBuilder;
        }

        private StringBuilder _stringBuilder;
        public StringBuilder StringBuilder
        {
            get { return _stringBuilder; }
        }

        private StringBuilder RequireStringBuilder()
        {
            if (_stringBuilder == null)
                throw new InvalidOperationException(DiagnosticMessages.JsonWriter_Empty);
            return _stringBuilder;
        }

        public JsonWriter WriteStartObject()
        {
            RequireStringBuilder().Append('{');
            return this;
        }

        public JsonWriter WriteEndObject()
        {
            RequireStringBuilder().Append('}');
            return this;
        }

        public JsonWriter WriteStartArray()
        {
            RequireStringBuilder().Append('[');
            return this;
        }

        public JsonWriter WriteEndArray()
        {
            RequireStringBuilder().Append(']');
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
            RequireStringBuilder().Append(',');
            return this;
        }

        public JsonWriter WriteObjectName(string name)
        {
            JsonValue.String(name).Write(RequireStringBuilder());
            RequireStringBuilder().Append(":");
            return this;
        }

        public JsonWriter WriteValue(JsonValue value)
        {
            value.Write(RequireStringBuilder());
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

        public string ToString(bool isPretty)
        {
            if (_stringBuilder == null)
                return null;

            var result = _stringBuilder.ToString();
            if (isPretty)
                result = PrettyPrint(result);
            return result;
        }
    }
}
