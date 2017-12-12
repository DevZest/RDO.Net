using DevZest.Data.Annotations.Primitives;
using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace DevZest.Data.Annotations
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class CheckAttribute : GeneralValidationModelWireupAttribute
    {
        public CheckAttribute(string message)
            : base(message)
        {
        }

        public string Name { get; set; }

        public string Description { get; set; }

        private Func<Model, _Boolean> _conditionGetter;
        protected override void Initialize(Type modelType, MemberInfo memberInfo)
        {
            var propertyInfo = memberInfo as PropertyInfo;
            if (propertyInfo == null)
                return;

            var getMethod = propertyInfo.GetGetMethod(true);
            if (getMethod == null)
                return;

            _conditionGetter = GetConditionGetter(modelType, getMethod);
        }

        private static Func<Model, _Boolean> GetConditionGetter(Type modelType, MethodInfo getMethod)
        {
            var paramModel = Expression.Parameter(typeof(Model));
            var model = Expression.Convert(paramModel, modelType);
            var call = Expression.Call(model, getMethod);
            return Expression.Lambda<Func<Model, _Boolean>>(call, paramModel).Compile();
        }

        protected override ModelWireupEvent WireupEvent
        {
            get { return ModelWireupEvent.Initializing; }
        }

        protected override Action<Model> WireupAction
        {
            get { return Wireup; }
        }

        private ConditionalWeakTable<Model, _Boolean> _conditions = new ConditionalWeakTable<Model, _Boolean>();
        private _Boolean GetCondition(Model model)
        {
            return _conditions.GetValue(model, GetConditionFromGetter);
        }

        private _Boolean GetConditionFromGetter(Model model)
        {
            return _conditionGetter == null ? null : _conditionGetter(model);
        }

        protected override bool IsValid(Model model, DataRow dataRow)
        {
            var condition = GetCondition(model);
            return condition[dataRow] != false;
        }

        protected override IColumns GetValidationSource(Model model)
        {
            var condition = GetCondition(model);
            var expression = condition.Expression;
            return expression == null ? Columns.Empty : expression.BaseColumns;
        }

        private void Wireup(Model model)
        {
            var condition = GetCondition(model);
            if (condition != null)
            {
                model.DbCheck(Name, Description, condition);
                AddValidator(model);
            }
        }
    }
}
