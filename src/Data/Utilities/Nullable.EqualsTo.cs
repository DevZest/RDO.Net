using System.Collections.Generic;

namespace DevZest
{
    internal static partial class Extensions
    {
        internal static bool? EqualsTo<T>(this T? value, T? other)
            where T : struct
        {
            if (value.HasValue && other.HasValue)
                return EqualityComparer<T>.Default.Equals(value.Value, other.Value);
            return null;
        }
    }
}
