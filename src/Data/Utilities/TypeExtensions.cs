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

        internal static bool IsNullable(this Type type)
        {
            return !type.GetTypeInfo().IsValueType || (Nullable.GetUnderlyingType(type) != null);
        }
    }
}
