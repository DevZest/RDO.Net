using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace DevZest.Data.Primitives
{
    public abstract class JsonReader
    {
        public static JsonReader Create(string json, IJsonCustomizer customizer)
        {
            return new StringJsonReader(json, customizer);
        }

        private sealed class StringJsonReader : JsonReader
        {
            readonly string _json;
            int _index;
            readonly StringBuilder _stringBuilder = new StringBuilder();

            public StringJsonReader(string json, IJsonCustomizer customizer)
                : base(customizer)
            {
                _json = json.VerifyNotEmpty(nameof(json));
            }

            private bool ConsumeNextWhitespaceChar(out char result)
            {
                if (!NextNonWhitespaceChar(out result))
                    return false;

                ConsumeCurrentChar();
                return true;
            }

            private void ConsumeCurrentChar()
            {
                Debug.Assert(_index < _json.Length);
                _index++;
            }

            private bool NextNonWhitespaceChar(out char result)
            {
                SkipWhitespace();
                if (_index == _json.Length)
                {
                    result = default(char);
                    return false;
                }

                result = _json[_index];
                return true;
            }

            private void SkipWhitespace()
            {
                while (_index < _json.Length)
                {
                    var currentChar = _json[_index];
                    if (currentChar > ' ')
                        return;

                    if (currentChar == ' ' || currentChar == '\t' || currentChar == '\n' || currentChar == '\r')
                        _index++;
                }
            }

            private bool ConsumeNextColon()
            {
                if (!NextNonWhitespaceChar(out var c))
                    return false;

                if (c == ':')
                {
                    ConsumeCurrentChar();
                    return true;
                }

                return false;
            }

            protected override JsonToken NextToken()
            {
                if (!ConsumeNextWhitespaceChar(out var c))
                    return JsonToken.Eof;

                switch (c)
                {
                    case '{':
                        return JsonToken.CurlyOpen;

                    case '}':
                        return JsonToken.CurlyClose;

                    case '[':
                        return JsonToken.SquaredOpen;

                    case ']':
                        return JsonToken.SquaredClose;

                    case ',':
                        return JsonToken.Comma;

                    case '"':
                        var stringValue = ParseStringValue();
                        var hasNextColon = ConsumeNextColon();
                        return hasNextColon ? JsonToken.PropertyName(stringValue) : JsonToken.String(stringValue);

                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                    case '-':
                    case '+':
                    case '.':
                        return JsonToken.Number(ParseNumberToken());

                    case 'f':
                        ExpectLiteral("false");
                        return JsonToken.False;

                    case 't':
                        ExpectLiteral("true");
                        return JsonToken.True;

                    case 'n':
                        ExpectLiteral("null");
                        return JsonToken.Null;
                }

                throw new FormatException(DiagnosticMessages.StringJsonReader_InvalidChar(c, _index - 1));
            }

            private string ParseNumberToken()
            {
                var startIndex = _index - 1;
                while (_index < _json.Length)
                {
                    var c = _json[_index];

                    if ((c >= '0' && c <= '9') || c == '.' || c == '-' || c == '+' || c == 'e' || c == 'E')
                    {
                        _index++;
                        continue;
                    }

                    break;
                }

                return _json.Substring(startIndex, _index - startIndex);
            }

            private string ParseStringValue()
            {
                _stringBuilder.Length = 0;

                int runIndex = -1;

                while (_index < _json.Length)
                {
                    var c = _json[_index++];

                    if (c == '"')
                    {
                        if (runIndex != -1)
                        {
                            if (_stringBuilder.Length == 0)
                                return _json.Substring(runIndex, _index - runIndex - 1);

                            _stringBuilder.Append(_json, runIndex, _index - runIndex - 1);
                        }
                        return _stringBuilder.ToString();
                    }

                    if (c != '\\')
                    {
                        if (runIndex == -1)
                            runIndex = _index - 1;
                        continue;
                    }

                    if (runIndex != -1)
                    {
                        _stringBuilder.Append(_json, runIndex, _index - runIndex - 1);
                        runIndex = -1;
                    }

                    ParseStringEscape();
                }

                throw new FormatException(DiagnosticMessages.StringJsonReader_UnexpectedEof);
            }

            private void ParseStringEscape()
            {
                Debug.Assert(_json[_index - 1] == '\\');

                if (_index == _json.Length)
                    throw new FormatException(DiagnosticMessages.StringJsonReader_UnexpectedEof);

                var c = _json[_index++];
                switch (c)
                {
                    case '"':
                        _stringBuilder.Append('"');
                        break;

                    case '\\':
                        _stringBuilder.Append('\\');
                        break;

                    case '/':
                        _stringBuilder.Append('/');
                        break;

                    case 'b':
                        _stringBuilder.Append('\b');
                        break;

                    case 'f':
                        _stringBuilder.Append('\f');
                        break;

                    case 'n':
                        _stringBuilder.Append('\n');
                        break;

                    case 'r':
                        _stringBuilder.Append('\r');
                        break;

                    case 't':
                        _stringBuilder.Append('\t');
                        break;

                    case 'u':
                        ParseUnicodeChar();
                        break;

                    default:
                        throw new FormatException(DiagnosticMessages.StringJsonReader_InvalidStringEscape(c, _index - 1));
                }

            }

            private void ParseUnicodeChar()
            {
                int remainingLength = _json.Length - _index;
                if (remainingLength < 4)
                    throw new FormatException(DiagnosticMessages.StringJsonReader_UnexpectedEof);

                uint codePoint = ParseHex(_json[_index], _json[_index + 1], _json[_index + 2], _json[_index + 3]);
                _stringBuilder.Append((char)codePoint);

                _index += 4;
            }

            private uint ParseHex(char c1, char c2, char c3, char c4)
            {
                uint p1 = ParseHexChar(c1);
                uint p2 = ParseHexChar(c2);
                uint p3 = ParseHexChar(c3);
                uint p4 = ParseHexChar(c4);

                return (p1 << 24) + (p2 << 16) + (p3 << 8) + p4;
            }

            private uint ParseHexChar(char c)
            {
                if (c >= '0' && c <= '9')
                    return (uint)(c - '0');
                else if (c >= 'A' && c <= 'F')
                    return (uint)((c - 'A') + 10);
                else if (c >= 'a' && c <= 'f')
                    return (uint)((c - 'a') + 10);
                else
                    throw new FormatException(DiagnosticMessages.StringJsonReader_InvalidHexChar(c, _index - 1));
            }

            private void ExpectLiteral(string literal)
            {
                Debug.Assert(_json[_index - 1] == literal[0]);

                for (int i = 1; i < literal.Length; i++)
                {
                    if (literal[i] != _json[_index++])
                        throw new FormatException(DiagnosticMessages.StringJsonReader_InvalidLiteral(literal, _index - i));
                }
            }
        }

        JsonToken? _lookAhead;

        protected JsonReader(IJsonCustomizer customer)
        {
            Customizer = Customizer;
        }

        protected IJsonCustomizer Customizer { get; }

        public bool IsDeserializable(Column column)
        {
            return Customizer == null ? column.IsDeserializable : Customizer.IsDeserializable(column);
        }

        public void Deserialize(Column column, int rowOrdinal, JsonValue value)
        {
            var jsonConverter = Customizer?.GetConverter(column);
            if (jsonConverter != null)
                jsonConverter.Deserialize(column, rowOrdinal, value);
            else
                column.Deserialize(rowOrdinal, value);
        }

        public JsonToken PeekToken()
        {
            if (!_lookAhead.HasValue)
                _lookAhead = NextToken();

            return _lookAhead.GetValueOrDefault();
        }

        public void ConsumeToken()
        {
            Debug.Assert(_lookAhead.HasValue);
            _lookAhead = null;
        }

        public virtual void ExpectEof()
        {
            InternalExpectToken(JsonTokenKind.Eof);
        }

        public JsonToken ExpectToken(JsonTokenKind expectedTokenKind)
        {
            if (expectedTokenKind == JsonTokenKind.Eof)
                throw new ArgumentException(DiagnosticMessages.JsonReader_ExpectToken_Eof, nameof(expectedTokenKind));
            return InternalExpectToken(expectedTokenKind);
        }

        private JsonToken InternalExpectToken(JsonTokenKind expectedTokenKind)
        {
            var currentToken = PeekToken();
            ConsumeToken();

            if ((currentToken.Kind & expectedTokenKind) != currentToken.Kind)
                throw new FormatException(DiagnosticMessages.JsonReader_InvalidTokenKind(currentToken.Kind, expectedTokenKind));
            return currentToken;
        }

        internal void ExpectComma()
        {
            ExpectToken(JsonTokenKind.Comma);
        }

        public void ExpectPropertyName(string propertyName)
        {
            var tokenText = ExpectToken(JsonTokenKind.PropertyName).Text;
            if (tokenText != propertyName)
                throw new FormatException(DiagnosticMessages.JsonReader_InvalidObjectName(tokenText, propertyName));
        }

        public string ExpectNullableStringProperty(string objectName, bool expectComma)
        {
            string result;
            ExpectPropertyName(objectName);
            if (PeekToken().Kind == JsonTokenKind.Null)
            {
                ConsumeToken();
                result = null;
            }
            else
                result = ExpectToken(JsonTokenKind.String).Text;
            if (expectComma)
                ExpectToken(JsonTokenKind.Comma);
            return result;
        }

        public string ExpectStringProperty(string objectName, bool expectComma)
        {
            ExpectPropertyName(objectName);
            var result = ExpectToken(JsonTokenKind.String).Text;
            if (expectComma)
                ExpectToken(JsonTokenKind.Comma);
            return result;
        }

        protected abstract JsonToken NextToken();
    }
}
