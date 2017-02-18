using System;

namespace DevZest
{
    internal static partial class ObjectExtensions
    {
        internal static string ToString(this object value, string format, IFormatProvider formatProvider)
        {
            if (string.IsNullOrEmpty(format))
                return value.ToString();
            else if (formatProvider == null)
                return string.Format(format, value);
            else
                return string.Format(formatProvider, format, value);
        }
    }
}
