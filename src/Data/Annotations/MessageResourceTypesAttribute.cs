using System;

namespace DevZest.Data.Annotations
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public sealed class MessageResourceTypesAttribute : Attribute
    {
        public MessageResourceTypesAttribute(params Type[] types)
        {
            Types = types;
        }

        public Type[] Types { get; }
    }
}
