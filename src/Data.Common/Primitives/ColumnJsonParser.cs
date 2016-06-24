using System;
using System.Collections.Generic;

namespace DevZest.Data.Primitives
{
    internal class ColumnJsonParser : JsonParser
    {
        internal const string TYPE_ID = "TypeId";
        internal const string ARG_TYPE_IDS = "ArgTypeIds";
        internal const string NAME = "Name";
        internal const string EXPRESSION = "Expression";

        internal ColumnJsonParser(string json)
            : base(json)
        {
        }

        internal void ExpectComma()
        {
            ExpectToken(TokenKind.Comma);
        }

        internal T ParseTopLevelColumn<T>(Model model)
            where T : Column
        {
            var result = ParseColumn<T>(model);
            ExpectToken(TokenKind.Eof);
            return result;
        }

        internal T ParseColumn<T>(Model model, bool allowsNull = false)
            where T : Column
        {
            if (allowsNull && PeekToken().Kind == TokenKind.Null)
            {
                ConsumeToken();
                return null;
            }

            T result;

            var converter = ExpectColumnConverter();
  
            var objectName = ExpectToken(TokenKind.String).Text;
            ExpectToken(TokenKind.Colon);
            if (objectName == NAME)
            {
                var name = ExpectToken(TokenKind.String).Text;
                result = model[name] as T;
                if (result == null || result.GetType() != converter.ColumnType)
                    throw new FormatException(Strings.ColumnJsonParser_InvalidColumnType(name, converter.ColumnType.FullName));
            }
            else if (objectName == EXPRESSION)
            {
                var expression = ParseExpression(model);
                result = (T)converter.MakeColumn(expression);
            }
            else
                throw new FormatException(Strings.ColumnJsonParser_InvalidObjectName(objectName, NAME, EXPRESSION));

            ExpectToken(TokenKind.CurlyClose);
            return result;
        }

        private ColumnConverter ExpectColumnConverter()
        {
            ExpectToken(TokenKind.CurlyOpen);

            var typeId = ExpectString(TYPE_ID, true);
            var result = ColumnConverter.Get(typeId);
            if (result == null)
                throw new FormatException(Strings.ColumnJsonParser_InvalidTypeId(typeId));
            return result;
        }

        internal ColumnExpression ParseExpression(Model model)
        {
            ExpectToken(TokenKind.CurlyOpen);

            var typeId = ExpectString(TYPE_ID, true);
            var argColumnTypeIds = ExpectArgTypeIds();
            var converter = ExpressionConverter.Get(typeId, argColumnTypeIds);
            var result = converter.ParseJson(model, this);
            ExpectToken(TokenKind.CurlyClose);
            return result;
        }

        private IReadOnlyList<string> ExpectArgTypeIds()
        {
            var result = new List<string>();

            ExpectObjectName(ARG_TYPE_IDS);
            ExpectToken(TokenKind.SquaredOpen);
            if (PeekToken().Kind == TokenKind.String)
            {
                result.Add(ExpectToken(TokenKind.String).Text);

                while (PeekToken().Kind == TokenKind.Comma)
                {
                    ConsumeToken();
                    result.Add(ExpectToken(TokenKind.String).Text);
                }
            }
            ExpectToken(TokenKind.SquaredClose);
            ExpectToken(TokenKind.Comma);
            return result;
        }

        internal T ParseNameColumnPair<T>(string name, Model model, bool allowsNull = false)
            where T : Column
        {
            ExpectObjectName(name);
            return ParseColumn<T>(model, allowsNull);
        }

        internal List<T> ParseNameColumnsPair<T>(string name, Model model)
            where T : Column
        {
            var result = new List<T>();

            ExpectObjectName(name);
            ExpectToken(TokenKind.SquaredOpen);

            while (PeekToken().Kind == TokenKind.CurlyOpen)
            {
                result.Add(ParseColumn<T>(model));

                while (PeekToken().Kind == TokenKind.Comma)
                {
                    ConsumeToken();
                    result.Add(ParseColumn<T>(model));
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
