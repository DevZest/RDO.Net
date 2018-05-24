using System;
using System.Collections.Generic;

namespace DevZest.Data.Utilities
{
    internal static class NullableExtensions
    {
        public static bool? EqualsTo<T>(this Nullable<T> value, Nullable<T> other)
            where T : struct
        {
            if (value.HasValue && other.HasValue)
                return EqualityComparer<T>.Default.Equals(value.Value, other.Value);
            return null;
        }
    }
}
