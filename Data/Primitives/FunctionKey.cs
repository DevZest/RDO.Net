using System;

namespace DevZest.Data.Primitives
{
    public sealed class FunctionKey
    {
        public FunctionKey(Type ownerType, string name)
        {
            OwnerType = ownerType;
            Name = name;
        }

        public Type OwnerType { get; private set; }

        public string Name { get; private set; }

        public override string ToString()
        {
            return string.Format("{0}.{1}", OwnerType, Name);
        }
    }
}
