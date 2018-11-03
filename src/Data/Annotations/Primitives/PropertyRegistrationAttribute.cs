using System;

namespace DevZest.Data.Annotations.Primitives
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    internal class PropertyRegistrationAttribute : Attribute
    {
    }
}
