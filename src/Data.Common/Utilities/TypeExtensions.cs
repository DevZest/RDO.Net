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

        internal static Func<string> ResolveStringGetter(this Type resourceType, string resourceName)
        {
            Check.NotNull(resourceType, nameof(resourceType));
            if (string.IsNullOrWhiteSpace(resourceName))
                throw new ArgumentException(DiagnosticMessages.TypeExtensions_InvalidResourceName, nameof(resourceName));

            try
            {
                return BuildStringGetter(resourceType, resourceName);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(DiagnosticMessages.TypeExtensions_CannotResolveStaticStringProperty(resourceType, resourceName), ex);
            }
        }

        private static Func<string> BuildStringGetter(Type resourceType, string resourceName)
        {
            Debug.Assert(resourceType != null);
            Debug.Assert(!string.IsNullOrWhiteSpace(resourceName));

            PropertyInfo property = resourceType.GetProperty(resourceName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            var methodInfo = property.GetGetMethod(true);
            var call = Expression.Call(methodInfo);
            return Expression.Lambda<Func<string>>(call).Compile();
        }
    }
}
