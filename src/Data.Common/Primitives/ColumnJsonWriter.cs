using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace DevZest.Data.Primitives
{
    internal static class ColumnJsonWriter
    {
        internal static string GetDefaultTypeId(this Type targetType)
        {
            var fullName = targetType.FullName;
            var nspace = targetType.Namespace;
            return string.IsNullOrEmpty(nspace) ? fullName : fullName.Substring(nspace.Length + 1);
        }


        internal static StringBuilder WriteColumn(this StringBuilder stringBuilder, Column column)
        {
            var converter = ColumnConverter.Get(column);
            converter.WriteJson(stringBuilder, column);
            return stringBuilder;
        }

        internal static StringBuilder WriteNameColumnPair<T>(this StringBuilder stringBuilder, string name, Column<T> column)
        {
            stringBuilder.WriteObjectName(name);
            column.WriteJson(stringBuilder);
            return stringBuilder;
        }

        internal static StringBuilder WriteExpression<T>(this StringBuilder stringBuilder, ColumnExpression<T> expression)
        {
            stringBuilder.WriteStartObject();
            var converter = ExpressionConverter.Get(expression);
            converter.WriteJson(stringBuilder, expression);
            stringBuilder.WriteEndObject();
            return stringBuilder;
        }

        internal static StringBuilder WriteNameColumnsPair<T>(this StringBuilder stringBuilder, string name, IReadOnlyList<T> columns)
            where T : Column
        {
            return stringBuilder.WriteObjectName(name).WriteStartArray().WriteColumns(columns).WriteEndArray();
        }

        private static StringBuilder WriteColumns<T>(this StringBuilder stringBuilder, IReadOnlyList<T> columns)
            where T : Column
        {
            for (int i = 0; i < columns.Count; i++)
            {
                stringBuilder.WriteColumn(columns[i]);
                if (i < columns.Count - 1)
                    stringBuilder.WriteComma();
            }
            return stringBuilder;
        }

        internal static StringBuilder WriteExpressionTypeInfo<T>(this StringBuilder stringBuilder, ColumnExpression<T> expression)
        {
            stringBuilder.WriteNameStringPair(ColumnJsonParser.TYPE_ID, ExpressionConverter.GetTypeId(expression)).WriteComma()
                .WriteObjectName(ColumnJsonParser.ARG_TYPE_IDS).WriteArgColumnTypeIds(expression).WriteComma();
            return stringBuilder;
        }

        private static StringBuilder WriteArgColumnTypeIds<T>(this StringBuilder stringBuilder, ColumnExpression<T> expression)
        {
            stringBuilder.WriteStartArray();

            var argColumnTypes = expression.ArgColumnTypes;
            if (argColumnTypes == null)
                argColumnTypes = Array<Type>.Empty;
            var typeArgs = GetTypeArgs(ExpressionConverter.GetConverterType(expression));
            Debug.Assert(argColumnTypes.Length == typeArgs.Length);

            for (int i = 0; i < argColumnTypes.Length; i++)
            {
                var typeId = JsonValue.FastString(ColumnConverter.GetTypeId(argColumnTypes[i]));
                stringBuilder.WriteValue(typeId);
                if (i < argColumnTypes.Length - 1)
                    stringBuilder.WriteComma();
            }

            stringBuilder.WriteEndArray();
            return stringBuilder;
        }

        private static Type[] GetTypeArgs(Type converterType)
        {
            return converterType.GetTypeInfo().IsGenericType ? converterType.GetGenericTypeDefinition().GetGenericArguments() : Array<Type>.Empty;
        }
    }
}
