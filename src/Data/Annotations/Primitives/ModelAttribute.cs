using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace DevZest.Data.Annotations.Primitives
{
    /// <summary>
    /// Base class for model level attribute, which can be wired up during <see cref="Model"/> object creation and initialization.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public abstract class ModelAttribute : Attribute
    {
        private void PerformInitialize(Type modelType)
        {
            Debug.Assert(ModelType == null && modelType != null);
            ModelType = modelType;
            Initialize();
        }

        /// <summary>
        /// Gets the type of the model.
        /// </summary>
        public Type ModelType { get; private set; }

        /// <summary>
        /// Initializes this attribute.
        /// </summary>
        protected abstract void Initialize();

        /// <summary>
        /// Gets a value indicates when this attribute should be wired up with the model.
        /// </summary>
        protected abstract ModelWireupEvent WireupEvent { get; }

        /// <summary>
        /// Wires up this attribute with the model.
        /// </summary>
        /// <param name="model">The model.</param>
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
