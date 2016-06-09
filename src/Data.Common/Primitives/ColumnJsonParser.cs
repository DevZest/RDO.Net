using System;
using System.Collections.Generic;

namespace DevZest.Data.Primitives
{
    internal class ColumnJsonParser : JsonParser
    {
        internal const string TYPE_ID = "TypeId";
        internal const string TYPE_ARG_ID = "TypeArgId";
        internal const string NAME = "Name";
        internal const string EXPRESSION = "Expression";

        internal ColumnJsonParser(string json)
            : base(json)
        {
        }

        internal Column ParseTopLevelColumn(Model model)
        {
            var result = ParseColumn(model);
            ExpectToken(TokenKind.Eof);
            return result;
        }

        internal Column ParseColumn(Model model)
        {
            Column result;

            var converter = ExpectColumnConverter();
  
            var objectName = ExpectToken(TokenKind.String).Text;
            ExpectToken(TokenKind.Colon);
            if (objectName == NAME)
            {
                var name = ExpectToken(TokenKind.String).Text;
                result = model[name] as Column;
                if (result == null || result.GetType() != converter.ColumnType)
                    throw new FormatException(Strings.ColumnJsonParser_InvalidColumnType(name, converter.ColumnType.FullName));
            }
            else if (objectName == EXPRESSION)
                result = converter.ParseJson(model, this);
            else
                throw new FormatException(Strings.ColumnJsonParser_InvalidObjectName(objectName, NAME, EXPRESSION));

            ExpectToken(TokenKind.CurlyClose);
            return result;
        }

        private ColumnConverter ExpectColumnConverter()
        {
            ExpectToken(TokenKind.CurlyOpen);

            var typeId = ExpectString(TYPE_ID, true);

            string typeArgId;
            var currentToken = PeekToken();
            if (currentToken.Kind == TokenKind.String && currentToken.Text == TYPE_ARG_ID)
                typeArgId = ExpectString(TYPE_ARG_ID, true);
            else
                typeArgId = null;

            var result = ColumnConverter.Get(typeId, typeArgId);
            if (result == null)
                throw new FormatException(Strings.ColumnJsonParser_InvalidTypeId(typeId));
            return result;
        }

        internal T ParseNameColumnPair<T>(string name, Model model)
            where T : Column
        {
            ExpectObjectName(name);
            return (T)ParseColumn(model);
        }

        internal IReadOnlyList<T> ParseNameColumnArrayPair<T>(string name, Model model)
            where T : Column
        {
            var result = new List<T>();

            ExpectObjectName(name);
            ExpectToken(TokenKind.SquaredOpen);

            while (PeekToken().Kind == TokenKind.CurlyOpen)
            {
                result.Add((T)ParseColumn(model));

                while (PeekToken().Kind == TokenKind.Comma)
                {
                    ConsumeToken();
                    result.Add((T)ParseColumn(model));
                }
            }

            ExpectToken(TokenKind.SquaredClose);
            return result;
        }

        internal T ParseNameValuePair<T>(string objectName, Column<T> deserializer)
        {
            ExpectObjectName(objectName);
            return deserializer.DeserializeValue(ExpectToken(TokenKind.ColumnValues).JsonValue);
        }
    }
}
