using System;

namespace DevZest.Data.Presenters
{
    public sealed class AsyncValidationFault : ValidationError<AsyncValidator>
    {
        public AsyncValidationFault(AsyncValidator source, Exception exception, Func<AsyncValidator, Exception, string> formatMessage = null)
            : base(formatMessage == null ? FormatMessage(source, exception) : formatMessage(source, exception), source)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(exception, nameof(exception));

            _exception = exception;
        }

        private readonly Exception _exception;
        public Exception Exception
        {
            get { return _exception; }
        }

        private static string FormatMessage(AsyncValidator source, Exception exception)
        {
            throw new NotImplementedException();
        }
    }
}
