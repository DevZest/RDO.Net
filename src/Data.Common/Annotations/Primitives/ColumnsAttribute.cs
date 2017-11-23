using DevZest.Data.Utilities;
using System;

namespace DevZest.Data.Annotations.Primitives
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple =true)]
    public abstract class ColumnsAttribute : Attribute
    {
        protected ColumnsAttribute(string name)
        {
            Check.NotEmpty(name, nameof(name));
            Name = name;
        }

        public string Name { get; private set; }
    }
}
