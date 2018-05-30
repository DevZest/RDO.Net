using System;
using System.Reflection;

namespace DevZest
{
    public static class TypeExtensions
    {
        internal static bool IsComparable(this Type type)
        {
            return typeof(IComparable).IsAssignableFrom(type);
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
