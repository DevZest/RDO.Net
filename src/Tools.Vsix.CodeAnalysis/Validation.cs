using System.Runtime.CompilerServices;

namespace DevZest.Data.CodeAnalysis
{
    public static class Validation
    {
        private static readonly object s_dummy = new object();
        private static readonly ConditionalWeakTable<ValidationError, object> s_warnings = new ConditionalWeakTable<ValidationError, object>();

        public static T AsWarning<T>(this T validationError)
            where T : ValidationError
        {
            if (validationError.GetErrorLevel() != ErrorLevel.Warning)
                s_warnings.Add(validationError, s_dummy);
            return validationError;
        }

        public static ErrorLevel GetErrorLevel(this ValidationError validationError)
        {
            return s_warnings.TryGetValue(validationError, out _) ? ErrorLevel.Warning : ErrorLevel.Error;
        }
    }
}
