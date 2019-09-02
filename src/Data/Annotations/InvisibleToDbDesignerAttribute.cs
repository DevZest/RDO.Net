using System;

namespace DevZest.Data.Annotations
{
    /// <summary>
    /// Specifies given class is ignored by database designer.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class InvisibleToDbDesignerAttribute : Attribute
    {
    }
}
