using System;

namespace DevZest
{
    internal static partial class Check
    {
        internal static T CheckNotNull<T>(this T reference, string paramName, int index)
            where T : class
        {
            if (reference == null)
                throw new ArgumentNullException(String.Format("{0}[{1}]", paramName, index));
            return reference;
        }
    }
}
