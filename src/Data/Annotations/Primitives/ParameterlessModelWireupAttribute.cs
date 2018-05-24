using System;
using System.Linq.Expressions;
using System.Reflection;

namespace DevZest.Data.Annotations.Primitives
{
    public abstract class ParameterlessModelWireupAttribute : ModelWireupAttribute
    {
        protected sealed override void Initialize(Type modelType, MemberInfo memberInfo)
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

        private Action<Model> _wireupAction;
        protected sealed override Action<Model> WireupAction
        {
            get { return _wireupAction; }
        }
    }
}
