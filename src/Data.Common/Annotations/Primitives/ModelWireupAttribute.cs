using System;
using System.Diagnostics;
using System.Reflection;

namespace DevZest.Data.Annotations.Primitives
{
    public abstract class ModelWireupAttribute : Attribute
    {
        internal void Initialize(Type modelType, MemberInfo memberInfo)
        {
            Debug.Assert(ModelType == null && modelType != null);
            ModelType = modelType;
        }

        public Type ModelType { get; private set; }

        protected abstract void PerformInitialize(Type modelType, MemberInfo memberInfo);

        protected internal abstract ModelWireupEvent WireupEvent { get; }

        protected internal abstract Action<Model> WireupAction { get; }
    }
}
