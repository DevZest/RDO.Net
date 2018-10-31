using DevZest.Data.Annotations.Primitives;
using DevZest.Data.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DevZest.Data
{
    internal static class ColumnManager
    {
        internal static Action<TColumn> Verify<TParent, TColumn>(this Expression<Func<TParent, TColumn>> getter, string paramName)
            where TColumn : Column, new()
        {
            getter.VerifyNotNull(paramName);
            if (!(getter.Body is MemberExpression memberExpr))
                throw new ArgumentException(DiagnosticMessages.InvalidGetterExpression, paramName);
            var columnName = memberExpr.Member.Name;

            var columnAttributes = GetColumnAttributes<TParent>(columnName, paramName);
            return x => Initialize(x, columnAttributes);
        }

        private static IEnumerable<ColumnAttribute> GetColumnAttributes<T>(string columnName, string paramName)
        {
            var propertyInfo = typeof(T).GetProperty(columnName, BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (propertyInfo == null)
                throw new ArgumentException(DiagnosticMessages.InvalidGetterExpression, paramName);

            var result = propertyInfo.GetCustomAttributes<ColumnAttribute>().ToArray();
            foreach (var columnAttribute in result)
                columnAttribute.DeclaringType = propertyInfo.DeclaringType;
            return result;
        }


        private static MethodInfo[] GetStaticMethods<T>()
        {
            return typeof(T).GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        }

        private static void Initialize(Column column, IEnumerable<ColumnAttribute> columnAttributes)
        {
            if (columnAttributes == null)
                return;
            foreach (var columnAttribute in columnAttributes)
                columnAttribute.TryWireup(column);
        }

        internal static Action<Column<T>> Verify<TParent, T>(this Expression<Func<TParent, Column<T>>> getter, string paramName)
        {
            getter.VerifyNotNull(paramName);
            if (!(getter.Body is MemberExpression memberExpr))
                throw new ArgumentException(DiagnosticMessages.InvalidGetterExpression, paramName);
            var columnName = memberExpr.Member.Name;

            var columnAttributes = GetColumnAttributes<TParent>(columnName, paramName);
            return x => Initialize(x, columnAttributes);
        }

        internal static bool ContainsSource(this IReadOnlyList<ColumnMapping> columnMappings, Column source)
        {
            foreach (var mapping in columnMappings)
            {
                if (mapping.Source == source)
                    return true;
            }
            return false;
        }

        internal static List<T> TranslateToColumns<T>(this List<T> columns, Model model)
            where T : Column
        {
            if (columns == null)
                return null;

            List<T> result = null;
            for (int i = 0; i < columns.Count; i++)
            {
                var column = columns[i];
                var translated = column.TranslateTo(model);
                if (result != null)
                    result.Add(translated);
                else if (translated != column)
                {
                    if (result == null)
                    {
                        result = new List<T>();
                        for (int j = 0; j < i; j++)
                            result.Add(columns[j]);
                    }
                    result.Add(translated);
                }
            }
            return result ?? columns;
        }

        internal static T[] TranslateToParams<T>(this IReadOnlyList<T> parameters, Model model)
            where T : Column
        {
            if (parameters == null)
                return null;

            T[] result = null;
            for (int i = 0; i < parameters.Count; i++)
            {
                var column = parameters[i];
                var translated = column.TranslateTo(model);
                if (result != null)
                    result[i] = translated;
                else if (translated != column)
                {
                    if (result == null)
                    {
                        result = new T[parameters.Count];
                        for (int j = 0; j < i; j++)
                            result[j] = parameters[j];
                    }
                    result[i] = translated;
                }
            }
            return result;
        }

        internal static Type ResolveColumnDataType(this Type columnType, bool bypassNullable)
        {
            for (var type = columnType; type != null; type = type.GetTypeInfo().BaseType)
            {
                if (type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(Column<>))
                {
                    var typeParam = type.GetGenericArguments()[0];
                    if (!bypassNullable)
                        return typeParam;

                    if (typeParam.GetTypeInfo().IsGenericType && typeParam.GetGenericTypeDefinition() == typeof(Nullable<>))
                        return typeParam.GetGenericArguments()[0];
                    else
                        return null;
                }
            }
            return null;
        }
    }
}
