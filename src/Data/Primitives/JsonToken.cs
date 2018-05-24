using System.Diagnostics;

namespace DevZest.Data.Primitives
{
    public struct JsonToken
    {
        internal static readonly JsonToken Eof = new JsonToken(JsonTokenKind.Eof, string.Empty);
        internal static readonly JsonToken CurlyOpen = new JsonToken(JsonTokenKind.CurlyOpen, "{");
        internal static readonly JsonToken CurlyClose = new JsonToken(JsonTokenKind.CurlyClose, "}");
        internal static readonly JsonToken SquaredOpen = new JsonToken(JsonTokenKind.SquaredOpen, "[");
        internal static readonly JsonToken SquaredClose = new JsonToken(JsonTokenKind.SquaredClose, "]");
        internal static readonly JsonToken Colon = new JsonToken(JsonTokenKind.Colon, ":");
        internal static readonly JsonToken Comma = new JsonToken(JsonTokenKind.Comma, ",");
        internal static readonly JsonToken True = new JsonToken(JsonTokenKind.True, "true");
        internal static readonly JsonToken False = new JsonToken(JsonTokenKind.False, "false");
        internal static readonly JsonToken Null = new JsonToken(JsonTokenKind.Null, "null");

        internal static JsonToken String(string text)
        {
            return new JsonToken(JsonTokenKind.String, text);
        }

        internal static JsonToken Number(string text)
        {
            return new JsonToken(JsonTokenKind.Number, text);
        }

        internal JsonToken(JsonTokenKind kind, string text)
        {
            Kind = kind;
            Text = text;
        }

        public readonly JsonTokenKind Kind;
        public readonly string Text;

        public JsonValue JsonValue
        {
            get
            {
                Debug.Assert((Kind & JsonTokenKind.ColumnValues) == Kind);
                return new JsonValue(Text, false, (JsonValueType)Kind);
            }
        }
    }
}
