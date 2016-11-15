using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace DevZest.Data.Primitives
{
    public static class JsonColumn
    {
        internal const string TYPE_ID = "TypeId";
        private const string ARG_TYPE_IDS = "ArgTypeIds";
        internal const string NAME = "Name";
        internal const string EXPRESSION = "Expression";

        internal static string GetDefaultTypeId(this Type targetType)
        {
            var fullName = targetType.FullName;
            var nspace = targetType.Namespace;
            return string.IsNullOrEmpty(nspace) ? fullName : fullName.Substring(nspace.Length + 1);
        }


        public static JsonWriter Write(this JsonWriter jsonWriter, Column column)
        {
            var converter = ColumnConverter.Get(column);
            converter.WriteJson(jsonWriter, column);
            return jsonWriter;
        }

        public static JsonWriter WriteNameColumnPair(this JsonWriter jsonWriter, string name, Column column)
        {
            jsonWriter.WriteObjectName(name).Write(column);
            return jsonWriter;
        }

        internal static JsonWriter WriteExpression<T>(this JsonWriter jsonWriter, ColumnExpression<T> expression)
        {
            jsonWriter.WriteStartObject();
            jsonWriter.WriteExpressionTypeInfo(expression);
            var converter = ExpressionConverter.Get(expression);
            converter.WriteJson(jsonWriter, expression);
            jsonWriter.WriteEndObject();
            return jsonWriter;
        }

        public static JsonWriter WriteNameColumnsPair<T>(this JsonWriter jsonWriter, string name, IReadOnlyList<T> columns)
            where T : Column
        {
            return jsonWriter.WriteObjectName(name).WriteStartArray().WriteColumns(columns).WriteEndArray();
        }

        private static JsonWriter WriteColumns<T>(this JsonWriter jsonWriter, IReadOnlyList<T> columns)
            where T : Column
        {
            for (int i = 0; i < columns.Count; i++)
            {
                jsonWriter.Write(columns[i]);
                if (i < columns.Count - 1)
                    jsonWriter.WriteComma();
            }
            return jsonWriter;
        }

        internal static JsonWriter WriteExpressionTypeInfo<T>(this JsonWriter jsonWriter, ColumnExpression<T> expression)
        {
            return jsonWriter.WriteNameStringPair(TYPE_ID, ExpressionConverter.GetTypeId(expression)).WriteComma()
                .WriteObjectName(ARG_TYPE_IDS).WriteArgColumnTypeIds(expression).WriteComma();
        }

        private static JsonWriter WriteArgColumnTypeIds<T>(this JsonWriter jsonWriter, ColumnExpression<T> expression)
        {
            jsonWriter.WriteStartArray();

            var argColumnTypes = expression.ArgColumnTypes;
            if (argColumnTypes == null)
                argColumnTypes = Array<Type>.Empty;
            var typeArgs = GetTypeArgs(ExpressionConverter.GetConverterType(expression));
            Debug.Assert(argColumnTypes.Length == typeArgs.Length);

            for (int i = 0; i < argColumnTypes.Length; i++)
            {
                var typeId = JsonValue.FastString(ColumnConverter.GetTypeId(argColumnTypes[i]));
                jsonWriter.WriteValue(typeId);
                if (i < argColumnTypes.Length - 1)
                    jsonWriter.WriteComma();
            }

            jsonWriter.WriteEndArray();
            return jsonWriter;
        }

        private static Type[] GetTypeArgs(Type converterType)
        {
            return converterType.GetTypeInfo().IsGenericType ? converterType.GetGenericTypeDefinition().GetGenericArguments() : Array<Type>.Empty;
        }

        private static void ExpectComma(this JsonParser jsonParser)
        {
            jsonParser.ExpectToken(JsonTokenKind.Comma);
        }

        public static T ParseColumn<T>(this JsonParser jsonParser, Model model, bool allowsNull = false)
            where T : Column
        {
            if (allowsNull && jsonParser.PeekToken().Kind == JsonTokenKind.Null)
            {
                jsonParser.ConsumeToken();
                return null;
            }

            T result;

            var converter = jsonParser.ExpectColumnConverter();

            var objectName = jsonParser.ExpectToken(JsonTokenKind.String).Text;
            jsonParser.ExpectToken(JsonTokenKind.Colon);
            if (objectName == NAME)
            {
                var name = jsonParser.ExpectToken(JsonTokenKind.String).Text;
                result = model[name] as T;
                if (result == null || result.GetType() != converter.ColumnType)
                    throw new FormatException(Strings.ColumnJsonParser_InvalidColumnType(name, converter.ColumnType.FullName));
            }
            else if (objectName == EXPRESSION)
            {
                var expression = jsonParser.ParseExpression(model);
                result = (T)converter.MakeColumn(expression);
            }
            else
                throw new FormatException(Strings.ColumnJsonParser_InvalidObjectName(objectName, NAME, EXPRESSION));

            jsonParser.ExpectToken(JsonTokenKind.CurlyClose);
            return result;
        }

        private static ColumnConverter ExpectColumnConverter(this JsonParser jsonParser)
        {
            jsonParser.ExpectToken(JsonTokenKind.CurlyOpen);

            var typeId = jsonParser.ExpectNameStringPair(TYPE_ID, true);
            var result = ColumnConverter.Get(typeId);
            if (result == null)
                throw new FormatException(Strings.ColumnJsonParser_InvalidTypeId(typeId));
            return result;
        }

        private static ColumnExpression ParseExpression(this JsonParser jsonParser, Model model)
        {
            jsonParser.ExpectToken(JsonTokenKind.CurlyOpen);

            var typeId = jsonParser.ExpectNameStringPair(TYPE_ID, true);
            var argColumnTypeIds = jsonParser.ExpectArgTypeIds();
            var converter = ExpressionConverter.Get(typeId, argColumnTypeIds);
            var result = converter.ParseJson(jsonParser, model);
            jsonParser.ExpectToken(JsonTokenKind.CurlyClose);
            return result;
        }

        private static IReadOnlyList<string> ExpectArgTypeIds(this JsonParser jsonParser)
        {
            var result = new List<string>();

            jsonParser.ExpectObjectName(ARG_TYPE_IDS);
            jsonParser.ExpectToken(JsonTokenKind.SquaredOpen);
            if (jsonParser.PeekToken().Kind == JsonTokenKind.String)
            {
                result.Add(jsonParser.ExpectToken(JsonTokenKind.String).Text);

                while (jsonParser.PeekToken().Kind == JsonTokenKind.Comma)
                {
                    jsonParser.ConsumeToken();
                    result.Add(jsonParser.ExpectToken(JsonTokenKind.String).Text);
                }
            }
            jsonParser.ExpectToken(JsonTokenKind.SquaredClose);
            jsonParser.ExpectToken(JsonTokenKind.Comma);
            return result;
        }

        internal static T ParseNameColumnPair<T>(this JsonParser jsonParser, string name, Model model, bool allowsNull = false)
            where T : Column
        {
            jsonParser.ExpectObjectName(name);
            return jsonParser.ParseColumn<T>(model, allowsNull);
        }

        internal static List<T> ParseNameColumnsPair<T>(this JsonParser jsonParser, string name, Model model)
            where T : Column
        {
            var result = new List<T>();

            jsonParser.ExpectObjectName(name);
            jsonParser.ExpectToken(JsonTokenKind.SquaredOpen);

            while (jsonParser.PeekToken().Kind == JsonTokenKind.CurlyOpen)
            {
                result.Add(jsonParser.ParseColumn<T>(model));

                while (jsonParser.PeekToken().Kind == JsonTokenKind.Comma)
                {
                    jsonParser.ConsumeToken();
                    result.Add(jsonParser.ParseColumn<T>(model));
                }
            }

            jsonParser.ExpectToken(JsonTokenKind.SquaredClose);
            return result;
        }

        internal static T ParseNameValuePair<T>(this JsonParser jsonParser, string objectName, Column<T> deserializer)
        {
            jsonParser.ExpectObjectName(objectName);
            return deserializer.DeserializeValue(jsonParser.ExpectToken(JsonTokenKind.ColumnValues).JsonValue);
        }
    }
}
