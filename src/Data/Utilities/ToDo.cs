using DevZest.Data;

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
        }
    }
}
