using System;
using System.Diagnostics;

namespace DevZest.Data.Presenters
{
    public sealed class AsyncValidationFault : ValidationError<AsyncValidator>
    {
        internal AsyncValidationFault(AsyncValidator source, Func<AsyncValidator, string> formatMessage)
            : base(FormatMessage(source, formatMessage), source)
        {
            source.VerifyNotNull(nameof(source));
        }

        private static string FormatMessage(AsyncValidator source, Func<AsyncValidator, string> formatMessage)
        {
            Debug.Assert(source != null && source.Exception != null);
            return formatMessage != null ? formatMessage(source) : FormatMessage(source);
        }

        internal static string FormatMessage(AsyncValidator source)
        {
            return UserMessages.AsyncValidationFault_FormatMessage(source.DisplayName, source.Exception.Message);
        }
    }
}
