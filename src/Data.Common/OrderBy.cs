using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace DevZest.Data
{
    public static class OrderBy
    {
        private const string COLUMN = nameof(ColumnSort.Column);
        private const string DIRECTION = nameof(ColumnSort.Direction);

        public static string ToJson(this IReadOnlyList<ColumnSort> orderBy, bool isPretty)
        {
            if (orderBy == null)
                return null;

            var result = new StringBuilder().WriteOrderByList(orderBy).ToString();
            if (isPretty)
                result = JsonFormatter.PrettyPrint(result);
            return result;
        }

        private static StringBuilder WriteOrderByList(this StringBuilder stringBuilder, IReadOnlyList<ColumnSort> orderByList)
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

        private static StringBuilder WriteOrderBy(this StringBuilder stringBuilder, ColumnSort orderBy)
        {
            return stringBuilder.WriteStartObject()
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
