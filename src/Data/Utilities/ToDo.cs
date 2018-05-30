using DevZest.Data;
using System;

namespace DevZest
{
    internal static partial class Extensions
    {
        private static class ToDo
        {
            public static string ArgumentIsNullOrWhitespace(string parameterName)
            {
                return DiagnosticMessages.ArgumentIsNullOrWhitespace(parameterName);
            }

            public static string ArgumentIsNullOrEmptyList(object parameterName)
            {
                return DiagnosticMessages.ArgumentIsNullOrEmptyList(parameterName);
            }

            public static string CannotResolveStaticProperty(Type type, string propertyName, Type propertyType)
            {
                return DiagnosticMessages.CannotResolveStaticProperty(type, propertyName, propertyType);
            }
        }
    }
}
