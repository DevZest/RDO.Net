using System;

namespace DevZest.Data.Primitives
{
    public sealed class FunctionKey
    {
        public FunctionKey(Type declaringType, string name)
        {
            DeclaringType = declaringType;
            Name = name;
        }

        public Type DeclaringType { get; private set; }

        public string Name { get; private set; }

        public override string ToString()
        {
            return string.Format("{0}.{1}", DeclaringType, Name);
        }
    }
}
