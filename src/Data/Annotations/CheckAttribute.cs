using DevZest.Data.Annotations.Primitives;
using DevZest.Data.Utilities;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace DevZest.Data.Annotations
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class CheckAttribute : ValidationModelWireupAttribute
    {
        public CheckAttribute(string message)
        {
            message.VerifyNotEmpty(nameof(message));
            _message = message;
        }

        public CheckAttribute(Type messageResourceType, string message)
        {
            messageResourceType.VerifyNotNull(nameof(messageResourceType));
            _messageResourceType = messageResourceType;
            _messageGetter = messageResourceType.ResolveStringGetter(message);
        }

        protected sealed override DataValidationError Validate(Model model, DataRow dataRow)
        {
            return IsValid(model, dataRow) ? null : new DataValidationError(MessageString, GetValidationSource(model));
        }

        private readonly string _message;
        public string Message
        {
            get { return _message; }
        }

        private readonly Type _messageResourceType;
        public Type ResourceType
        {
            get { return _messageResourceType; }
        }

        private readonly Func<string> _messageGetter;

        private string MessageString
        {
            get { return _messageGetter != null ? _messageGetter() : _message; }
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

            _conditionGetter = BuildConditionGetter(modelType, getMethod);
        }

        private static Func<Model, _Boolean> BuildConditionGetter(Type modelType, MethodInfo getMethod)
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

        private _Boolean GetCondition(Model model)
        {
            return _conditionGetter == null ? null : _conditionGetter(model);
        }

        private bool IsValid(Model model, DataRow dataRow)
        {
            var condition = GetCondition(model);
            return condition[dataRow] != false;
        }

        private IColumns GetValidationSource(Model model)
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
