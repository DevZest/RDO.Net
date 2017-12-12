using DevZest.Data.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace DevZest.Data.Annotations.Primitives
{
    public abstract class ModelWireupAttribute : Attribute
    {
        private void PerformInitialize(Type modelType, MemberInfo memberInfo)
        {
            Debug.Assert(ModelType == null && modelType != null);
            ModelType = modelType;
        }

        public Type ModelType { get; private set; }

        protected abstract void Initialize(Type modelType, MemberInfo memberInfo);

        protected abstract ModelWireupEvent WireupEvent { get; }

        protected abstract Action<Model> WireupAction { get; }

        private static ConcurrentDictionary<Type, IReadOnlyList<ModelWireupAttribute>> s_attributes = new ConcurrentDictionary<Type, IReadOnlyList<ModelWireupAttribute>>();

        internal static void WireupAttributes(Model model, ModelWireupEvent wireupEvent)
        {
            var attributes = GetOrAddAttributes(model.GetType());
            foreach (var attribute in attributes)
            {
                if (attribute.WireupEvent == wireupEvent)
                    attribute.WireupAction?.Invoke(model);
            }
        }

        private static IReadOnlyList<ModelWireupAttribute> GetOrAddAttributes(Type modelType)
        {
            return s_attributes.GetOrAdd(modelType, ResolveAttributes);
        }

        private static IReadOnlyList<ModelWireupAttribute> ResolveAttributes(Type modelType)
        {
            Debug.Assert(modelType != null);

            List<ModelWireupAttribute> result = null;

            var baseType = modelType.GetTypeInfo().BaseType;
            result = result.Append(baseType != null && typeof(Model).IsAssignableFrom(baseType) ? GetOrAddAttributes(baseType) : Array<ModelWireupAttribute>.Empty);

            result = result.Append(ResolveModelAttributes(modelType));

            foreach (var member in modelType.GetMembers(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                result = result.Append(ResolveModelMemberAttributes(member));

            if (result != null)
                return result;
            else
                return Array<ModelWireupAttribute>.Empty;
        }

        private static IReadOnlyList<ModelWireupAttribute> ResolveModelAttributes(Type modelType)
        {
            var result = modelType.GetTypeInfo().GetCustomAttributes<ModelWireupAttribute>().ToArray();
            for (int i = 0; i < result.Length; i++)
                result[i].PerformInitialize(modelType, null);
            return result;
        }

        private static IReadOnlyList<ModelWireupAttribute> ResolveModelMemberAttributes(MemberInfo member)
        {
            var result = member.GetCustomAttributes<ModelWireupAttribute>(false).ToArray();
            for (int i = 0; i < result.Length; i++)
                result[i].PerformInitialize(member.DeclaringType, member);
            return result;
        }
    }
}
