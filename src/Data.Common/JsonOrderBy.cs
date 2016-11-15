using DevZest.Data.Primitives;
using System;
using System.Collections.Generic;

namespace DevZest.Data
{
    public static class JsonOrderBy
    {
        public static string ToJson(this IEnumerable<ColumnSort> orderBy, bool isPretty)
        {
            if (orderBy == null)
                orderBy = Array<ColumnSort>.Empty;
            return JsonWriter.New().Write(orderBy).ToString(isPretty);
        }

        public static IReadOnlyList<ColumnSort> ParseOrderBy(this Model model, string jsonString)
        {
            var jsonParser = new JsonParser(jsonString);
            var result = jsonParser.ParseOrderBy(model);
            jsonParser.ExpectToken(JsonTokenKind.Eof);
            return result;
        }
    }
}
