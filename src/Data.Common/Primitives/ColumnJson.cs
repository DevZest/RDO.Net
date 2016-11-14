using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace DevZest.Data.Primitives
{
    public static class ColumnJson
    {
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
            return jsonWriter.WriteNameStringPair(ColumnJsonParser.TYPE_ID, ExpressionConverter.GetTypeId(expression)).WriteComma()
                .WriteObjectName(ColumnJsonParser.ARG_TYPE_IDS).WriteArgColumnTypeIds(expression).WriteComma();
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
    }
}
