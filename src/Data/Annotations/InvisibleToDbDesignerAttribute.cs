using System;

namespace DevZest.Data.Annotations
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class InvisibleToDbDesignerAttribute : Attribute
    {
    }
}
