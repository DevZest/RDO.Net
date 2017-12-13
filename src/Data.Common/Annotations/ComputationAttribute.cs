using DevZest.Data.Annotations.Primitives;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace DevZest.Data.Annotations
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class ComputationAttribute : ModelWireupAttribute
    {
        public bool IsAggregate { get; set; }

        protected override void Initialize(Type modelType, MemberInfo memberInfo)
        {
            _wireupAction = BuildWireupAction(modelType, memberInfo);
        }

        private Action<Model> BuildWireupAction(Type modelType, MemberInfo memberInfo)
        {
            var methodInfo = memberInfo as MethodInfo;
            if (methodInfo == null)
                return null;

            var paramModel = Expression.Parameter(typeof(Model));
            var model = Expression.Convert(paramModel, modelType);
            var call = Expression.Call(model, methodInfo);
            return Expression.Lambda<Action<Model>>(call, paramModel).Compile();
        }

        protected override ModelWireupEvent WireupEvent
        {
            get { return IsAggregate ? ModelWireupEvent.ChildDataSetsCreated : ModelWireupEvent.Constructing; }
        }

        private Action<Model> _wireupAction;
        protected override Action<Model> WireupAction
        {
            get { return _wireupAction; }
        }
    }
}
