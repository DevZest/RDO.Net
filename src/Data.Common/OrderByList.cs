using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace DevZest.Data
{
    public static class OrderByList
    {
        private const string COLUMN = nameof(OrderBy.Column);
        private const string DIRECTION = nameof(OrderBy.Direction);

        public static string ToJson(this IReadOnlyList<OrderBy> orderByList, bool isPretty)
        {
            if (orderByList == null)
                return null;

            var result = new StringBuilder().WriteOrderByList(orderByList).ToString();
            if (isPretty)
                result = JsonFormatter.PrettyPrint(result);
            return result;
        }

        private static StringBuilder WriteOrderByList(this StringBuilder stringBuilder, IReadOnlyList<OrderBy> orderByList)
        {
            stringBuilder.WriteStartArray();
            for (int i = 0; i < orderByList.Count; i++)
            {
                stringBuilder.WriteOrderBy(orderByList[i]);
                if (i < orderByList.Count - 1)
                    stringBuilder.WriteComma();
            }
            stringBuilder.WriteEndArray();
            return stringBuilder;
        }

        private static StringBuilder WriteOrderBy(this StringBuilder stringBuilder, OrderBy orderBy)
        {
            return stringBuilder.WriteStartObject()
                .WriteNameColumnPair(COLUMN, orderBy.Column).WriteComma()
                .WriteNameValuePair(DIRECTION, JsonValue.FastString(orderBy.Direction.ToString()))
                .WriteEndObject();
        }

        public static IReadOnlyList<OrderBy> ParseJson(Model model, string json)
        {
            return string.IsNullOrEmpty(json) ? null : new OrderByJsonParser(json).ParseOrderByList(model);
        }

        private class OrderByJsonParser : ColumnJsonParser
        {
            internal OrderByJsonParser(string json)
                : base(json)
            {
            }

            public IReadOnlyList<OrderBy> ParseOrderByList(Model model)
            {
                var result = new List<OrderBy>();

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

                return result;
            }

            private OrderBy ParseOrderBy(Model model)
            {
                ExpectToken(TokenKind.CurlyOpen);
                ExpectObjectName(COLUMN);
                var column = ParseColumn<Column>(model);
                ExpectComma();
                var direction = ParseDirection();
                ExpectToken(TokenKind.CurlyClose);
                return new OrderBy(column, direction);
            }

            private SortDirection ParseDirection()
            {
                var value = ExpectString(DIRECTION, false);
                return (SortDirection)Enum.Parse(typeof(SortDirection), value);
            }
        }
    }
}
