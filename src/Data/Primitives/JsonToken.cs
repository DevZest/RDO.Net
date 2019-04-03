namespace DevZest.Data.Primitives
{
    public struct JsonToken
    {
        public static readonly JsonToken Eof = new JsonToken(JsonTokenKind.Eof, string.Empty);
        public static readonly JsonToken CurlyOpen = new JsonToken(JsonTokenKind.CurlyOpen, "{");
        public static readonly JsonToken CurlyClose = new JsonToken(JsonTokenKind.CurlyClose, "}");
        public static readonly JsonToken SquaredOpen = new JsonToken(JsonTokenKind.SquaredOpen, "[");
        public static readonly JsonToken SquaredClose = new JsonToken(JsonTokenKind.SquaredClose, "]");
        public static readonly JsonToken Comma = new JsonToken(JsonTokenKind.Comma, ",");
        public static readonly JsonToken True = new JsonToken(JsonTokenKind.True, "true");
        public static readonly JsonToken False = new JsonToken(JsonTokenKind.False, "false");
        public static readonly JsonToken Null = new JsonToken(JsonTokenKind.Null, "null");

        public static JsonToken PropertyName(string propertyName)
        {
            propertyName.VerifyNotEmpty(nameof(propertyName));
            return new JsonToken(JsonTokenKind.PropertyName, propertyName);
        }

        public static JsonToken String(string text)
        {
            return new JsonToken(JsonTokenKind.String, text);
        }

        public static JsonToken Number(string text)
        {
            return new JsonToken(JsonTokenKind.Number, text);
        }

        private JsonToken(JsonTokenKind kind, string text)
        {
            Kind = kind;
            Text = text;
        }

        public readonly JsonTokenKind Kind;
        public readonly string Text;

        public JsonValue JsonValue
        {
            get { return ((Kind & JsonTokenKind.ColumnValues) == Kind) ? new JsonValue(Text, (JsonValueType)Kind) : default(JsonValue); }
        }
    }
}
