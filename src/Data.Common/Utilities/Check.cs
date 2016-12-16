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
                throw new ArgumentException(Strings.ArgumentIsNullOrWhitespace(parameterName), parameterName);

            return value;
        }

        internal static T CheckNotNull<T>(this T reference)
            where T : class
        {
            if (reference == null)
                throw new NullReferenceException();
            return reference;
        }
    }
}
