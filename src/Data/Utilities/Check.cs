using System;
using System.Collections.Generic;

namespace DevZest.Data.Utilities
{
    internal static class Check
    {
        internal static string NotEmpty(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(DiagnosticMessages.ArgumentIsNullOrWhitespace(parameterName), parameterName);

            return value;
        }

        internal static T CheckNotNull<T>(this T reference, string paramName, int index)
            where T : class
        {
            if (reference == null)
                throw new ArgumentNullException(String.Format("{0}[{1}]", paramName, index));
            return reference;
        }

        internal static IReadOnlyList<T> NotEmpty<T>(IReadOnlyList<T> list, string parameterName)
        {
            list.VerifyNotNull(parameterName);
            if (list.Count == 0)
                throw new ArgumentException(DiagnosticMessages.ArgumentIsNullOrWhitespace(parameterName), parameterName);

            return list;
        }
    }
}
