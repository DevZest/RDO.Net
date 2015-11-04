using System;
using System.Diagnostics;
using System.Text;

namespace DevZest.Data.Primitives
{
    internal sealed class JsonParser
    {
        [Flags]
        private enum TokenKind
        {
            Eof = 0,
            String = JsonValueType.String,
            Number = JsonValueType.Number,
            True = JsonValueType.True,
            False = JsonValueType.False,
            Null = JsonValueType.Null,
            CurlyOpen = 0x20,
            CurlyClose = 0x40,
            SquaredOpen = 0x80,
            SquaredClose = 0x100,
            Colon = 0x200,
            Comma = 0x400,
            ColumnValues = String | Number | True | False | Null
        }

        private struct Token
        {
            public static readonly Token Eof = new Token(TokenKind.Eof, string.Empty);
            public static readonly Token CurlyOpen = new Token(TokenKind.CurlyOpen, "{");
            public static readonly Token CurlyClose = new Token(TokenKind.CurlyClose, "}");
            public static readonly Token SquaredOpen = new Token(TokenKind.SquaredOpen, "[");
            public static readonly Token SquaredClose = new Token(TokenKind.SquaredClose, "]");
            public static readonly Token Colon = new Token(TokenKind.Colon, ":");
            public static readonly Token Comma = new Token(TokenKind.Comma, ",");
            public static readonly Token True = new Token(TokenKind.True, "true");
            public static readonly Token False = new Token(TokenKind.False, "false");
            public static readonly Token Null = new Token(TokenKind.Null, "null");
            public static Token String(string text)
            {
                return new Token(TokenKind.String, text);
            }

            public static Token Number(string text)
            {
                return new Token(TokenKind.Number, text);
            }

            private Token(TokenKind kind, string text)
            {
                Kind = kind;
                Text = text;
            }

            public readonly TokenKind Kind;
            public readonly string Text;

            public JsonValue JsonValue
            {
                get
                {
                    Debug.Assert((Kind & TokenKind.ColumnValues) == Kind);
                    return new JsonValue(Text, false, (JsonValueType)Kind);
                }
            }
        }

        public static void Parse(string json, DataSet dataSet)
        {
            var parser = new JsonParser(json);
            parser.Parse(dataSet, true);
        }

        readonly string _json;
        int _index;
        readonly StringBuilder s = new StringBuilder();
        Token? _lookAhead;

        private JsonParser(string json)
        {
            Debug.Assert(!string.IsNullOrEmpty(json));
            _json = json;
        }

        private void Parse(DataSet dataSet, bool isTopLevel)
        {
            ExpectToken(TokenKind.SquaredOpen);

            if (PeekToken().Kind == TokenKind.CurlyOpen)
            {
                var model = dataSet.Model;
                model.EnterDataSetInitialization();

                Parse(dataSet.AddRow());

                while (PeekToken().Kind == TokenKind.Comma)
                {
                    ConsumeToken();
                    Parse(dataSet.AddRow());
                }

                model.ExitDataSetInitialization();
            }

            ExpectToken(TokenKind.SquaredClose);
            if (isTopLevel)
                ExpectToken(TokenKind.Eof);
        }

        private void Parse(DataRow dataRow)
        {
            ExpectToken(TokenKind.CurlyOpen);

            var token = PeekToken();
            if (token.Kind == TokenKind.String)
            {
                ConsumeToken();
                Parse(dataRow, token.Text);

                while (PeekToken().Kind == TokenKind.Comma)
                {
                    ConsumeToken();
                    token = ExpectToken(TokenKind.String);
                    Parse(dataRow, token.Text);
                }
            }

            ExpectToken(TokenKind.CurlyClose);
        }

        private void Parse(DataRow dataRow, string memberName)
        {
            ExpectToken(TokenKind.Colon);

            var model = dataRow.Model;
            var member = model[memberName];
            if (member == null)
                throw new FormatException(Strings.JsonParser_InvalidModelMember(memberName, model.GetType().FullName));
            var column = member as Column;
            if (column != null)
                Parse(column, dataRow.Ordinal);
            else
                Parse(dataRow[(Model)member], false);
        }

        private void Parse(Column column, int ordinal)
        {
            Debug.Assert(column != null);

            var token = ExpectToken(TokenKind.ColumnValues);
            if (column.ShouldSerialize)
                column.Deserialize(ordinal, token.JsonValue);
        }

        private Token PeekToken()
        {
            if (!_lookAhead.HasValue)
                _lookAhead = NextToken();

            return _lookAhead.GetValueOrDefault();
        }

        private void ConsumeToken()
        {
            Debug.Assert(_lookAhead.HasValue);
            _lookAhead = null;
        }

        private Token ExpectToken(TokenKind expectedTokenKind)
        {
            var currentToken = PeekToken();
            ConsumeToken();

            if ((currentToken.Kind & expectedTokenKind) != currentToken.Kind)
                throw new FormatException(Strings.JsonParser_InvalidTokenKind(currentToken.Kind, expectedTokenKind));
            return currentToken;
        }

        private Token NextToken()
        {
            char c = new char();

            while (_index < _json.Length)
            {
                c = _json[_index++];

                if (c > ' ') break;
                if (c != ' ' && c != '\t' && c != '\n' && c != '\r') break;
            }

            if (_index == _json.Length)
                return Token.Eof;

            switch (c)
            {
                case '{':
                    return Token.CurlyOpen;

                case '}':
                    return Token.CurlyClose;

                case '[':
                    return Token.SquaredOpen;

                case ']':
                    return Token.SquaredClose;

                case ',':
                    return Token.Comma;

                case '"':
                    return Token.String(ParseStringToken());

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
                    return Token.String(ParseNumberToken());

                case ':':
                    return Token.Colon;

                case 'f':
                    ExpectLiteral("false");
                    return Token.False;

                case 't':
                    ExpectLiteral("true");
                    return Token.True;

                case 'n':
                    ExpectLiteral("null");
                    return Token.Null;
            }

            throw new FormatException(Strings.JsonParser_InvalidChar(c, _index - 1));
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

        private string ParseStringToken()
        {
            s.Length = 0;

            int runIndex = -1;

            while (_index < _json.Length)
            {
                var c = _json[_index++];

                if (c == '"')
                {
                    if (runIndex != -1)
                    {
                        if (s.Length == 0)
                            return _json.Substring(runIndex, _index - runIndex - 1);

                        s.Append(_json, runIndex, _index - runIndex - 1);
                    }
                    return s.ToString();
                }

                if (c != '\\')
                {
                    if (runIndex == -1)
                        runIndex = _index - 1;
                    continue;
                }

                if (runIndex != -1)
                {
                    s.Append(_json, runIndex, _index - runIndex - 1);
                    runIndex = -1;
                }

                ParseStringEscape();
            }

            throw new FormatException(Strings.JsonParser_UnexpectedEof);
        }

        private void ParseStringEscape()
        {
            Debug.Assert(_json[_index - 1] == '\\');

            if (_index == _json.Length)
                throw new FormatException(Strings.JsonParser_UnexpectedEof);

            var c = _json[_index++];
            switch (c)
            {
                case '"':
                    s.Append('"');
                    break;

                case '\\':
                    s.Append('\\');
                    break;

                case '/':
                    s.Append('/');
                    break;

                case 'b':
                    s.Append('\b');
                    break;

                case 'f':
                    s.Append('\f');
                    break;

                case 'n':
                    s.Append('\n');
                    break;

                case 'r':
                    s.Append('\r');
                    break;

                case 't':
                    s.Append('\t');
                    break;

                case 'u':
                    ParseUnicodeChar();
                    break;

                default:
                    throw new FormatException(Strings.JsonParser_InvalidStringEscape(c, _index - 1));
            }

        }

        private void ParseUnicodeChar()
        {
            int remainingLength = _json.Length - _index;
            if (remainingLength < 4)
                throw new FormatException(Strings.JsonParser_UnexpectedEof);

            uint codePoint = ParseHex(_json[_index], _json[_index + 1], _json[_index + 2], _json[_index + 3]);
            s.Append((char)codePoint);

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
                throw new FormatException(Strings.JsonParser_InvalidHexChar(c, _index - 1));
        }

        private void ExpectLiteral(string literal)
        {
            Debug.Assert(_json[_index - 1] == literal[0]);

            for (int i = 1; i < literal.Length; i++)
            {
                if (literal[i] != _json[_index++])
                    throw new FormatException(Strings.JsonParser_InvalidLiteral(literal, _index - i));
            }
        }
    }
}
