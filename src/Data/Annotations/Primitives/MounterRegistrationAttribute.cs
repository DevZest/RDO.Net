using System;

namespace DevZest.Data.Annotations.Primitives
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    internal class MounterRegistrationAttribute : Attribute
    {
        internal MounterRegistrationAttribute()
        {
        }
    }
}
