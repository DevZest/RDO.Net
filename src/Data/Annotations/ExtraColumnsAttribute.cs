using DevZest.Data.Annotations.Primitives;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace DevZest.Data.Annotations
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false)]
    public sealed class ExtraColumnsAttribute : ModelWireupAttribute
    {
        public ExtraColumnsAttribute(Type columnContainerType)
        {
            _columnContainerType = columnContainerType;
        }

        private readonly Type _columnContainerType;

        protected override void Initialize(Type modelType, MemberInfo memberInfo)
        {
            _wireupEvent = memberInfo == null ? ModelWireupEvent.Constructing : ModelWireupEvent.ChildModelsMounted;
            _wireupAction = GetWireupAction(modelType, memberInfo);
        }

        private Action<Model> GetWireupAction(Type modelType, MemberInfo memberInfo)
        {
            if (memberInfo == null)
                return SetExtraColumns;

            var propertyInfo = memberInfo as PropertyInfo;
            if (propertyInfo == null)
                return null;

            var getMethod = propertyInfo.GetGetMethod(true);
            if (getMethod == null)
                return null;

            return BuildWireupAction(modelType, getMethod, (model, type) => model.SetExtraColumns(type), _columnContainerType);
        }

        private static Action<Model> BuildWireupAction(Type modelType, MethodInfo getMethod, Expression<Action<Model, Type>> setExt, Type extType)
        {
            var paramModel = Expression.Parameter(typeof(Model));
            var model = Expression.Convert(paramModel, modelType);
            var childModel = Expression.Call(model, getMethod);
            var call = Expression.Invoke(setExt, childModel, Expression.Constant(extType));
            return Expression.Lambda<Action<Model>>(call, paramModel).Compile();
        }

        private void SetExtraColumns(Model model)
        {
            model.SetExtraColumns(_columnContainerType);
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
