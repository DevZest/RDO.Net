using System;
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

        internal static Func<Column, _String> GetMessageFunc(this Type funcType, string funcName)
        {
            Debug.Assert(funcType != null);
            Debug.Assert(!string.IsNullOrWhiteSpace(funcName));

            var methodInfo = funcType.GetStaticMethodInfo(funcName);
            var paramColumn = Expression.Parameter(typeof(Column), methodInfo.GetParameters()[0].Name);
            var call = Expression.Call(methodInfo, paramColumn);
            return Expression.Lambda<Func<Column, _String>>(call, paramColumn).Compile();
        }
    }
}
