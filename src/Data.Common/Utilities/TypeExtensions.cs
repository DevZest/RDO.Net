﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace DevZest.Data.Utilities
{
    public static class TypeExtensions
    {
        internal static bool IsComparable(this Type type)
        {
            return typeof(IComparable).IsAssignableFrom(type);
        }

        public static Type ResolveColumnDataType(this Type columnType, bool bypassNullable = false)
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

        public static bool IsDerivedFrom(this Type type, Type genericTypeDefinition)
        {
            for (; type != null; type = type.GetTypeInfo().BaseType)
            {
                if (type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == genericTypeDefinition)
                    return true;
            }
            return false;
        }

        public static MethodInfo GetStaticMethodInfo(this Type type, string methodName)
        {
            return type.GetMethod(methodName, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        }

        internal static Func<string, IReadOnlyList<Column>, DataRow, string> GetColumnsMessageFunc(this Type funcType, string funcName)
        {
            Debug.Assert(funcType != null);
            Debug.Assert(!string.IsNullOrWhiteSpace(funcName));

            var methodInfo = funcType.GetStaticMethodInfo(funcName);
            var paramAttributeName = Expression.Parameter(typeof(string), methodInfo.GetParameters()[0].Name);
            var paramColumns = Expression.Parameter(typeof(IReadOnlyList<Column>), methodInfo.GetParameters()[1].Name);
            var paramDataRow = Expression.Parameter(typeof(DataRow), methodInfo.GetParameters()[2].Name);
            var call = Expression.Call(methodInfo, paramAttributeName, paramColumns, paramDataRow);
            return Expression.Lambda<Func<string, IReadOnlyList<Column>, DataRow, string>>(call, paramAttributeName, paramColumns, paramDataRow).Compile();
        }

        internal static bool IsNullable(this Type type)
        {
            return !type.GetTypeInfo().IsValueType || (Nullable.GetUnderlyingType(type) != null);
        }
    }
}
