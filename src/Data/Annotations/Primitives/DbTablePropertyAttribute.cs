using System;

namespace DevZest.Data.Annotations.Primitives
{
    [AttributeUsage(AttributeTargets.Property)]
    public abstract class DbTablePropertyAttribute : Attribute
    {
        protected DbTablePropertyAttribute()
        {
        }

        protected internal abstract void Wireup(Model model);
    }
}
