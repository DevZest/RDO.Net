using System;

namespace DevZest.Data.Utilities
{
    internal class Check
    {
        public static T NotNull<T>(T value, string parameterName) where T : class
        {
            if (value == null)
                throw new ArgumentNullException(parameterName);

            return value;
        }

        public static string NotEmpty(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(Strings.ArgumentIsNullOrWhitespace(parameterName), parameterName);

            return value;
        }
    }
}
