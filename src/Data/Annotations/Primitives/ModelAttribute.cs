using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace DevZest.Data.Annotations.Primitives
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public abstract class ModelAttribute : Attribute
    {
        private void PerformInitialize(Type modelType)
        {
            Debug.Assert(ModelType == null && modelType != null);
            ModelType = modelType;
            Initialize();
        }

        public Type ModelType { get; private set; }

        protected abstract void Initialize();

        protected abstract ModelWireupEvent WireupEvent { get; }

        protected abstract void Wireup(Model model);

        private static ConcurrentDictionary<Type, IReadOnlyList<ModelAttribute>> s_attributes = new ConcurrentDictionary<Type, IReadOnlyList<ModelAttribute>>();

        internal static void WireupAttributes(Model model, ModelWireupEvent wireupEvent)
        {
            var attributes = GetOrAddAttributes(model.GetType());
            foreach (var attribute in attributes)
            {
                if (attribute.WireupEvent == wireupEvent)
                    attribute.Wireup(model);
            }
        }

        private static IReadOnlyList<ModelAttribute> GetOrAddAttributes(Type modelType)
        {
            return s_attributes.GetOrAdd(modelType, ResolveAttributes);
        }

        private static IReadOnlyList<ModelAttribute> ResolveAttributes(Type modelType)
        {
            Debug.Assert(modelType != null);

            List<ModelAttribute> result = null;

            var baseType = modelType.GetTypeInfo().BaseType;
            result = result.Append(baseType != null && typeof(Model).IsAssignableFrom(baseType) && baseType != typeof(Model) ? GetOrAddAttributes(baseType) : Array.Empty<ModelAttribute>());
            result = result.Append(ResolveModelAttributes(modelType));

            if (result != null)
                return result;
            else
                return Array.Empty<ModelAttribute>();
        }

        private static IReadOnlyList<ModelAttribute> ResolveModelAttributes(Type modelType)
        {
            var result = modelType.GetTypeInfo().GetCustomAttributes<ModelAttribute>(false).ToArray();
            for (int i = 0; i < result.Length; i++)
                result[i].PerformInitialize(modelType);
            return result;
        }
    }
}
