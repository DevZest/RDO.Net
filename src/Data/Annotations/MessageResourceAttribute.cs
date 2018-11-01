using System;

namespace DevZest.Data.Annotations
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public sealed class MessageResourceAttribute : Attribute
    {
        public MessageResourceAttribute(Type type)
        {
            Types = type;
        }

        public Type Types { get; }
    }
}
