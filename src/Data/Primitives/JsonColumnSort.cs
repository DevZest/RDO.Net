using System;
using System.Collections.Generic;

namespace DevZest.Data.Primitives
{
    public static class JsonColumnSort
    {
        private const string COLUMN = nameof(ColumnSort.Column);
        private const string DIRECTION = nameof(ColumnSort.Direction);

        public static JsonWriter Write(this JsonWriter jsonWriter, IEnumerable<ColumnSort> orderBy)
        {
            jsonWriter.WriteStartArray();
            var count = 0;
            foreach (var columnSort in orderBy)
            {
                if (count > 0)
                    jsonWriter.WriteComma();
                jsonWriter.Write(columnSort);
                count++;
            }
            return jsonWriter.WriteEndArray();
        }

        public static JsonWriter Write(this JsonWriter jsonWriter, ColumnSort columnSort)
        {
            return jsonWriter.WriteStartObject()
                .WriteNameValuePair(COLUMN, JsonValue.String(columnSort.Column.Name)).WriteComma()
                .WriteNameValuePair(DIRECTION, JsonValue.String(columnSort.Direction.ToString()))
                .WriteEndObject();
        }

        public static IReadOnlyList<ColumnSort> ParseOrderBy(this JsonParser jsonParser, Model model)
        {
            List<ColumnSort> result = null;

            jsonParser.ExpectToken(JsonTokenKind.SquaredOpen);

            while (jsonParser.PeekToken().Kind == JsonTokenKind.CurlyOpen)
            {
                if (result == null)
                    result = new List<Data.ColumnSort>();
                result.Add(jsonParser.ParseColumnSort(model));

                while (jsonParser.PeekToken().Kind == JsonTokenKind.Comma)
                {
                    jsonParser.ConsumeToken();
                    result.Add(jsonParser.ParseColumnSort(model));
                }
            }

            jsonParser.ExpectToken(JsonTokenKind.SquaredClose);
            if (result == null)
                return Array.Empty<ColumnSort>();
            else
                return result;
        }

        public static ColumnSort ParseColumnSort(this JsonParser jsonParser, Model model)
        {
            jsonParser.ExpectToken(JsonTokenKind.CurlyOpen);
            jsonParser.ExpectObjectName(COLUMN);
            var columnName = jsonParser.ExpectToken(JsonTokenKind.String).Text;
            var column = model.Columns[columnName];
            jsonParser.ExpectToken(JsonTokenKind.Comma);
            var direction = jsonParser.ParseDirection();
            jsonParser.ExpectToken(JsonTokenKind.CurlyClose);
            return new ColumnSort(column, direction);
        }

        private static SortDirection ParseDirection(this JsonParser jsonParser)
        {
            var value = jsonParser.ExpectNameStringPair(DIRECTION, false);
            return (SortDirection)Enum.Parse(typeof(SortDirection), value);
        }
    }
}
