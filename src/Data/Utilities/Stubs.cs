using DevZest.Data;
using System;

namespace DevZest
{
    internal static partial class Extensions
    {
        private static class Stubs
        {
            public static string ArgumentIsNullOrWhitespace(string parameterName)
            {
                return DiagnosticMessages.Common_ArgumentIsNullOrWhitespace(parameterName);
            }

            public static string ArgumentIsNullOrEmptyList(object parameterName)
            {
                return DiagnosticMessages.Common_ArgumentIsNullOrEmptyList(parameterName);
            }

            public static string FailedToResolveStaticProperty(Type type, string propertyName, Type propertyType)
            {
                return DiagnosticMessages.Common_FailedToResolveStaticProperty(type, propertyName, propertyType);
            }
        }
    }
}
