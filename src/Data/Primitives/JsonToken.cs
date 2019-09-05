namespace DevZest.Data.Primitives
{
    /// <summary>
    /// Represents token used to parse JSON data.
    /// </summary>
    public struct JsonToken
    {
        /// <summary>
        /// Gets the Eof token.
        /// </summary>
        public static readonly JsonToken Eof = new JsonToken(JsonTokenKind.Eof, string.Empty);

        /// <summary>
        /// Gets the curly open '{' token.
        /// </summary>
        public static readonly JsonToken CurlyOpen = new JsonToken(JsonTokenKind.CurlyOpen, "{");

        /// <summary>
        /// Gets the curly close '}' token.
        /// </summary>
        public static readonly JsonToken CurlyClose = new JsonToken(JsonTokenKind.CurlyClose, "}");

        /// <summary>
        /// Gets the squared open '[' token.
        /// </summary>
        public static readonly JsonToken SquaredOpen = new JsonToken(JsonTokenKind.SquaredOpen, "[");

        /// <summary>
        /// Gets the squared close ']' token.
        /// </summary>
        public static readonly JsonToken SquaredClose = new JsonToken(JsonTokenKind.SquaredClose, "]");

        /// <summary>
        /// Gets the comma ',' token.
        /// </summary>
        public static readonly JsonToken Comma = new JsonToken(JsonTokenKind.Comma, ",");

        /// <summary>
        /// Gets the 'true' literal token.
        /// </summary>
        public static readonly JsonToken True = new JsonToken(JsonTokenKind.True, "true");

        /// <summary>
        /// Gets the 'false' literal token.
        /// </summary>
        public static readonly JsonToken False = new JsonToken(JsonTokenKind.False, "false");

        /// <summary>
        /// Gets the 'null' literal token.
        /// </summary>
        public static readonly JsonToken Null = new JsonToken(JsonTokenKind.Null, "null");

        /// <summary>
        /// Returns a token for specified property name.
        /// </summary>
        /// <param name="propertyName">The property name.</param>
        /// <returns>The token.</returns>
        public static JsonToken PropertyName(string propertyName)
        {
            propertyName.VerifyNotEmpty(nameof(propertyName));
            return new JsonToken(JsonTokenKind.PropertyName, propertyName);
        }

        /// <summary>
        /// Returns a token for specified text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The token.</returns>
        public static JsonToken String(string text)
        {
            return new JsonToken(JsonTokenKind.String, text);
        }

        /// <summary>
        /// Returns a token for specified number.
        /// </summary>
        /// <param name="text">The number text.</param>
        /// <returns>The token.</returns>
        public static JsonToken Number(string text)
        {
            return new JsonToken(JsonTokenKind.Number, text);
        }

        private JsonToken(JsonTokenKind kind, string text)
        {
            Kind = kind;
            Text = text;
        }

        /// <summary>
        /// Gets the kind of token.
        /// </summary>
        public readonly JsonTokenKind Kind;

        /// <summary>
        /// Gets the text of token.
        /// </summary>
        public readonly string Text;

        /// <summary>
        /// Gets the <see cref="JsonValue"/>  of the token.
        /// </summary>
        public JsonValue JsonValue
        {
            get { return ((Kind & JsonTokenKind.ColumnValues) == Kind) ? new JsonValue(Text, (JsonValueType)Kind) : default(JsonValue); }
        }
    }
}
