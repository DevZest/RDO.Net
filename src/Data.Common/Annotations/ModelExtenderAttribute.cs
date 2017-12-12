using DevZest.Data.Annotations.Primitives;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace DevZest.Data.Annotations
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false)]
    public sealed class ModelExtenderAttribute : ModelWireupAttribute
    {
        public ModelExtenderAttribute(Type extenderType)
        {
            _extenderType = extenderType;
        }

        private readonly Type _extenderType;

        protected override void Initialize(Type modelType, MemberInfo memberInfo)
        {
            _wireupEvent = memberInfo == null ? ModelWireupEvent.Constructing : ModelWireupEvent.ChildModelsMounted;
            _wireupAction = GetWireupAction(modelType, memberInfo);
        }

        private Action<Model> GetWireupAction(Type modelType, MemberInfo memberInfo)
        {
            if (memberInfo == null)
                return SetExtender;

            var propertyInfo = memberInfo as PropertyInfo;
            if (propertyInfo == null)
                return null;

            var getMethod = propertyInfo.GetGetMethod(true);
            if (getMethod == null)
                return null;

            return BuildWireupAction(modelType, getMethod, (model, type) => model.SetExtender(type), _extenderType);
        }

        private static Action<Model> BuildWireupAction(Type modelType, MethodInfo getMethod, Expression<Action<Model, Type>> setExtender, Type extenderType)
        {
            var paramModel = Expression.Parameter(typeof(Model));
            var model = Expression.Convert(paramModel, modelType);
            var childModel = Expression.Call(model, getMethod);
            var call = Expression.Invoke(setExtender, childModel, Expression.Constant(extenderType));
            return Expression.Lambda<Action<Model>>(call, paramModel).Compile();
        }

        private void SetExtender(Model model)
        {
            model.SetExtender(_extenderType);
        }

        private ModelWireupEvent _wireupEvent;
        protected override ModelWireupEvent WireupEvent
        {
            get { return _wireupEvent; }
        }

        private Action<Model> _wireupAction;
        protected override Action<Model> WireupAction
        {
            get { return _wireupAction; }
        }

    }
}
