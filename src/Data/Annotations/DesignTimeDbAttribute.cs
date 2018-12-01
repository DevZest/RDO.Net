using System;

namespace DevZest.Data.Annotations
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class DesignTimeDbAttribute : Attribute
    {
        public DesignTimeDbAttribute(bool isClean)
        {
            IsClean = isClean;
        }

        public bool IsClean { get; }
    }
}
