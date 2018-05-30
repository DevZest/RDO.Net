using System;

namespace DevZest.Data.Annotations.Primitives
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple =true)]
    public abstract class ColumnGroupAttribute : Attribute
    {
        protected ColumnGroupAttribute(string name)
        {
            Name = name.VerifyNotEmpty(nameof(name));
        }

        public string Name { get; private set; }
    }
}
