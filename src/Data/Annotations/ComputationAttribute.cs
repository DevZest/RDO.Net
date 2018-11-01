using DevZest.Data.Annotations.Primitives;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace DevZest.Data.Annotations
{
    [CrossReference(typeof(_ComputationAttribute))]
    public sealed class ComputationAttribute : NamedModelAttribute
    {
        public ComputationAttribute(string name)
            : base(name)
        {
        }

        public ComputationAttribute(string name, ComputationMode mode)
            : this(name)
        {
            Mode = mode;
        }

        public ComputationMode? Mode { get; }

        private Action<Model> _wireupAction;
        protected sealed override void Initialize()
        {
            var methodInfo = GetMethodInfo(Array.Empty<Type>(), typeof(void));
            _wireupAction = BuildWireupAction(ModelType, methodInfo);
        }

        private Action<Model> BuildWireupAction(Type modelType, MethodInfo methodInfo)
        {
            var paramModel = Expression.Parameter(typeof(Model));
            var model = Expression.Convert(paramModel, modelType);
            var call = Expression.Call(model, methodInfo);
            return Expression.Lambda<Action<Model>>(call, paramModel).Compile();
        }

        protected override void Wireup(Model model)
        {
            _wireupAction(model);
        }

        protected override ModelWireupEvent WireupEvent
        {
            get { return Mode.HasValue ? ModelWireupEvent.ChildDataSetsCreated : ModelWireupEvent.Constructing; }
        }
    }
}
