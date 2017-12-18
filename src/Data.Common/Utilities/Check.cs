using System;

namespace DevZest.Data.Utilities
{
    internal static class Check
    {
        internal static T NotNull<T>(T value, string parameterName)
            where T : class
        {
            if (value == null)
                throw new ArgumentNullException(parameterName);

            return value;
        }

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
    }
}
