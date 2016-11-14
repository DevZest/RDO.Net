using DevZest.Data.Primitives;
using System;
using System.Collections.Generic;

namespace DevZest.Data
{
    public static class OrderByJson
    {
        private const string COLUMN = nameof(ColumnSort.Column);
        private const string DIRECTION = nameof(ColumnSort.Direction);

        public static string ToJson(this IEnumerable<ColumnSort> orderBy, bool isPretty)
        {
            if (orderBy == null)
                return null;

            return JsonWriter.New().Write(orderBy).ToString(isPretty);
        }

        public static JsonWriter Write(this JsonWriter jsonWriter, IEnumerable<ColumnSort> orderByList)
        {
            jsonWriter.WriteStartArray();
            var count = 0;
            foreach (var orderBy in orderByList)
            {
                if (count > 0)
                    jsonWriter.WriteComma();
                jsonWriter.Write(orderBy);
                count++;
            }
            return jsonWriter.WriteEndArray();
        }

        public static JsonWriter Write(this JsonWriter jsonWriter, ColumnSort orderBy)
        {
            return jsonWriter.WriteStartObject()
                .WriteNameColumnPair(COLUMN, orderBy.Column).WriteComma()
                .WriteNameValuePair(DIRECTION, JsonValue.FastString(orderBy.Direction.ToString()))
                .WriteEndObject();
        }

        public static ColumnSort[] ParseJson(Model model, string json)
        {
            return string.IsNullOrEmpty(json) ? null : new OrderByJsonParser(json).ParseOrderByList(model);
        }

        private class OrderByJsonParser : ColumnJsonParser
        {
            internal OrderByJsonParser(string json)
                : base(json)
            {
            }

            public ColumnSort[] ParseOrderByList(Model model)
            {
                var result = new List<ColumnSort>();

                ExpectToken(TokenKind.SquaredOpen);

                while (PeekToken().Kind == TokenKind.CurlyOpen)
                {
                    result.Add(ParseOrderBy(model));

                    while (PeekToken().Kind == TokenKind.Comma)
                    {
                        ConsumeToken();
                        result.Add(ParseOrderBy(model));
                    }
                }

                ExpectToken(TokenKind.SquaredClose);
                ExpectToken(TokenKind.Eof);

                return result.ToArray();
            }

            private ColumnSort ParseOrderBy(Model model)
            {
                ExpectToken(TokenKind.CurlyOpen);
                ExpectObjectName(COLUMN);
                var column = ParseColumn<Column>(model);
                ExpectComma();
                var direction = ParseDirection();
                ExpectToken(TokenKind.CurlyClose);
                return new ColumnSort(column, direction);
            }

            private SortDirection ParseDirection()
            {
                var value = ExpectString(DIRECTION, false);
                return (SortDirection)Enum.Parse(typeof(SortDirection), value);
            }
        }
    }
}
