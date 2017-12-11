using System;
using System.Reflection;

namespace DevZest.Data.Annotations.Primitives
{
    public abstract class ModelWireupAttribute : Attribute
    {
        protected internal abstract void Initialize(Type modelType, MemberInfo memberInfo);

        protected internal abstract ModelWireupEvent WireupEvent { get; }

        protected internal abstract Action<Model> WireupAction { get; }
    }
}
